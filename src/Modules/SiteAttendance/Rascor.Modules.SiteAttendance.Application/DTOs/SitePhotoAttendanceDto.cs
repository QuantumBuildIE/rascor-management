namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record SitePhotoAttendanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = null!;
    public Guid SiteId { get; init; }
    public string SiteName { get; init; } = null!;
    public DateOnly EventDate { get; init; }
    public string? WeatherConditions { get; init; }
    public string? ImageUrl { get; init; }
    public decimal? DistanceToSite { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
