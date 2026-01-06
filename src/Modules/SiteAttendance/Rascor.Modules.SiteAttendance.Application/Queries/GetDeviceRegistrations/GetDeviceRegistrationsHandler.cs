using AutoMapper;
using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetDeviceRegistrations;

public class GetDeviceRegistrationsHandler : IRequestHandler<GetDeviceRegistrationsQuery, PaginatedList<DeviceRegistrationDto>>
{
    private readonly IDeviceRegistrationRepository _deviceRepository;
    private readonly IMapper _mapper;

    public GetDeviceRegistrationsHandler(
        IDeviceRegistrationRepository deviceRepository,
        IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<DeviceRegistrationDto>> Handle(GetDeviceRegistrationsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.DeviceRegistration> devices;

        if (request.EmployeeId.HasValue)
        {
            devices = await _deviceRepository.GetByEmployeeAsync(
                request.TenantId,
                request.EmployeeId.Value,
                cancellationToken);
        }
        else if (request.IsActive == true)
        {
            devices = await _deviceRepository.GetActiveDevicesAsync(
                request.TenantId,
                cancellationToken);
        }
        else
        {
            devices = await _deviceRepository.GetActiveDevicesAsync(
                request.TenantId,
                cancellationToken);
        }

        var deviceList = devices.ToList();

        // Apply additional filters
        if (request.IsActive.HasValue)
        {
            deviceList = deviceList.Where(d => d.IsActive == request.IsActive.Value).ToList();
        }

        // Order by last active descending
        deviceList = deviceList.OrderByDescending(d => d.LastActiveAt ?? d.RegisteredAt).ToList();

        // Paginate
        var totalCount = deviceList.Count;
        var pagedDevices = deviceList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<DeviceRegistrationDto>>(pagedDevices);

        return new PaginatedList<DeviceRegistrationDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
