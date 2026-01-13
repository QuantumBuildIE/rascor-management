using Rascor.Core.Domain.Common;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Tracks synchronization progress and history from the mobile geofence database.
/// Used to ensure incremental syncing and provide audit trail.
/// </summary>
public class GeofenceSyncLog : TenantEntity
{
    /// <summary>
    /// When the sync operation started
    /// </summary>
    public DateTime SyncStarted { get; private set; }

    /// <summary>
    /// When the sync operation completed (null if still running or failed)
    /// </summary>
    public DateTime? SyncCompleted { get; private set; }

    /// <summary>
    /// Total number of records processed during this sync
    /// </summary>
    public int RecordsProcessed { get; private set; }

    /// <summary>
    /// Number of new records created in Rascor
    /// </summary>
    public int RecordsCreated { get; private set; }

    /// <summary>
    /// Number of records skipped (duplicates or invalid)
    /// </summary>
    public int RecordsSkipped { get; private set; }

    /// <summary>
    /// The last event ID processed from the mobile database (for incremental sync)
    /// </summary>
    public string? LastEventId { get; private set; }

    /// <summary>
    /// The timestamp of the last event processed (for incremental sync)
    /// </summary>
    public DateTime? LastEventTimestamp { get; private set; }

    /// <summary>
    /// Error message if the sync failed
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Whether the sync completed successfully
    /// </summary>
    public bool IsSuccess => SyncCompleted.HasValue && string.IsNullOrEmpty(ErrorMessage);

    private GeofenceSyncLog() { } // EF Core

    /// <summary>
    /// Creates a new sync log entry when starting a sync operation
    /// </summary>
    public static GeofenceSyncLog Start(Guid tenantId)
    {
        return new GeofenceSyncLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SyncStarted = DateTime.UtcNow,
            RecordsProcessed = 0,
            RecordsCreated = 0,
            RecordsSkipped = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates progress during the sync operation
    /// </summary>
    public void UpdateProgress(int processed, int created, int skipped, string? lastEventId, DateTime? lastEventTimestamp)
    {
        RecordsProcessed = processed;
        RecordsCreated = created;
        RecordsSkipped = skipped;
        LastEventId = lastEventId;
        LastEventTimestamp = lastEventTimestamp;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the sync as completed successfully
    /// </summary>
    public void Complete()
    {
        SyncCompleted = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the sync as failed with an error message
    /// </summary>
    public void Fail(string errorMessage)
    {
        SyncCompleted = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }
}
