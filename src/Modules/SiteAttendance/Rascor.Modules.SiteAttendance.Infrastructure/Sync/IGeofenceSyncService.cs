using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Sync;

/// <summary>
/// Service interface for synchronizing geofence events from the mobile database to Rascor.
/// </summary>
public interface IGeofenceSyncService
{
    /// <summary>
    /// Executes a sync operation for the specified tenant
    /// </summary>
    /// <param name="tenantId">The tenant to sync events for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result summary</returns>
    Task<GeofenceSyncResult> SyncAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sync status including recent sync history
    /// </summary>
    /// <param name="tenantId">The tenant to get status for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync status with recent history</returns>
    Task<GeofenceSyncStatusDto> GetSyncStatusAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of device IDs from the mobile database that don't have matching employees
    /// </summary>
    /// <param name="tenantId">The tenant to check against</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unmapped devices</returns>
    Task<List<UnmappedDeviceDto>> GetUnmappedDevicesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a geofence sync operation
/// </summary>
public record GeofenceSyncResult(
    bool Success,
    int RecordsProcessed,
    int RecordsCreated,
    int RecordsSkipped,
    string? ErrorMessage = null,
    DateTime? LastEventTimestamp = null,
    List<DateOnly>? DatesProcessed = null,
    int SummariesCreated = 0,
    int EventsProcessedForSummaries = 0);
