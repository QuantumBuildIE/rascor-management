using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.API.Controllers;

/// <summary>
/// API endpoints for AI-powered RAMS features
/// </summary>
[ApiController]
[Route("api/rams/ai")]
[Authorize]
public class RamsAiController : ControllerBase
{
    private readonly IRamsAiService _aiService;
    private readonly ILogger<RamsAiController> _logger;

    public RamsAiController(
        IRamsAiService aiService,
        ILogger<RamsAiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Get AI-powered control measure suggestions for a risk assessment
    /// </summary>
    /// <param name="request">The suggestion request containing task and hazard details</param>
    /// <param name="ramsDocumentId">Optional RAMS document ID for context and audit</param>
    /// <param name="riskAssessmentId">Optional risk assessment ID for context and audit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Control measure suggestions from library and/or AI</returns>
    [HttpPost("suggest-controls")]
    public async Task<ActionResult<ControlMeasureSuggestionResponseDto>> SuggestControls(
        [FromBody] ControlMeasureSuggestionRequestDto request,
        [FromQuery] Guid? ramsDocumentId = null,
        [FromQuery] Guid? riskAssessmentId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Control measure suggestion requested for task: {Task}, hazard: {Hazard}",
                request.TaskActivity, request.HazardIdentified);

            var result = await _aiService.GetControlMeasureSuggestionsAsync(
                request, ramsDocumentId, riskAssessmentId, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting control measure suggestions");
            return StatusCode(500, new { message = "Error getting control measure suggestions" });
        }
    }

    /// <summary>
    /// Mark whether a suggestion was accepted by the user (for analytics)
    /// </summary>
    /// <param name="dto">The acceptance details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("accept-suggestion")]
    public async Task<IActionResult> AcceptSuggestion(
        [FromBody] AcceptSuggestionDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _aiService.MarkSuggestionAcceptedAsync(dto.AuditLogId, dto.Accepted, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking suggestion as accepted");
            return StatusCode(500, new { message = "Error marking suggestion as accepted" });
        }
    }
}
