using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateSitePhotoAttendance;

public record UpdateSitePhotoAttendanceCommand : IRequest<SitePhotoAttendanceDto>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string? WeatherConditions { get; init; }
    public string? ImageUrl { get; init; }
    public string? SignatureUrl { get; init; }
    public string? Notes { get; init; }
}
