using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Core.Application.Abstractions.Email;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Data;
using Rascor.Core.Infrastructure.Float.Models;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Service for checking Float schedules against SPA submissions and sending reminder notifications.
/// </summary>
public class FloatSpaCheckService : IFloatSpaCheckService
{
    private readonly IFloatApiClient _floatApiClient;
    private readonly ApplicationDbContext _appDbContext;
    private readonly SiteAttendanceDbContext _siteAttendanceDbContext;
    private readonly IEmailProvider _emailProvider;
    private readonly IFloatSpaEmailTemplateService _emailTemplateService;
    private readonly ILogger<FloatSpaCheckService> _logger;
    private readonly FloatSettings _floatSettings;
    private readonly EmailSettings _emailSettings;

    public FloatSpaCheckService(
        IFloatApiClient floatApiClient,
        ApplicationDbContext appDbContext,
        SiteAttendanceDbContext siteAttendanceDbContext,
        IEmailProvider emailProvider,
        IFloatSpaEmailTemplateService emailTemplateService,
        ILogger<FloatSpaCheckService> logger,
        IOptions<FloatSettings> floatSettings,
        IOptions<EmailSettings> emailSettings)
    {
        _floatApiClient = floatApiClient;
        _appDbContext = appDbContext;
        _siteAttendanceDbContext = siteAttendanceDbContext;
        _emailProvider = emailProvider;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
        _floatSettings = floatSettings.Value;
        _emailSettings = emailSettings.Value;
    }

    /// <inheritdoc />
    public async Task<SpaCheckResult> RunSpaCheckAsync(
        Guid tenantId,
        DateOnly? date = null,
        CancellationToken ct = default)
    {
        var checkDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var stopwatch = Stopwatch.StartNew();

        var result = new SpaCheckResult
        {
            CheckDate = checkDate
        };

        _logger.LogInformation(
            "Starting Float SPA check for tenant {TenantId}, date {Date}",
            tenantId, checkDate);

        try
        {
            // Check if Float API is configured
            if (!_floatApiClient.IsConfigured)
            {
                _logger.LogWarning("Float API is not configured. Skipping SPA check for tenant {TenantId}", tenantId);
                result.Errors.Add("Float API is not configured");
                return result;
            }

            // 1. Get today's tasks from Float
            var floatTasks = await _floatApiClient.GetTasksForDateAsync(checkDate, ct);
            result.TotalScheduledTasks = floatTasks.Count;

            if (floatTasks.Count == 0)
            {
                _logger.LogInformation("No Float tasks scheduled for {Date}", checkDate);
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // 2. Get all Float people and projects for matching
            var floatPeople = await _floatApiClient.GetPeopleAsync(ct);
            var floatProjects = await _floatApiClient.GetProjectsAsync(ct);

            var peopleDict = floatPeople
                .Where(p => p.PeopleId.HasValue)
                .ToDictionary(p => p.PeopleId!.Value);
            var projectsDict = floatProjects
                .Where(p => p.ProjectId.HasValue)
                .ToDictionary(p => p.ProjectId!.Value);

            // 3. Get employees and sites with Float links for this tenant
            var employees = await _appDbContext.Employees
                .IgnoreQueryFilters()
                .Where(e => e.TenantId == tenantId && !e.IsDeleted && e.FloatPersonId != null)
                .ToListAsync(ct);

            var sites = await _appDbContext.Sites
                .IgnoreQueryFilters()
                .Where(s => s.TenantId == tenantId && !s.IsDeleted && s.FloatProjectId != null)
                .ToListAsync(ct);

            var employeesByFloatId = employees.ToDictionary(e => e.FloatPersonId!.Value);
            var sitesByFloatId = sites.ToDictionary(s => s.FloatProjectId!.Value);

            // Track unique employees checked (an employee might have multiple tasks)
            var employeesChecked = new HashSet<Guid>();

            // 4. Process each task
            foreach (var task in floatTasks)
            {
                try
                {
                    await ProcessFloatTaskAsync(
                        tenantId,
                        task,
                        checkDate,
                        peopleDict,
                        projectsDict,
                        employeesByFloatId,
                        sitesByFloatId,
                        employeesChecked,
                        result,
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing Float task {TaskId} for tenant {TenantId}",
                        task.TaskId, tenantId);
                    result.Errors.Add($"Task {task.TaskId}: {ex.Message}");
                }
            }

            result.EmployeesChecked = employeesChecked.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Float SPA check failed for tenant {TenantId}", tenantId);
            result.Errors.Add($"Fatal error: {ex.Message}");
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;

        _logger.LogInformation(
            "Float SPA check completed for tenant {TenantId}: " +
            "Tasks={Tasks}, Checked={Checked}, Submitted={Submitted}, Missing={Missing}, " +
            "RemindersSent={Sent}, Failed={Failed}, Duration={Duration}ms",
            tenantId, result.TotalScheduledTasks, result.EmployeesChecked,
            result.SpaSubmitted, result.SpaMissing, result.RemindersSent,
            result.RemindersFailed, stopwatch.ElapsedMilliseconds);

        return result;
    }

    /// <summary>
    /// Process a single Float task and send reminder if SPA is missing.
    /// </summary>
    private async Task ProcessFloatTaskAsync(
        Guid tenantId,
        FloatTask task,
        DateOnly checkDate,
        Dictionary<int, FloatPerson> peopleDict,
        Dictionary<int, FloatProject> projectsDict,
        Dictionary<int, Employee> employeesByFloatId,
        Dictionary<int, Site> sitesByFloatId,
        HashSet<Guid> employeesChecked,
        SpaCheckResult result,
        CancellationToken ct)
    {
        // Skip tasks without ProjectId
        if (task.ProjectId is null)
        {
            _logger.LogDebug("Skipping task {TaskId} - missing ProjectId", task.TaskId);
            return;
        }

        // Collect all person IDs from both PeopleId (singular) and PeopleIds (plural)
        var personIds = new List<int>();
        if (task.PeopleId.HasValue)
            personIds.Add(task.PeopleId.Value);
        if (task.PeopleIds != null)
            personIds.AddRange(task.PeopleIds);

        if (personIds.Count == 0)
        {
            _logger.LogDebug("Skipping task {TaskId} - no PeopleId or PeopleIds", task.TaskId);
            return;
        }

        // Process each person assigned to this task
        foreach (var personId in personIds.Distinct())
        {
            // 1. Find matched employee
            if (!employeesByFloatId.TryGetValue(personId, out var employee))
            {
                // Try to get Float person name for logging
                var personName = peopleDict.TryGetValue(personId, out var fp) ? fp.Name : $"ID:{personId}";
                _logger.LogDebug("Skipping task - Float person {PersonName} not linked to any employee", personName);
                result.SkippedUnmatchedPeople++;
                continue;
            }

            // Process this employee for the task
            await ProcessEmployeeForTaskAsync(
                tenantId, task, employee, checkDate, projectsDict,
                sitesByFloatId, employeesChecked, result, ct);
        }
    }

    /// <summary>
    /// Process a single employee for a Float task and send reminder if SPA is missing.
    /// </summary>
    private async Task ProcessEmployeeForTaskAsync(
        Guid tenantId,
        FloatTask task,
        Employee employee,
        DateOnly checkDate,
        Dictionary<int, FloatProject> projectsDict,
        Dictionary<int, Site> sitesByFloatId,
        HashSet<Guid> employeesChecked,
        SpaCheckResult result,
        CancellationToken ct)
    {

        // 2. Find matched site
        if (!sitesByFloatId.TryGetValue(task.ProjectId.Value, out var site))
        {
            var projectName = projectsDict.TryGetValue(task.ProjectId.Value, out var proj) ? proj.Name : $"ID:{task.ProjectId}";
            _logger.LogDebug("Skipping task - Float project {ProjectName} not linked to any site", projectName);
            result.SkippedUnmatchedProjects++;
            return;
        }

        // Track that we checked this employee
        employeesChecked.Add(employee.Id);

        // 3. Check if employee has email
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            _logger.LogWarning(
                "Employee {EmployeeId} ({EmployeeName}) has no email - cannot send reminder",
                employee.Id, employee.FullName);
            result.SkippedNoEmail++;
            return;
        }

        // 4. Check if SPA already submitted
        var hasSubmitted = await HasSubmittedSpaAsync(tenantId, employee.Id, site.Id, checkDate, ct);

        if (hasSubmitted)
        {
            _logger.LogDebug(
                "SPA already submitted by {EmployeeName} for {SiteName} on {Date}",
                employee.FullName, site.SiteName, checkDate);
            result.SpaSubmitted++;
            return;
        }

        result.SpaMissing++;

        // 5. Check if we already sent a notification today for this employee/site
        var alreadyNotified = await _appDbContext.SpaNotificationAudits
            .IgnoreQueryFilters()
            .AnyAsync(n => n.TenantId == tenantId
                && n.EmployeeId == employee.Id
                && n.SiteId == site.Id
                && n.ScheduledDate == checkDate
                && n.Status == "Sent"
                && !n.IsDeleted, ct);

        if (alreadyNotified)
        {
            _logger.LogDebug(
                "Already sent SPA reminder to {EmployeeName} for {SiteName} on {Date}",
                employee.FullName, site.SiteName, checkDate);
            result.SkippedAlreadyNotified++;
            return;
        }

        // 6. Send reminder
        try
        {
            var audit = await SendSpaReminderAsync(tenantId, employee.Id, site.Id, checkDate, task, ct);

            if (audit.Status == "Sent")
            {
                result.RemindersSent++;
            }
            else
            {
                result.RemindersFailed++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send SPA reminder to {EmployeeName} for {SiteName}",
                employee.FullName, site.SiteName);
            result.RemindersFailed++;
            result.Errors.Add($"Reminder failed for {employee.FullName}: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasSubmittedSpaAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        CancellationToken ct = default)
    {
        // Query the SitePhotoAttendances table in the site_attendance schema
        var submitted = await _siteAttendanceDbContext.SitePhotoAttendances
            .AnyAsync(spa => spa.TenantId == tenantId
                && spa.EmployeeId == employeeId
                && spa.SiteId == siteId
                && spa.EventDate == date
                && !spa.IsDeleted, ct);

        return submitted;
    }

    /// <inheritdoc />
    public async Task<SpaNotificationAudit> SendSpaReminderAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly scheduledDate,
        FloatTask? floatTask,
        CancellationToken ct = default)
    {
        var employee = await _appDbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.TenantId == tenantId && !e.IsDeleted, ct)
            ?? throw new InvalidOperationException($"Employee {employeeId} not found");

        var site = await _appDbContext.Sites
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == siteId && s.TenantId == tenantId && !s.IsDeleted, ct)
            ?? throw new InvalidOperationException($"Site {siteId} not found");

        // Create audit record first
        var audit = new SpaNotificationAudit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            FloatTaskId = floatTask?.TaskId,
            FloatPersonId = employee.FloatPersonId,
            FloatProjectId = site.FloatProjectId,
            ScheduledDate = scheduledDate,
            ScheduledHours = floatTask?.HoursParsed,
            NotificationType = "FloatSpaReminder",
            NotificationMethod = "Email",
            RecipientEmail = employee.Email,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "FloatSpaCheckJob"
        };

        await _appDbContext.SpaNotificationAudits.AddAsync(audit, ct);
        await _appDbContext.SaveChangesAsync(ct);

        try
        {
            // Check email provider is configured
            if (!_emailProvider.IsConfigured)
            {
                audit.Status = "Failed";
                audit.ErrorMessage = "Email provider is not configured";
                await _appDbContext.SaveChangesAsync(ct);

                _logger.LogWarning(
                    "Email provider not configured. Cannot send SPA reminder to {Email}",
                    employee.Email);
                return audit;
            }

            // Build SPA submission URL with employee and site context
            var spaSubmissionUrl = BuildSpaSubmissionUrl(employeeId, siteId, scheduledDate);

            // Generate email content
            var subject = _emailTemplateService.GenerateReminderEmailSubject(site, scheduledDate);
            var htmlBody = _emailTemplateService.GenerateReminderEmailHtml(employee, site, scheduledDate, spaSubmissionUrl);
            var plainTextBody = _emailTemplateService.GenerateReminderEmailPlainText(employee, site, scheduledDate, spaSubmissionUrl);

            var emailMessage = new EmailMessage
            {
                ToEmail = employee.Email!,
                ToName = employee.FullName,
                Subject = subject,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody
            };

            // Send email
            var emailResult = await _emailProvider.SendAsync(emailMessage, ct);

            if (emailResult.Success)
            {
                audit.Status = "Sent";
                audit.SentAt = DateTime.UtcNow;
                audit.EmailProviderId = emailResult.MessageId;

                _logger.LogInformation(
                    "SPA reminder sent to {Email} for site {SiteName} on {Date}. MessageId: {MessageId}",
                    employee.Email, site.SiteName, scheduledDate, emailResult.MessageId);
            }
            else
            {
                audit.Status = "Failed";
                audit.ErrorMessage = emailResult.ErrorMessage;

                _logger.LogWarning(
                    "Failed to send SPA reminder to {Email}: {Error}",
                    employee.Email, emailResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            audit.Status = "Failed";
            audit.ErrorMessage = ex.Message;

            _logger.LogError(ex,
                "Exception sending SPA reminder to {Email}",
                employee.Email);
        }

        await _appDbContext.SaveChangesAsync(ct);

        return audit;
    }

    /// <summary>
    /// Builds the SPA submission URL with employee and site context.
    /// </summary>
    private string BuildSpaSubmissionUrl(Guid employeeId, Guid siteId, DateOnly scheduledDate)
    {
        var baseUrl = _emailSettings.SpaSubmissionBaseUrl.TrimEnd('/');
        return $"{baseUrl}?employeeId={employeeId}&siteId={siteId}&date={scheduledDate:yyyy-MM-dd}";
    }
}
