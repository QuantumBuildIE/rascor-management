namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record AttendanceSettingsDto
{
    public Guid Id { get; init; }
    public decimal ExpectedHoursPerDay { get; init; }
    public TimeOnly WorkStartTime { get; init; }
    public int LateThresholdMinutes { get; init; }
    public bool IncludeSaturday { get; init; }
    public bool IncludeSunday { get; init; }
    public int GeofenceRadiusMeters { get; init; }
    public int NoiseThresholdMeters { get; init; }
    public int SpaGracePeriodMinutes { get; init; }
    public bool EnablePushNotifications { get; init; }
    public bool EnableEmailNotifications { get; init; }
    public bool EnableSmsNotifications { get; init; }
    public string NotificationTitle { get; init; } = null!;
    public string NotificationMessage { get; init; } = null!;
}
