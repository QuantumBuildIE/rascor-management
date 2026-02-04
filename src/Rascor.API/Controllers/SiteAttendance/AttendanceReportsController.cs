using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetSiteAttendanceReport;

namespace Rascor.API.Controllers.SiteAttendance;

/// <summary>
/// Controller for site attendance reports.
/// Provides endpoints for generating attendance reconciliation reports.
/// </summary>
[ApiController]
[Route("api/site-attendance/reports")]
[Authorize]
public class AttendanceReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceReportsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get the site attendance report for a specific date.
    /// Cross-references Float scheduling data (who SHOULD be on site) with geofence events
    /// (who ACTUALLY arrived) and SPA completion.
    /// </summary>
    /// <param name="date">The date to generate the report for. Defaults to today if not specified.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The site attendance report with entries categorized as Planned, Arrived, or Unplanned.</returns>
    [HttpGet("attendance")]
    [Authorize(Policy = "SiteAttendance.View")]
    [ProducesResponseType(typeof(SiteAttendanceReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SiteAttendanceReportDto>> GetAttendanceReport(
        [FromQuery] DateOnly? date,
        CancellationToken ct)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetSiteAttendanceReportQuery
        {
            TenantId = _currentUserService.TenantId,
            Date = reportDate
        };

        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
