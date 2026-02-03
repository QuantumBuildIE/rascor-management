namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Cached device status data synced from the mobile geofence database.
/// This is a read-only cache - the source of truth is the geofence DB.
/// </summary>
public class DeviceStatusCache
{
    /// <summary>
    /// Device ID in format EVT#### (Primary Key)
    /// </summary>
    public string DeviceId { get; set; } = null!;

    /// <summary>
    /// Device model name (e.g., "SM-A546B", "Pixel 7")
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Platform: "iOS" or "Android"
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// Whether the device is currently active in the geofence system
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the device was last seen/active (from heartbeat)
    /// </summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// Last known GPS latitude
    /// </summary>
    public decimal? LastLatitude { get; set; }

    /// <summary>
    /// Last known GPS longitude
    /// </summary>
    public decimal? LastLongitude { get; set; }

    /// <summary>
    /// GPS accuracy in meters
    /// </summary>
    public decimal? LastAccuracy { get; set; }

    /// <summary>
    /// Last known battery level (0-100)
    /// </summary>
    public int? LastBatteryLevel { get; set; }

    /// <summary>
    /// When this cache entry was last synced from the geofence DB
    /// </summary>
    public DateTime SyncedAt { get; set; }
}
