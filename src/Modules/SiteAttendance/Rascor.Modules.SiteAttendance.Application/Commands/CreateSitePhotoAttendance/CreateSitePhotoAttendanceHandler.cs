using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.CreateSitePhotoAttendance;

public class CreateSitePhotoAttendanceHandler : IRequestHandler<CreateSitePhotoAttendanceCommand, SitePhotoAttendanceDto>
{
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly IGeofenceService _geofenceService;
    private readonly IMapper _mapper;

    public CreateSitePhotoAttendanceHandler(
        ISitePhotoAttendanceRepository spaRepository,
        IGeofenceService geofenceService,
        IMapper mapper)
    {
        _spaRepository = spaRepository;
        _geofenceService = geofenceService;
        _mapper = mapper;
    }

    public async Task<SitePhotoAttendanceDto> Handle(CreateSitePhotoAttendanceCommand request, CancellationToken cancellationToken)
    {
        // Check if SPA already exists for this employee/site/date
        var existingSpa = await _spaRepository.GetByEmployeeSiteDateAsync(
            request.TenantId,
            request.EmployeeId,
            request.SiteId,
            request.EventDate,
            cancellationToken);

        if (existingSpa != null)
        {
            throw new InvalidOperationException(
                $"Site Photo Attendance already exists for employee {request.EmployeeId} at site {request.SiteId} on {request.EventDate}");
        }

        // Create SPA
        var spa = SitePhotoAttendance.Create(
            request.TenantId,
            request.EmployeeId,
            request.SiteId,
            request.EventDate,
            request.WeatherConditions,
            request.ImageUrl,
            request.Latitude,
            request.Longitude,
            request.Notes);

        // Calculate distance to site if coordinates provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var isWithinGeofence = await _geofenceService.IsWithinGeofenceAsync(
                request.SiteId,
                request.Latitude.Value,
                request.Longitude.Value,
                cancellationToken);

            var (nearestSite, distance) = await _geofenceService.FindNearestSiteAsync(
                request.TenantId,
                request.Latitude.Value,
                request.Longitude.Value,
                cancellationToken);

            if (nearestSite != null && nearestSite.Id == request.SiteId)
            {
                spa.SetDistanceToSite((decimal)distance);
            }
        }

        await _spaRepository.AddAsync(spa, cancellationToken);

        return _mapper.Map<SitePhotoAttendanceDto>(spa);
    }
}
