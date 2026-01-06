using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceSummaries;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/summaries")]
[Authorize]
public class AttendanceSummariesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceSummariesController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get attendance summaries with filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaries(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? siteId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAttendanceSummariesQuery
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            FromDate = fromDate,
            ToDate = toDate,
            Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<AttendanceStatus>(status, true),
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get attendance summaries for a specific employee
    /// </summary>
    [HttpGet("employee/{employeeId:guid}")]
    [ProducesResponseType(typeof(PaginatedList<AttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmployee(
        Guid employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAttendanceSummariesQuery
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = employeeId,
            FromDate = fromDate,
            ToDate = toDate,
            Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<AttendanceStatus>(status, true),
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get attendance summaries for a specific site
    /// </summary>
    [HttpGet("site/{siteId:guid}")]
    [ProducesResponseType(typeof(PaginatedList<AttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySite(
        Guid siteId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAttendanceSummariesQuery
        {
            TenantId = _currentUserService.TenantId,
            SiteId = siteId,
            FromDate = fromDate,
            ToDate = toDate,
            Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<AttendanceStatus>(status, true),
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
