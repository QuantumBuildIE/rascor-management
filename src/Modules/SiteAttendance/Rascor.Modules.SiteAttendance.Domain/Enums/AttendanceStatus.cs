namespace Rascor.Modules.SiteAttendance.Domain.Enums;

public enum AttendanceStatus
{
    Excellent = 1,    // >= 90% utilization
    Good = 2,         // 75-90% utilization
    BelowTarget = 3,  // < 75% utilization
    Absent = 4,       // No attendance recorded
    Incomplete = 5    // Entry but no exit, or vice versa
}
