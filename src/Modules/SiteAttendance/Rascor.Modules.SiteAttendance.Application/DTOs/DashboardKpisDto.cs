namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record DashboardKpisDto
{
    public decimal OverallUtilization { get; init; }
    public decimal AverageHoursPerDay { get; init; }
    public int TotalActiveEmployees { get; init; }
    public int TotalActiveSites { get; init; }
    public int ExcellentCount { get; init; }
    public int GoodCount { get; init; }
    public int BelowTargetCount { get; init; }
    public int AbsentCount { get; init; }
    public decimal ExpectedHours { get; init; }
    public decimal ActualHours { get; init; }
    public decimal VarianceHours { get; init; }
    public int WorkingDays { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
}
