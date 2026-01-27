using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateSitePhotoAttendance;

public class UpdateSitePhotoAttendanceHandler : IRequestHandler<UpdateSitePhotoAttendanceCommand, SitePhotoAttendanceDto>
{
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly IMapper _mapper;

    public UpdateSitePhotoAttendanceHandler(
        ISitePhotoAttendanceRepository spaRepository,
        IMapper mapper)
    {
        _spaRepository = spaRepository;
        _mapper = mapper;
    }

    public async Task<SitePhotoAttendanceDto> Handle(UpdateSitePhotoAttendanceCommand request, CancellationToken cancellationToken)
    {
        var spa = await _spaRepository.GetByIdAsync(request.Id, cancellationToken);

        if (spa == null)
        {
            throw new KeyNotFoundException($"Site Photo Attendance with ID {request.Id} not found.");
        }

        if (spa.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException("Access denied to this Site Photo Attendance record.");
        }

        // Update mutable fields
        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            spa.UpdateImage(request.ImageUrl);
        }

        if (!string.IsNullOrEmpty(request.SignatureUrl))
        {
            spa.UpdateSignature(request.SignatureUrl);
        }

        spa.UpdateWeatherConditions(request.WeatherConditions);
        spa.UpdateNotes(request.Notes);

        await _spaRepository.UpdateAsync(spa, cancellationToken);

        return _mapper.Map<SitePhotoAttendanceDto>(spa);
    }
}
