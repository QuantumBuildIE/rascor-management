using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetDashboardKpis;
using Rascor.Modules.SiteAttendance.Application.Queries.GetEmployeePerformance;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/dashboard")]
[Authorize]
public class AttendanceDashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceDashboardController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get dashboard KPIs for a date range
    /// </summary>
    [HttpGet("kpis")]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(DashboardKpisDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKpis(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] Guid? siteId)
    {
        var query = new GetDashboardKpisQuery
        {
            TenantId = _currentUserService.TenantId,
            FromDate = fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            ToDate = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            SiteId = siteId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get employee performance breakdown
    /// </summary>
    [HttpGet("employee-performance")]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(PaginatedList<EmployeePerformanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeePerformance(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetEmployeePerformanceQuery
        {
            TenantId = _currentUserService.TenantId,
            FromDate = fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            ToDate = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            SiteId = siteId,
            EmployeeId = employeeId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
