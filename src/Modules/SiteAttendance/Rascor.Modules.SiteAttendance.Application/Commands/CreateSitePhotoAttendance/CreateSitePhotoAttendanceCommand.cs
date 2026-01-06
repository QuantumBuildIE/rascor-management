using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Commands.CreateSitePhotoAttendance;

public record CreateSitePhotoAttendanceCommand : IRequest<SitePhotoAttendanceDto>
{
    public Guid TenantId { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid SiteId { get; init; }
    public DateOnly EventDate { get; init; }
    public string? WeatherConditions { get; init; }
    public string? ImageUrl { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Notes { get; init; }
}
