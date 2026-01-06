using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetSitePhotoAttendanceById;

public class GetSitePhotoAttendanceByIdHandler : IRequestHandler<GetSitePhotoAttendanceByIdQuery, SitePhotoAttendanceDto?>
{
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly IMapper _mapper;

    public GetSitePhotoAttendanceByIdHandler(
        ISitePhotoAttendanceRepository spaRepository,
        IMapper mapper)
    {
        _spaRepository = spaRepository;
        _mapper = mapper;
    }

    public async Task<SitePhotoAttendanceDto?> Handle(GetSitePhotoAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        var spa = await _spaRepository.GetByIdAsync(request.Id, cancellationToken);

        if (spa == null || spa.TenantId != request.TenantId)
            return null;

        return _mapper.Map<SitePhotoAttendanceDto>(spa);
    }
}
