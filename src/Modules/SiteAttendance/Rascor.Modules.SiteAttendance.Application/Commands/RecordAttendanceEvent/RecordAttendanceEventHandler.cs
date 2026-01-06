using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.RecordAttendanceEvent;

public class RecordAttendanceEventHandler : IRequestHandler<RecordAttendanceEventCommand, AttendanceEventDto>
{
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly IDeviceRegistrationRepository _deviceRepository;
    private readonly IGeofenceService _geofenceService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public RecordAttendanceEventHandler(
        IAttendanceEventRepository eventRepository,
        IDeviceRegistrationRepository deviceRepository,
        IGeofenceService geofenceService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _deviceRepository = deviceRepository;
        _geofenceService = geofenceService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<AttendanceEventDto> Handle(RecordAttendanceEventCommand request, CancellationToken cancellationToken)
    {
        // Get device registration if provided
        Guid? deviceId = null;
        if (!string.IsNullOrEmpty(request.DeviceIdentifier))
        {
            var device = await _deviceRepository.GetByDeviceIdentifierAsync(
                request.TenantId,
                request.DeviceIdentifier,
                cancellationToken);
            deviceId = device?.Id;
        }

        // Create event
        var attendanceEvent = AttendanceEvent.Create(
            request.TenantId,
            request.EmployeeId,
            request.SiteId,
            request.EventType,
            request.Timestamp,
            request.Latitude,
            request.Longitude,
            request.TriggerMethod,
            deviceId);

        // Check for noise
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var (isNoise, distance) = await _geofenceService.CheckForNoiseAsync(
                attendanceEvent,
                request.TenantId,
                cancellationToken);

            if (isNoise && distance.HasValue)
            {
                attendanceEvent.MarkAsNoise(distance.Value);
            }
        }

        await _eventRepository.AddAsync(attendanceEvent, cancellationToken);

        // Check for missing SPA on entry events
        if (request.EventType == EventType.Enter && !attendanceEvent.IsNoise)
        {
            await _notificationService.CheckAndNotifyMissingSpaAsync(attendanceEvent, cancellationToken);
        }

        return _mapper.Map<AttendanceEventDto>(attendanceEvent);
    }
}
