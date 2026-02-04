namespace Rascor.Modules.SiteAttendance.Application.DTOs;

/// <summary>
/// Status indicating the relationship between Float scheduling and actual geofence arrival.
/// </summary>
public enum AttendanceReportStatus
{
    /// <summary>
    /// Scheduled on Float, no geofence entry yet.
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Scheduled on Float AND geofence confirms arrival.
    /// </summary>
    Arrived = 1,

    /// <summary>
    /// NOT scheduled on Float, but geofence shows they arrived.
    /// </summary>
    Unplanned = 2
}

/// <summary>
/// A single entry in the site attendance report representing one employee-site combination.
/// </summary>
public class SiteAttendanceReportEntryDto
{
    /// <summary>
    /// The attendance status (Planned, Arrived, or Unplanned).
    /// </summary>
    public AttendanceReportStatus Status { get; set; }

    /// <summary>
    /// The employee ID.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// The employee's full name.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// The site ID.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// The site name.
    /// </summary>
    public string SiteName { get; set; } = string.Empty;

    /// <summary>
    /// The site code (if available).
    /// </summary>
    public string? SiteCode { get; set; }

    /// <summary>
    /// The planned arrival date/time from Float task start date.
    /// Note: Float only provides date, not specific time, so this is midnight UTC of the task date.
    /// </summary>
    public DateTime? PlannedArrival { get; set; }

    /// <summary>
    /// The actual arrival time from the first geofence Enter event.
    /// </summary>
    public DateTime? ActualArrival { get; set; }

    /// <summary>
    /// Whether an SPA (Site Photo Attendance) record exists for this employee/site/date.
    /// </summary>
    public bool SpaCompleted { get; set; }

    /// <summary>
    /// The SPA record ID if it exists.
    /// </summary>
    public Guid? SpaId { get; set; }

    /// <summary>
    /// The SPA image URL if available.
    /// </summary>
    public string? SpaImageUrl { get; set; }
}

/// <summary>
/// The complete site attendance report for a given date.
/// Cross-references Float scheduling data with geofence events and SPA completion.
/// </summary>
public class SiteAttendanceReportDto
{
    /// <summary>
    /// The date of the report.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The list of attendance entries.
    /// </summary>
    public List<SiteAttendanceReportEntryDto> Entries { get; set; } = new();

    /// <summary>
    /// Total count of entries with Planned status (scheduled but not arrived).
    /// </summary>
    public int TotalPlanned { get; set; }

    /// <summary>
    /// Total count of entries with Arrived status (scheduled and arrived).
    /// </summary>
    public int TotalArrived { get; set; }

    /// <summary>
    /// Total count of entries with Unplanned status (not scheduled but arrived).
    /// </summary>
    public int TotalUnplanned { get; set; }
}
