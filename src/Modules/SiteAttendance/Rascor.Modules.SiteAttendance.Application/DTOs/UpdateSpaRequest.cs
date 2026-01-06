namespace Rascor.Modules.SiteAttendance.Application.DTOs;

public record UpdateSpaRequest
{
    public string? WeatherConditions { get; init; }
    public string? ImageUrl { get; init; }
    public string? Notes { get; init; }
}
