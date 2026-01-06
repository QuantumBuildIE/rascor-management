namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record SiteActivityDto
{
    public Guid SiteId { get; init; }
    public string SiteName { get; init; } = null!;
    public int EmployeeCount { get; init; }
    public decimal TotalHours { get; init; }
    public decimal AverageHoursPerEmployee { get; init; }
    public int TotalEvents { get; init; }
    public int DaysActive { get; init; }
}
