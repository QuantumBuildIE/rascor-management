using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Commands.RegisterDevice;

public record RegisterDeviceCommand : IRequest<DeviceRegistrationDto>
{
    public Guid TenantId { get; init; }
    public string DeviceIdentifier { get; init; } = null!;
    public string? DeviceName { get; init; }
    public string? Platform { get; init; }
    public string? PushToken { get; init; }
    public Guid? EmployeeId { get; init; }
}
