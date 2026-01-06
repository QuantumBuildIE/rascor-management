namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record AttendanceEventDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = null!;
    public Guid SiteId { get; init; }
    public string SiteName { get; init; } = null!;
    public string EventType { get; init; } = null!;
    public DateTime Timestamp { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string TriggerMethod { get; init; } = null!;
    public bool IsNoise { get; init; }
    public decimal? NoiseDistance { get; init; }
}
