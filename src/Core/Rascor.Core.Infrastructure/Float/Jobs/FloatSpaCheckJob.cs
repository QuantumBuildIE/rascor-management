using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Core.Application.Interfaces;

namespace Rascor.Core.Infrastructure.Float.Jobs;

/// <summary>
/// Hangfire background job for checking Float scheduled tasks against SPA submissions.
/// Runs at a configurable time (default 10:00 AM) to send SPA reminders to employees
/// who are scheduled to work but haven't submitted their Site Photo Attendance.
/// </summary>
public class FloatSpaCheckJob
{
    private readonly IFloatSpaCheckService _spaCheckService;
    private readonly ITenantRepository _tenantRepository;
    private readonly FloatSettings _floatSettings;
    private readonly ILogger<FloatSpaCheckJob> _logger;

    public FloatSpaCheckJob(
        IFloatSpaCheckService spaCheckService,
        ITenantRepository tenantRepository,
        IOptions<FloatSettings> floatSettings,
        ILogger<FloatSpaCheckJob> logger)
    {
        _spaCheckService = spaCheckService;
        _tenantRepository = tenantRepository;
        _floatSettings = floatSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes the Float SPA check job for all active tenants.
    /// Checks today's scheduled Float tasks and sends reminders for missing SPA submissions.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Float SPA check job");

        // Check if Float integration is enabled
        if (!_floatSettings.Enabled)
        {
            _logger.LogInformation("Float integration is disabled. Skipping SPA check.");
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tenants = await _tenantRepository.GetAllActiveAsync(cancellationToken);
        var tenantList = tenants.ToList();

        _logger.LogInformation(
            "Processing Float SPA check for {TenantCount} tenant(s) for date {Date}",
            tenantList.Count,
            today);

        var successCount = 0;
        var failCount = 0;
        var totalReminders = 0;
        var totalMissing = 0;

        foreach (var tenant in tenantList)
        {
            try
            {
                _logger.LogInformation(
                    "Processing Float SPA check for tenant {TenantId} ({TenantName})",
                    tenant.Id,
                    tenant.Name);

                var result = await _spaCheckService.RunSpaCheckAsync(tenant.Id, today, cancellationToken);

                _logger.LogInformation(
                    "Float SPA check completed for tenant {TenantId}: " +
                    "Tasks={Tasks}, Submitted={Submitted}, Missing={Missing}, " +
                    "RemindersSent={Sent}, Failed={Failed}",
                    tenant.Id,
                    result.TotalScheduledTasks,
                    result.SpaSubmitted,
                    result.SpaMissing,
                    result.RemindersSent,
                    result.RemindersFailed);

                successCount++;
                totalReminders += result.RemindersSent;
                totalMissing += result.SpaMissing;

                if (result.Errors.Any())
                {
                    _logger.LogWarning(
                        "Float SPA check for tenant {TenantId} had {ErrorCount} errors: {Errors}",
                        tenant.Id,
                        result.Errors.Count,
                        string.Join("; ", result.Errors.Take(5)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing Float SPA check for tenant {TenantId} ({TenantName})",
                    tenant.Id,
                    tenant.Name);
                failCount++;
                // Continue processing other tenants even if one fails
            }
        }

        _logger.LogInformation(
            "Float SPA check job completed: " +
            "Tenants processed={Success}, Failed={Failed}, " +
            "Total reminders sent={Reminders}, Total missing SPA={Missing}",
            successCount,
            failCount,
            totalReminders,
            totalMissing);
    }
}
