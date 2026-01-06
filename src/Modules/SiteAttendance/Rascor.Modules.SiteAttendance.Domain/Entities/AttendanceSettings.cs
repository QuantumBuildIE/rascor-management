using Rascor.Core.Domain.Common;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Tenant-level attendance configuration settings
/// </summary>
public class AttendanceSettings : TenantEntity
{
    public decimal ExpectedHoursPerDay { get; private set; }
    public TimeOnly WorkStartTime { get; private set; }
    public int LateThresholdMinutes { get; private set; }
    public bool IncludeSaturday { get; private set; }
    public bool IncludeSunday { get; private set; }
    public int GeofenceRadiusMeters { get; private set; }
    public int NoiseThresholdMeters { get; private set; }
    public int SpaGracePeriodMinutes { get; private set; }
    public bool EnablePushNotifications { get; private set; }
    public bool EnableEmailNotifications { get; private set; }
    public bool EnableSmsNotifications { get; private set; }
    public string NotificationTitle { get; private set; } = null!;
    public string NotificationMessage { get; private set; } = null!;

    private AttendanceSettings() { } // EF Core

    public static AttendanceSettings CreateDefault(Guid tenantId)
    {
        return new AttendanceSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExpectedHoursPerDay = 7.5m,
            WorkStartTime = new TimeOnly(8, 0),
            LateThresholdMinutes = 30,
            IncludeSaturday = false,
            IncludeSunday = false,
            GeofenceRadiusMeters = 100,
            NoiseThresholdMeters = 150,
            SpaGracePeriodMinutes = 5,
            EnablePushNotifications = true,
            EnableEmailNotifications = true,
            EnableSmsNotifications = false,
            NotificationTitle = "Site Attendance Reminder",
            NotificationMessage = "Please complete your site attendance record",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        decimal expectedHoursPerDay,
        TimeOnly workStartTime,
        int lateThresholdMinutes,
        bool includeSaturday,
        bool includeSunday,
        int geofenceRadiusMeters,
        int noiseThresholdMeters,
        int spaGracePeriodMinutes,
        bool enablePushNotifications,
        bool enableEmailNotifications,
        bool enableSmsNotifications,
        string notificationTitle,
        string notificationMessage)
    {
        ExpectedHoursPerDay = expectedHoursPerDay;
        WorkStartTime = workStartTime;
        LateThresholdMinutes = lateThresholdMinutes;
        IncludeSaturday = includeSaturday;
        IncludeSunday = includeSunday;
        GeofenceRadiusMeters = geofenceRadiusMeters;
        NoiseThresholdMeters = noiseThresholdMeters;
        SpaGracePeriodMinutes = spaGracePeriodMinutes;
        EnablePushNotifications = enablePushNotifications;
        EnableEmailNotifications = enableEmailNotifications;
        EnableSmsNotifications = enableSmsNotifications;
        NotificationTitle = notificationTitle;
        NotificationMessage = notificationMessage;
    }
}
