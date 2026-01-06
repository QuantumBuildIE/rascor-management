namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record UpdateAttendanceSettingsRequest
{
    public decimal ExpectedHoursPerDay { get; set; }
    public TimeOnly WorkStartTime { get; set; }
    public int LateThresholdMinutes { get; set; }
    public bool IncludeSaturday { get; set; }
    public bool IncludeSunday { get; set; }
    public int GeofenceRadiusMeters { get; set; }
    public int NoiseThresholdMeters { get; set; }
    public int SpaGracePeriodMinutes { get; set; }
    public bool EnablePushNotifications { get; set; }
    public bool EnableEmailNotifications { get; set; }
    public bool EnableSmsNotifications { get; set; }
    public string NotificationTitle { get; set; } = null!;
    public string NotificationMessage { get; set; } = null!;
}
