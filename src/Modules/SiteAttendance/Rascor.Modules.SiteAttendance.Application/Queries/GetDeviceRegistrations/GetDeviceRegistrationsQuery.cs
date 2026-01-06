using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetDeviceRegistrations;

public record GetDeviceRegistrationsQuery : IRequest<PaginatedList<DeviceRegistrationDto>>
{
    public Guid TenantId { get; init; }
    public Guid? EmployeeId { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
