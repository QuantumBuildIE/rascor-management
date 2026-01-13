namespace Rascor.Modules.SiteAttendance.Application.DTOs;

/// <summary>
/// Result of a manual geofence sync operation
/// </summary>
public record GeofenceSyncResultDto(
    bool Success,
    int RecordsProcessed,
    int RecordsCreated,
    int RecordsSkipped,
    string? ErrorMessage = null,
    DateTime? LastEventTimestamp = null,
    double DurationMs = 0,
    List<DateOnly>? DatesProcessed = null,
    int SummariesCreated = 0,
    int EventsProcessedForSummaries = 0);

/// <summary>
/// Status of a sync log entry
/// </summary>
public record GeofenceSyncLogDto(
    Guid Id,
    DateTime SyncStarted,
    DateTime? SyncCompleted,
    int RecordsProcessed,
    int RecordsCreated,
    int RecordsSkipped,
    string? LastEventId,
    DateTime? LastEventTimestamp,
    string? ErrorMessage,
    bool IsSuccess);

/// <summary>
/// Overall sync health status
/// </summary>
public record GeofenceSyncStatusDto(
    bool IsHealthy,
    DateTime? LastSuccessfulSync,
    int TotalSyncsLast24Hours,
    int FailedSyncsLast24Hours,
    int TotalEventsCreatedLast24Hours,
    List<GeofenceSyncLogDto> RecentSyncs);

/// <summary>
/// Device from mobile database that doesn't have a matching Employee
/// </summary>
public record UnmappedDeviceDto(
    string DeviceId,
    string? Platform,
    string? Model,
    string? Manufacturer,
    DateTime RegisteredAt,
    DateTime? LastSeenAt,
    bool IsActive,
    int EventCount);
