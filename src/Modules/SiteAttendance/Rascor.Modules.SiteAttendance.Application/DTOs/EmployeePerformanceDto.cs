namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record EmployeePerformanceDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = null!;
    public decimal TotalHours { get; init; }
    public decimal ExpectedHours { get; init; }
    public decimal UtilizationPercent { get; init; }
    public decimal VarianceHours { get; init; }
    public string Status { get; init; } = null!;
    public int DaysPresent { get; init; }
    public int DaysAbsent { get; init; }
    public int SpaCount { get; init; }
}
