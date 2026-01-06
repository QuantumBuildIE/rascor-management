using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.Commands.RegisterDevice;
using Rascor.Modules.SiteAttendance.Application.Commands.UpdateDevice;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetDeviceRegistrations;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/devices")]
[Authorize]
public class DeviceRegistrationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DeviceRegistrationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Register a new device
    /// </summary>
    [HttpPost("register")]
    [Authorize(Policy = "SiteAttendance.MarkAttendance")]
    [ProducesResponseType(typeof(DeviceRegistrationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request)
    {
        var command = new RegisterDeviceCommand
        {
            TenantId = _currentUserService.TenantId,
            DeviceIdentifier = request.DeviceIdentifier,
            DeviceName = request.DeviceName,
            Platform = request.Platform,
            PushToken = request.PushToken,
            EmployeeId = request.EmployeeId
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetList), result);
    }

    /// <summary>
    /// Get registered devices
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(typeof(PaginatedList<DeviceRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? employeeId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetDeviceRegistrationsQuery
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = employeeId,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update a device registration
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(typeof(DeviceRegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeviceRequest request)
    {
        var command = new UpdateDeviceCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            DeviceName = request.DeviceName,
            PushToken = request.PushToken,
            EmployeeId = request.EmployeeId,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Deactivate a device (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SiteAttendance.Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new UpdateDeviceCommand
        {
            Id = id,
            TenantId = _currentUserService.TenantId,
            IsActive = false
        };

        await _mediator.Send(command);
        return NoContent();
    }
}
