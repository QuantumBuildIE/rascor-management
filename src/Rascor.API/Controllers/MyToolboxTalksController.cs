using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.ToolboxTalks.Application.Commands.CompleteToolboxTalk;
using Rascor.Modules.ToolboxTalks.Application.Commands.MarkSectionRead;
using Rascor.Modules.ToolboxTalks.Application.Commands.SubmitQuizAnswers;
using Rascor.Modules.ToolboxTalks.Application.Commands.UpdateVideoProgress;
using Rascor.Modules.ToolboxTalks.Application.DTOs;
using Rascor.Modules.ToolboxTalks.Application.Queries.GetMyToolboxTalkById;
using Rascor.Modules.ToolboxTalks.Application.Queries.GetMyToolboxTalks;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.API.Controllers;

/// <summary>
/// Controller for employee portal to view and complete assigned toolbox talks
/// All endpoints return data filtered to the current authenticated employee
/// </summary>
[ApiController]
[Route("api/my/toolbox-talks")]
[Authorize]
public class MyToolboxTalksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MyToolboxTalksController> _logger;

    public MyToolboxTalksController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<MyToolboxTalksController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all toolbox talks assigned to the current employee
    /// </summary>
    /// <param name="status">Optional filter by status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of assigned toolbox talks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<MyToolboxTalkListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTalks(
        [FromQuery] ScheduledTalkStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var query = new GetMyToolboxTalksQuery
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = employeeId.Value,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my toolbox talks");
            return StatusCode(500, Result.Fail("Error retrieving my toolbox talks"));
        }
    }

    /// <summary>
    /// Get a specific assigned toolbox talk with full content for completion
    /// </summary>
    /// <param name="id">Scheduled talk ID</param>
    /// <returns>Full toolbox talk content with progress</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MyToolboxTalkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTalkById(Guid id)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var query = new GetMyToolboxTalkByIdQuery
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = employeeId.Value,
                ScheduledTalkId = id
            };

            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { message = "Toolbox talk not found or not assigned to you" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my toolbox talk {ScheduledTalkId}", id);
            return StatusCode(500, new { message = "Error retrieving toolbox talk" });
        }
    }

    /// <summary>
    /// Mark a section as read
    /// </summary>
    /// <param name="id">Scheduled talk ID</param>
    /// <param name="sectionId">Section ID to mark as read</param>
    /// <param name="request">Optional time spent data</param>
    /// <returns>Updated section progress</returns>
    [HttpPost("{id:guid}/sections/{sectionId:guid}/read")]
    [ProducesResponseType(typeof(ScheduledTalkSectionProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkSectionRead(
        Guid id,
        Guid sectionId,
        [FromBody] MarkSectionReadRequest? request = null)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var command = new MarkSectionReadCommand
            {
                ScheduledTalkId = id,
                SectionId = sectionId,
                TimeSpentSeconds = request?.TimeSpentSeconds
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking section {SectionId} as read for talk {ScheduledTalkId}", sectionId, id);
            return StatusCode(500, new { message = "Error marking section as read" });
        }
    }

    /// <summary>
    /// Submit quiz answers for a toolbox talk
    /// </summary>
    /// <param name="id">Scheduled talk ID</param>
    /// <param name="request">Quiz answers</param>
    /// <returns>Quiz result with score and pass/fail status</returns>
    [HttpPost("{id:guid}/quiz/submit")]
    [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitQuiz(Guid id, [FromBody] SubmitQuizRequest request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var command = new SubmitQuizAnswersCommand
            {
                ScheduledTalkId = id,
                Answers = request.Answers
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quiz for talk {ScheduledTalkId}", id);
            return StatusCode(500, new { message = "Error submitting quiz" });
        }
    }

    /// <summary>
    /// Update video watch progress
    /// </summary>
    /// <param name="id">Scheduled talk ID</param>
    /// <param name="request">Video progress data</param>
    /// <returns>Updated progress status</returns>
    [HttpPost("{id:guid}/video-progress")]
    [ProducesResponseType(typeof(VideoProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVideoProgress(Guid id, [FromBody] UpdateVideoProgressRequest request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var command = new UpdateVideoProgressCommand
            {
                ScheduledTalkId = id,
                WatchPercent = request.WatchPercent
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating video progress for talk {ScheduledTalkId}", id);
            return StatusCode(500, new { message = "Error updating video progress" });
        }
    }

    /// <summary>
    /// Complete a toolbox talk with signature
    /// </summary>
    /// <param name="id">Scheduled talk ID</param>
    /// <param name="request">Completion data including signature</param>
    /// <returns>Completion confirmation with certificate details</returns>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ScheduledTalkCompletionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteToolboxTalkRequest request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var command = new CompleteToolboxTalkCommand
            {
                ScheduledTalkId = id,
                SignatureData = request.SignatureData,
                SignedByName = request.SignedByName
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing talk {ScheduledTalkId}", id);
            return StatusCode(500, new { message = "Error completing toolbox talk" });
        }
    }

    /// <summary>
    /// Get pending (not started) toolbox talks for the current employee
    /// </summary>
    /// <returns>List of pending toolbox talks</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PaginatedList<MyToolboxTalkListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var query = new GetMyToolboxTalksQuery
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = employeeId.Value,
                Status = ScheduledTalkStatus.Pending,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending toolbox talks");
            return StatusCode(500, Result.Fail("Error retrieving pending toolbox talks"));
        }
    }

    /// <summary>
    /// Get in-progress toolbox talks for the current employee
    /// </summary>
    /// <returns>List of in-progress toolbox talks</returns>
    [HttpGet("in-progress")]
    [ProducesResponseType(typeof(PaginatedList<MyToolboxTalkListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInProgress(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var query = new GetMyToolboxTalksQuery
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = employeeId.Value,
                Status = ScheduledTalkStatus.InProgress,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving in-progress toolbox talks");
            return StatusCode(500, Result.Fail("Error retrieving in-progress toolbox talks"));
        }
    }

    /// <summary>
    /// Get overdue toolbox talks for the current employee
    /// </summary>
    /// <returns>List of overdue toolbox talks</returns>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PaginatedList<MyToolboxTalkListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var query = new GetMyToolboxTalksQuery
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = employeeId.Value,
                Status = ScheduledTalkStatus.Overdue,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue toolbox talks");
            return StatusCode(500, Result.Fail("Error retrieving overdue toolbox talks"));
        }
    }

    /// <summary>
    /// Get completed toolbox talks for the current employee
    /// </summary>
    /// <returns>List of completed toolbox talks</returns>
    [HttpGet("completed")]
    [ProducesResponseType(typeof(PaginatedList<MyToolboxTalkListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompleted(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                return BadRequest(new { message = "No employee record associated with current user" });
            }

            var query = new GetMyToolboxTalksQuery
            {
                TenantId = _currentUserService.TenantId,
                EmployeeId = employeeId.Value,
                Status = ScheduledTalkStatus.Completed,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving completed toolbox talks");
            return StatusCode(500, Result.Fail("Error retrieving completed toolbox talks"));
        }
    }

    /// <summary>
    /// Get current employee ID from claims
    /// </summary>
    private Guid? GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("employee_id")?.Value;
        if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId))
        {
            return null;
        }
        return employeeId;
    }
}

#region Request DTOs

/// <summary>
/// Request for marking a section as read
/// </summary>
public record MarkSectionReadRequest
{
    /// <summary>
    /// Optional time spent on this section in seconds
    /// </summary>
    public int? TimeSpentSeconds { get; init; }
}

/// <summary>
/// Request for submitting quiz answers
/// </summary>
public record SubmitQuizRequest
{
    /// <summary>
    /// Dictionary of question ID to submitted answer
    /// </summary>
    public Dictionary<Guid, string> Answers { get; init; } = new();
}

/// <summary>
/// Request for updating video watch progress
/// </summary>
public record UpdateVideoProgressRequest
{
    /// <summary>
    /// Current watch progress percentage (0-100)
    /// </summary>
    public int WatchPercent { get; init; }
}

/// <summary>
/// Request for completing a toolbox talk
/// </summary>
public record CompleteToolboxTalkRequest
{
    /// <summary>
    /// Base64 encoded signature image (PNG)
    /// </summary>
    public string SignatureData { get; init; } = string.Empty;

    /// <summary>
    /// Name entered by the employee when signing
    /// </summary>
    public string SignedByName { get; init; } = string.Empty;
}

#endregion
