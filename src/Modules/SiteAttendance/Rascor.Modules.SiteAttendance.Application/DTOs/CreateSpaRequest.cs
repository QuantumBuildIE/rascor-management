namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record CreateSpaRequest
{
    public Guid EmployeeId { get; init; }
    public Guid SiteId { get; init; }
    public DateOnly EventDate { get; init; }
    public string? WeatherConditions { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Notes { get; init; }
}
