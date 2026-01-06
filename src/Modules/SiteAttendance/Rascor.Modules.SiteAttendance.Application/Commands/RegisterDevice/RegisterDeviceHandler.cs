using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.RegisterDevice;

public class RegisterDeviceHandler : IRequestHandler<RegisterDeviceCommand, DeviceRegistrationDto>
{
    private readonly IDeviceRegistrationRepository _deviceRepository;
    private readonly IMapper _mapper;

    public RegisterDeviceHandler(
        IDeviceRegistrationRepository deviceRepository,
        IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<DeviceRegistrationDto> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        // Check if device already exists
        var existingDevice = await _deviceRepository.GetByDeviceIdentifierAsync(
            request.TenantId,
            request.DeviceIdentifier,
            cancellationToken);

        if (existingDevice != null)
        {
            // Update existing device registration
            existingDevice.MarkActive();

            if (!string.IsNullOrEmpty(request.PushToken))
            {
                existingDevice.UpdatePushToken(request.PushToken);
            }

            if (request.EmployeeId.HasValue)
            {
                existingDevice.AssignToEmployee(request.EmployeeId.Value);
            }

            await _deviceRepository.UpdateAsync(existingDevice, cancellationToken);

            return _mapper.Map<DeviceRegistrationDto>(existingDevice);
        }

        // Create new device registration
        var device = DeviceRegistration.Create(
            request.TenantId,
            request.DeviceIdentifier,
            request.DeviceName,
            request.Platform,
            request.EmployeeId);

        if (!string.IsNullOrEmpty(request.PushToken))
        {
            device.UpdatePushToken(request.PushToken);
        }

        await _deviceRepository.AddAsync(device, cancellationToken);

        return _mapper.Map<DeviceRegistrationDto>(device);
    }
}
