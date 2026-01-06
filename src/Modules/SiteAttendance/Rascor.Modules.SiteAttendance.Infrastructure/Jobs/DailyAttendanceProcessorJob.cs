using Hangfire;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.SiteAttendance.Application.Services;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job for processing daily attendance records.
/// Runs nightly to process and summarize the previous day's attendance events.
/// </summary>
public class DailyAttendanceProcessorJob
{
    private readonly ITimeCalculationService _timeCalculationService;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<DailyAttendanceProcessorJob> _logger;

    public DailyAttendanceProcessorJob(
        ITimeCalculationService timeCalculationService,
        ITenantRepository tenantRepository,
        ILogger<DailyAttendanceProcessorJob> logger)
    {
        _timeCalculationService = timeCalculationService;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the daily attendance processing job.
    /// Processes attendance events from yesterday for all active tenants.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting daily attendance processing for yesterday");

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Process for all active tenants
        var tenants = await _tenantRepository.GetAllActiveAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            try
            {
                _logger.LogInformation(
                    "Processing attendance for tenant {TenantId} ({TenantName}) for date {Date}",
                    tenant.Id,
                    tenant.Name,
                    yesterday);

                await _timeCalculationService.ProcessDailyAttendanceAsync(tenant.Id, yesterday, cancellationToken);

                _logger.LogInformation(
                    "Completed processing attendance for tenant {TenantId}",
                    tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing attendance for tenant {TenantId} ({TenantName})",
                    tenant.Id,
                    tenant.Name);
                // Continue processing other tenants even if one fails
            }
        }

        _logger.LogInformation("Daily attendance processing completed");
    }
}
