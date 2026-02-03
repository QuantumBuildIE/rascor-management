namespace Rascor.Modules.SiteAttendance.Infrastructure.Sync.Models;

/// <summary>
/// Entity model that maps to the devices table in the mobile geofence database.
/// This is a read-only model used for syncing data to Rascor.
/// </summary>
public class MobileDevice
{
    /// <summary>
    /// Device ID in format EVT####
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Platform-specific device identifier
    /// </summary>
    public string? PlatformIdentifier { get; set; }

    /// <summary>
    /// Platform: "iOS" or "Android"
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// Device model name
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Device manufacturer
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Operating system version
    /// </summary>
    public string? OsVersion { get; set; }

    /// <summary>
    /// Type of device (phone, tablet, etc.)
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// When the device was first registered
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// When the device was last seen/active
    /// </summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// Whether the device is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Last known GPS latitude from heartbeat
    /// </summary>
    public double? LastLatitude { get; set; }

    /// <summary>
    /// Last known GPS longitude from heartbeat
    /// </summary>
    public double? LastLongitude { get; set; }

    /// <summary>
    /// GPS accuracy in meters from heartbeat
    /// </summary>
    public double? LastAccuracy { get; set; }

    /// <summary>
    /// Last known battery level (0-100) from heartbeat
    /// </summary>
    public int? LastBatteryLevel { get; set; }
}
