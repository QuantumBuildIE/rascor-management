using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.Commands.RecordAttendanceEvent;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceEventById;
using Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceEvents;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.API.Controllers.SiteAttendance;

[ApiController]
[Route("api/site-attendance/events")]
[Authorize]
public class AttendanceEventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceEventsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Record an attendance event (for mobile app)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AttendanceEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordEvent([FromBody] RecordAttendanceEventRequest request)
    {
        var command = new RecordAttendanceEventCommand
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = request.EmployeeId,
            SiteId = request.SiteId,
            EventType = Enum.Parse<EventType>(request.EventType, true),
            Timestamp = request.Timestamp,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TriggerMethod = Enum.Parse<TriggerMethod>(request.TriggerMethod, true),
            DeviceIdentifier = request.DeviceIdentifier,
            UserId = Guid.TryParse(_currentUserService.UserId, out var userId) ? userId : null
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Record multiple attendance events (batch for offline sync)
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(IEnumerable<AttendanceEventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordEventsBatch([FromBody] IEnumerable<RecordAttendanceEventRequest> requests)
    {
        var results = new List<AttendanceEventDto>();

        foreach (var request in requests.OrderBy(r => r.Timestamp))
        {
            var command = new RecordAttendanceEventCommand
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = request.EmployeeId,
                SiteId = request.SiteId,
                EventType = Enum.Parse<EventType>(request.EventType, true),
                Timestamp = request.Timestamp,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                TriggerMethod = Enum.Parse<TriggerMethod>(request.TriggerMethod, true),
                DeviceIdentifier = request.DeviceIdentifier,
                UserId = Guid.TryParse(_currentUserService.UserId, out var userId) ? userId : null
            };

            var result = await _mediator.Send(command);
            results.Add(result);
        }

        return CreatedAtAction(nameof(GetEvents), results);
    }

    /// <summary>
    /// Get attendance events with filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AttendanceEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? siteId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? eventType,
        [FromQuery] bool? includeNoise,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAttendanceEventsQuery
        {
            TenantId = _currentUserService.TenantId,
            EmployeeId = employeeId,
            SiteId = siteId,
            FromDate = fromDate,
            ToDate = toDate,
            EventType = string.IsNullOrEmpty(eventType) ? null : Enum.Parse<EventType>(eventType, true),
            IncludeNoise = includeNoise,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AttendanceEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetAttendanceEventByIdQuery
        {
            Id = id,
            TenantId = _currentUserService.TenantId
        };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
