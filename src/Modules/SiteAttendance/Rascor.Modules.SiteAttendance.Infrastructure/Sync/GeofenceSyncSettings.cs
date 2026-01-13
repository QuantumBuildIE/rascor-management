namespace Rascor.Modules.SiteAttendance.Infrastructure.Sync;

/// <summary>
/// Configuration settings for the geofence sync service
/// </summary>
public class GeofenceSyncSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "GeofenceSync";

    /// <summary>
    /// Whether the sync service is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Interval in minutes between sync operations
    /// </summary>
    public int IntervalMinutes { get; set; } = 15;

    /// <summary>
    /// Default tenant ID for sync operations
    /// </summary>
    public string DefaultTenantId { get; set; } = "11111111-1111-1111-1111-111111111111";

    /// <summary>
    /// Maximum number of events to process in a single sync batch
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Number of days to look back for events on initial sync (when no previous sync exists)
    /// </summary>
    public int InitialSyncDays { get; set; } = 30;

    /// <summary>
    /// Whether to process attendance summaries immediately after syncing events.
    /// When enabled, summaries are updated within 15 minutes of events being synced.
    /// When disabled, summaries are only updated by the nightly DailyAttendanceProcessorJob.
    /// </summary>
    public bool ProcessSummariesAfterSync { get; set; } = true;

    /// <summary>
    /// Gets the default tenant ID as a Guid
    /// </summary>
    public Guid GetDefaultTenantGuid() => Guid.Parse(DefaultTenantId);
}
