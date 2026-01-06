using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceSettings;

public class GetAttendanceSettingsHandler : IRequestHandler<GetAttendanceSettingsQuery, AttendanceSettingsDto?>
{
    private readonly IAttendanceSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public GetAttendanceSettingsHandler(
        IAttendanceSettingsRepository settingsRepository,
        IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public async Task<AttendanceSettingsDto?> Handle(GetAttendanceSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByTenantAsync(request.TenantId, cancellationToken);

        if (settings == null)
            return null;

        return _mapper.Map<AttendanceSettingsDto>(settings);
    }
}
