using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;

namespace Rascor.API.Controllers.SiteAttendance;

/// <summary>
/// Admin endpoints for device management.
/// Supports the Zoho migration workflow where existing devices need to be manually linked to employees.
/// </summary>
[ApiController]
[Route("api/admin/devices")]
[Authorize(Policy = "SiteAttendance.Admin")]
public class AdminDevicesController : ControllerBase
{
    private readonly IAdminDeviceService _adminDeviceService;
    private readonly ICurrentUserService _currentUserService;

    public AdminDevicesController(
        IAdminDeviceService adminDeviceService,
        ICurrentUserService currentUserService)
    {
        _adminDeviceService = adminDeviceService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get paginated list of devices with optional filtering.
    /// </summary>
    /// <param name="isLinked">Filter: true=linked only, false=unlinked only, null=all</param>
    /// <param name="isActive">Filter: true=active only, false=inactive only, null=all</param>
    /// <param name="search">Search by device ID (e.g., EVT0001) or employee name</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of devices</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AdminDeviceListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevices(
        [FromQuery] bool? isLinked = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminDeviceService.GetDevicesAsync(
            _currentUserService.TenantId,
            isLinked,
            isActive,
            search,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get detailed device information by ID.
    /// </summary>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device details</returns>
    [HttpGet("{deviceId:guid}")]
    [ProducesResponseType(typeof(AdminDeviceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        var result = await _adminDeviceService.GetDeviceAsync(
            _currentUserService.TenantId,
            deviceId,
            cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Device not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Link a device to an employee.
    /// Used for Zoho migration where existing devices need to be manually associated.
    /// </summary>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="request">Link request with employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error result</returns>
    [HttpPost("{deviceId:guid}/link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkDevice(
        Guid deviceId,
        [FromBody] LinkDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminDeviceService.LinkDeviceToEmployeeAsync(
            _currentUserService.TenantId,
            deviceId,
            request.EmployeeId,
            cancellationToken);

        if (!result.Success)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Device linked successfully" });
    }

    /// <summary>
    /// Unlink a device from its current employee.
    /// </summary>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="request">Unlink request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error result</returns>
    [HttpPost("{deviceId:guid}/unlink")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDevice(
        Guid deviceId,
        [FromBody] UnlinkDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminDeviceService.UnlinkDeviceAsync(
            _currentUserService.TenantId,
            deviceId,
            request.Reason,
            cancellationToken);

        if (!result.Success)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Device unlinked successfully" });
    }

    /// <summary>
    /// Deactivate a device (soft delete).
    /// The device will no longer be able to send attendance events.
    /// </summary>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error result</returns>
    [HttpPost("{deviceId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateDevice(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var result = await _adminDeviceService.DeactivateDeviceAsync(
            _currentUserService.TenantId,
            deviceId,
            cancellationToken);

        if (!result.Success)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Device deactivated successfully" });
    }

    /// <summary>
    /// Reactivate a previously deactivated device.
    /// </summary>
    /// <param name="deviceId">Device ID (Guid)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error result</returns>
    [HttpPost("{deviceId:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateDevice(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var result = await _adminDeviceService.ReactivateDeviceAsync(
            _currentUserService.TenantId,
            deviceId,
            cancellationToken);

        if (!result.Success)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Device reactivated successfully" });
    }
}
