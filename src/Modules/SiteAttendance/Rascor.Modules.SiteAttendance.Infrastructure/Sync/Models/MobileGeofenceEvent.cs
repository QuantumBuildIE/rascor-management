namespace Rascor.Modules.SiteAttendance.Infrastructure.Sync.Models;

/// <summary>
/// Entity model that maps to the geofence_events table in the mobile geofence database.
/// This is a read-only model used for syncing data to Rascor.
/// </summary>
public class MobileGeofenceEvent
{
    /// <summary>
    /// Primary key in the mobile database
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User ID from the mobile app (maps to Employee in Rascor)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Site ID (should match Rascor Site ID)
    /// </summary>
    public string SiteId { get; set; } = string.Empty;

    /// <summary>
    /// Event type: "enter" or "exit"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// GPS latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// GPS longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// How the event was triggered: "automatic" (GPS geofence) or "manual"
    /// </summary>
    public string TriggerMethod { get; set; } = string.Empty;
}
