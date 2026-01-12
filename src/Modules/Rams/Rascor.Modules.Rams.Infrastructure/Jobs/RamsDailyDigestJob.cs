using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.Modules.Rams.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job that sends daily RAMS digest emails
/// </summary>
public class RamsDailyDigestJob
{
    private readonly IRamsNotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RamsDailyDigestJob> _logger;

    public RamsDailyDigestJob(
        IRamsNotificationService notificationService,
        IConfiguration configuration,
        ILogger<RamsDailyDigestJob> logger)
    {
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Executes the daily digest job
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Check if digest is enabled
        var enabled = _configuration.GetValue<bool>("RamsNotifications:Enabled", true) &&
                      _configuration.GetValue<bool>("RamsNotifications:DailyDigestEnabled", true);

        if (!enabled)
        {
            _logger.LogInformation("RAMS daily digest is disabled, skipping execution");
            return;
        }

        _logger.LogInformation("Starting RAMS daily digest job");

        try
        {
            await _notificationService.SendDailyDigestAsync(cancellationToken);
            _logger.LogInformation("RAMS daily digest job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAMS daily digest job failed");
            throw; // Re-throw to let Hangfire handle retries
        }
    }
}
