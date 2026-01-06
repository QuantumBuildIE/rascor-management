using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateDevice;

public record UpdateDeviceCommand : IRequest<DeviceRegistrationDto>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string? DeviceName { get; init; }
    public string? PushToken { get; init; }
    public Guid? EmployeeId { get; init; }
    public bool? IsActive { get; init; }
}
