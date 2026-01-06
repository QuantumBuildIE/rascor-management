using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceEventById;

public record GetAttendanceEventByIdQuery : IRequest<AttendanceEventDto?>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
}
