using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceSettings;

public record GetAttendanceSettingsQuery : IRequest<AttendanceSettingsDto?>
{
    public Guid TenantId { get; init; }
}
