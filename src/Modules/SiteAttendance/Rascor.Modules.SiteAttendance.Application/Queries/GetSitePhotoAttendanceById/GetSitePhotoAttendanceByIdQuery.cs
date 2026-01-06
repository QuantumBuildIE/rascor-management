using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetSitePhotoAttendanceById;

public record GetSitePhotoAttendanceByIdQuery : IRequest<SitePhotoAttendanceDto?>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
}
