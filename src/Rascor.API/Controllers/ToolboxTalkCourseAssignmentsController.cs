using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Commands;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Queries;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/toolbox-talks/course-assignments")]
[Authorize(Policy = "ToolboxTalks.View")]
public class ToolboxTalkCourseAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolboxTalkCourseAssignmentsController> _logger;

    public ToolboxTalkCourseAssignmentsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<ToolboxTalkCourseAssignmentsController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Assign a course to one or more employees
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ToolboxTalks.Create")]
    [ProducesResponseType(typeof(Result<List<ToolboxTalkCourseAssignmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign([FromBody] AssignCourseDto dto)
    {
        try
        {
            var command = new AssignCourseCommand
            {
                TenantId = _currentUserService.TenantId,
                Dto = dto
            };

            var result = await _mediator.Send(command);
            return Ok(Result.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning course");
            return StatusCode(500, Result.Fail("Error assigning course"));
        }
    }

    /// <summary>
    /// Get all assignments for a course (admin view)
    /// </summary>
    [HttpGet("by-course/{courseId:guid}")]
    [ProducesResponseType(typeof(Result<List<CourseAssignmentListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        try
        {
            var query = new GetCourseAssignmentsQuery
            {
                TenantId = _currentUserService.TenantId,
                CourseId = courseId
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course assignments for course {CourseId}", courseId);
            return StatusCode(500, Result.Fail("Error retrieving course assignments"));
        }
    }

    /// <summary>
    /// Get a single course assignment with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ToolboxTalkCourseAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetCourseAssignmentByIdQuery
            {
                TenantId = _currentUserService.TenantId,
                Id = id
            };

            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { message = "Course assignment not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course assignment {AssignmentId}", id);
            return StatusCode(500, new { message = "Error retrieving course assignment" });
        }
    }

    /// <summary>
    /// Delete a course assignment (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ToolboxTalks.Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteCourseAssignmentCommand
            {
                TenantId = _currentUserService.TenantId,
                Id = id
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course assignment {AssignmentId}", id);
            return StatusCode(500, new { message = "Error deleting course assignment" });
        }
    }
}
