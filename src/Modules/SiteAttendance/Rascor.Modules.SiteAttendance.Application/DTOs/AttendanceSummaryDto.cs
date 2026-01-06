namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record AttendanceSummaryDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = null!;
    public Guid SiteId { get; init; }
    public string SiteName { get; init; } = null!;
    public DateOnly Date { get; init; }
    public DateTime? FirstEntry { get; init; }
    public DateTime? LastExit { get; init; }
    public decimal TimeOnSiteHours { get; init; }
    public decimal ExpectedHours { get; init; }
    public decimal UtilizationPercent { get; init; }
    public decimal VarianceHours { get; init; }
    public string Status { get; init; } = null!;
    public int EntryCount { get; init; }
    public int ExitCount { get; init; }
    public bool HasSpa { get; init; }
}
