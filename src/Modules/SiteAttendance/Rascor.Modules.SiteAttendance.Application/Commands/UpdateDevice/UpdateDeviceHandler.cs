using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateDevice;

public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, DeviceRegistrationDto>
{
    private readonly IDeviceRegistrationRepository _deviceRepository;
    private readonly IMapper _mapper;

    public UpdateDeviceHandler(
        IDeviceRegistrationRepository deviceRepository,
        IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<DeviceRegistrationDto> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (device == null)
        {
            throw new KeyNotFoundException($"Device with ID {request.Id} not found.");
        }

        if (device.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException("Access denied to this device registration.");
        }

        // Update push token if provided
        if (!string.IsNullOrEmpty(request.PushToken))
        {
            device.UpdatePushToken(request.PushToken);
        }

        // Update employee assignment if provided
        if (request.EmployeeId.HasValue)
        {
            device.AssignToEmployee(request.EmployeeId.Value);
        }

        // Update active status if provided
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                device.MarkActive();
            }
            else
            {
                device.Deactivate();
            }
        }

        await _deviceRepository.UpdateAsync(device, cancellationToken);

        return _mapper.Map<DeviceRegistrationDto>(device);
    }
}
