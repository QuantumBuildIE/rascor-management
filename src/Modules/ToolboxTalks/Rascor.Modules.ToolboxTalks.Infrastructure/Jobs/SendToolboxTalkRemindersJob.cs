using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job for sending reminder notifications for overdue toolbox talks.
/// Runs daily at 8:00 AM to notify employees and escalate to managers when thresholds are reached.
/// </summary>
public class SendToolboxTalkRemindersJob
{
    private readonly IToolboxTalksDbContext _dbContext;
    private readonly ICoreDbContext _coreDbContext;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<SendToolboxTalkRemindersJob> _logger;

    public SendToolboxTalkRemindersJob(
        IToolboxTalksDbContext dbContext,
        ICoreDbContext coreDbContext,
        ITenantRepository tenantRepository,
        ILogger<SendToolboxTalkRemindersJob> logger)
    {
        _dbContext = dbContext;
        _coreDbContext = coreDbContext;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the reminder sending job.
    /// Finds overdue talks and sends reminders, escalating to managers when necessary.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting SendToolboxTalkRemindersJob");

        var today = DateTime.UtcNow.Date;
        var remindersSent = 0;
        var escalationsSent = 0;
        var errorCount = 0;

        // Get all active tenants
        var tenants = await _tenantRepository.GetAllActiveAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            try
            {
                // Get tenant settings
                var settings = await _dbContext.ToolboxTalkSettings
                    .FirstOrDefaultAsync(s => s.TenantId == tenant.Id, cancellationToken);

                var maxReminders = settings?.MaxReminders ?? 5;
                var escalateAfterReminders = settings?.EscalateAfterReminders ?? 3;

                // Find overdue incomplete talks that haven't reached max reminders
                var overdueTalks = await _dbContext.ScheduledTalks
                    .Include(st => st.Employee)
                    .Include(st => st.ToolboxTalk)
                    .Where(st => st.TenantId == tenant.Id)
                    .Where(st => st.Status != ScheduledTalkStatus.Completed &&
                                 st.Status != ScheduledTalkStatus.Cancelled)
                    .Where(st => st.DueDate.Date < today)
                    .Where(st => st.RemindersSent < maxReminders)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Found {Count} overdue talks requiring reminders for tenant {TenantId}",
                    overdueTalks.Count,
                    tenant.Id);

                foreach (var talk in overdueTalks)
                {
                    try
                    {
                        // Increment reminders
                        talk.RemindersSent++;
                        talk.LastReminderAt = DateTime.UtcNow;

                        // Update status to Overdue if not already
                        if (talk.Status != ScheduledTalkStatus.Overdue)
                        {
                            talk.Status = ScheduledTalkStatus.Overdue;
                        }

                        // TODO: Send reminder email to employee
                        // await _emailService.SendReminderEmailAsync(talk, talk.Employee, talk.RemindersSent);
                        _logger.LogInformation(
                            "Reminder {ReminderNum} queued for ScheduledTalk {TalkId}, Employee: {EmployeeId}",
                            talk.RemindersSent,
                            talk.Id,
                            talk.EmployeeId);

                        remindersSent++;

                        // Check if escalation is needed
                        if (talk.RemindersSent >= escalateAfterReminders)
                        {
                            // TODO: Implement escalation when Employee.ManagerId is added to the domain model
                            // Currently the Employee entity doesn't have a ManagerId property
                            // When it's added, uncomment and adapt the following code:
                            // var employee = await _coreDbContext.Employees
                            //     .FirstOrDefaultAsync(e => e.Id == talk.EmployeeId, cancellationToken);
                            // if (employee?.ManagerId != null)
                            // {
                            //     var manager = await _coreDbContext.Employees
                            //         .FirstOrDefaultAsync(e => e.Id == employee.ManagerId, cancellationToken);
                            //     if (manager != null)
                            //     {
                            //         await _emailService.SendEscalationEmailAsync(talk, employee, manager);
                            //         escalationsSent++;
                            //     }
                            // }

                            _logger.LogWarning(
                                "Escalation threshold reached for ScheduledTalk {TalkId}, but manager escalation is not yet implemented",
                                talk.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error sending reminder for ScheduledTalk {TalkId}",
                            talk.Id);
                        errorCount++;
                        // Continue processing other talks
                    }
                }

                // Save all changes for this tenant
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing reminders for tenant {TenantId} ({TenantName})",
                    tenant.Id,
                    tenant.Name);
                // Continue processing other tenants
            }
        }

        _logger.LogInformation(
            "Completed SendToolboxTalkRemindersJob. Reminders: {RemindersSent}, Escalations: {EscalationsSent}, Errors: {ErrorCount}",
            remindersSent,
            escalationsSent,
            errorCount);
    }
}
