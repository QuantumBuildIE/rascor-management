using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.SiteAttendance.Application.Commands.UpdateAttendanceSettings;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceSettings;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/settings")]
[Authorize]
public class AttendanceSettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceSettingsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get tenant attendance settings
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(AttendanceSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var query = new GetAttendanceSettingsQuery
        {
            TenantId = _currentUserService.TenantId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update tenant attendance settings
    /// </summary>
    [HttpPut]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(typeof(AttendanceSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateAttendanceSettingsRequest request)
    {
        var command = new UpdateAttendanceSettingsCommand
        {
            TenantId = _currentUserService.TenantId,
            ExpectedHoursPerDay = request.ExpectedHoursPerDay,
            WorkStartTime = request.WorkStartTime,
            LateThresholdMinutes = request.LateThresholdMinutes,
            IncludeSaturday = request.IncludeSaturday,
            IncludeSunday = request.IncludeSunday,
            GeofenceRadiusMeters = request.GeofenceRadiusMeters,
            NoiseThresholdMeters = request.NoiseThresholdMeters,
            SpaGracePeriodMinutes = request.SpaGracePeriodMinutes,
            EnablePushNotifications = request.EnablePushNotifications,
            EnableEmailNotifications = request.EnableEmailNotifications,
            EnableSmsNotifications = request.EnableSmsNotifications,
            NotificationTitle = request.NotificationTitle,
            NotificationMessage = request.NotificationMessage
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
