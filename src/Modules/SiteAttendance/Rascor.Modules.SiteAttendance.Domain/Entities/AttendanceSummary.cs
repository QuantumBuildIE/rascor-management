using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Daily aggregated attendance per employee per site
/// </summary>
public class AttendanceSummary : TenantEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid SiteId { get; private set; }
    public DateOnly Date { get; private set; }
    public DateTime? FirstEntry { get; private set; }
    public DateTime? LastExit { get; private set; }
    public int TimeOnSiteMinutes { get; private set; }
    public decimal ExpectedHours { get; private set; }
    public decimal UtilizationPercent { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public int EntryCount { get; private set; }
    public int ExitCount { get; private set; }
    public bool HasSpa { get; private set; }

    // Navigation properties
    public virtual Employee Employee { get; private set; } = null!;
    public virtual Site Site { get; private set; } = null!;

    private AttendanceSummary() { } // EF Core

    public static AttendanceSummary Create(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly date,
        decimal expectedHours = 7.5m)
    {
        return new AttendanceSummary
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            Date = date,
            ExpectedHours = expectedHours,
            TimeOnSiteMinutes = 0,
            UtilizationPercent = 0,
            Status = AttendanceStatus.Absent,
            EntryCount = 0,
            ExitCount = 0,
            HasSpa = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateFromEvents(
        DateTime? firstEntry,
        DateTime? lastExit,
        int timeOnSiteMinutes,
        int entryCount,
        int exitCount)
    {
        FirstEntry = firstEntry;
        LastExit = lastExit;
        TimeOnSiteMinutes = timeOnSiteMinutes;
        EntryCount = entryCount;
        ExitCount = exitCount;

        // Calculate utilization
        var actualHours = timeOnSiteMinutes / 60.0m;
        UtilizationPercent = ExpectedHours > 0
            ? Math.Round((actualHours / ExpectedHours) * 100, 2)
            : 0;

        // Determine status
        Status = UtilizationPercent switch
        {
            >= 90 => AttendanceStatus.Excellent,
            >= 75 => AttendanceStatus.Good,
            > 0 => AttendanceStatus.BelowTarget,
            _ when entryCount > 0 || exitCount > 0 => AttendanceStatus.Incomplete,
            _ => AttendanceStatus.Absent
        };
    }

    public void MarkHasSpa(bool hasSpa)
    {
        HasSpa = hasSpa;
    }

    public decimal ActualHours => Math.Round(TimeOnSiteMinutes / 60.0m, 2);
    public decimal VarianceHours => ActualHours - ExpectedHours;
}
