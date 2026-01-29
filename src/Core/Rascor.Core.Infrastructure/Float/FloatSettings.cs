namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Configuration settings for Float API integration.
/// Used for fetching scheduled tasks, people, and projects from Float.com.
/// </summary>
public class FloatSettings
{
    public const string SectionName = "Float";

    /// <summary>
    /// Float API key for authentication.
    /// Get this from Float Settings > Integrations > API.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Float API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.float.com/v3";

    /// <summary>
    /// User-Agent header for API requests.
    /// Float requires a descriptive User-Agent.
    /// </summary>
    public string UserAgent { get; set; } = "RASCOR-Integration/1.0";

    /// <summary>
    /// Whether Float integration is enabled.
    /// When disabled, API calls will return empty results.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Cron expression for the SPA check job.
    /// Default: "0 10 * * *" (10:00 AM daily)
    /// </summary>
    public string SpaCheckCronExpression { get; set; } = "0 10 * * *";

    /// <summary>
    /// Grace period in minutes after scheduled start time before sending SPA reminder.
    /// Default: 60 minutes (1 hour)
    /// </summary>
    public int SpaCheckGracePeriodMinutes { get; set; } = 60;
}
