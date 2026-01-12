using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for AI-powered control measure suggestions
/// </summary>
public interface IRamsAiService
{
    /// <summary>
    /// Get AI-powered control measure suggestions based on task and hazard information
    /// </summary>
    /// <param name="request">The suggestion request containing task and hazard details</param>
    /// <param name="ramsDocumentId">Optional RAMS document ID for context</param>
    /// <param name="riskAssessmentId">Optional risk assessment ID for context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Control measure suggestions from library and/or AI</returns>
    Task<ControlMeasureSuggestionResponseDto> GetControlMeasureSuggestionsAsync(
        ControlMeasureSuggestionRequestDto request,
        Guid? ramsDocumentId = null,
        Guid? riskAssessmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a suggestion as accepted or rejected for tracking purposes
    /// </summary>
    /// <param name="auditLogId">The audit log ID from the suggestion response</param>
    /// <param name="accepted">Whether the suggestion was accepted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkSuggestionAcceptedAsync(
        Guid auditLogId,
        bool accepted,
        CancellationToken cancellationToken = default);
}
