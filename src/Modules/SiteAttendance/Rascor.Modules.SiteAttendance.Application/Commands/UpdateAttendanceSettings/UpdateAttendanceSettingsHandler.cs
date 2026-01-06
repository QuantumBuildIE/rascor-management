using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.UpdateAttendanceSettings;

public class UpdateAttendanceSettingsHandler : IRequestHandler<UpdateAttendanceSettingsCommand, AttendanceSettingsDto>
{
    private readonly IAttendanceSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public UpdateAttendanceSettingsHandler(
        IAttendanceSettingsRepository settingsRepository,
        IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public async Task<AttendanceSettingsDto> Handle(UpdateAttendanceSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByTenantAsync(request.TenantId, cancellationToken);

        if (settings == null)
        {
            // Create default settings if none exist
            settings = AttendanceSettings.CreateDefault(request.TenantId);
            await _settingsRepository.AddAsync(settings, cancellationToken);
        }

        settings.Update(
            request.ExpectedHoursPerDay,
            request.WorkStartTime,
            request.LateThresholdMinutes,
            request.IncludeSaturday,
            request.IncludeSunday,
            request.GeofenceRadiusMeters,
            request.NoiseThresholdMeters,
            request.SpaGracePeriodMinutes,
            request.EnablePushNotifications,
            request.EnableEmailNotifications,
            request.EnableSmsNotifications,
            request.NotificationTitle,
            request.NotificationMessage);

        await _settingsRepository.UpdateAsync(settings, cancellationToken);

        return _mapper.Map<AttendanceSettingsDto>(settings);
    }
}
