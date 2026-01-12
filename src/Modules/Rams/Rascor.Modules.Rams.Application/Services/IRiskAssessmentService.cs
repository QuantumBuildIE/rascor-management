using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for Risk Assessment operations within RAMS documents
/// </summary>
public interface IRiskAssessmentService
{
    /// <summary>
    /// Get all risk assessments for a RAMS document
    /// </summary>
    Task<IReadOnlyList<RiskAssessmentDto>> GetByRamsDocumentIdAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a risk assessment by ID
    /// </summary>
    Task<RiskAssessmentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new risk assessment
    /// </summary>
    Task<RiskAssessmentDto> CreateAsync(
        Guid ramsDocumentId,
        CreateRiskAssessmentDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing risk assessment
    /// </summary>
    Task<RiskAssessmentDto> UpdateAsync(
        Guid id,
        UpdateRiskAssessmentDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a risk assessment
    /// </summary>
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorder risk assessments within a RAMS document
    /// </summary>
    Task<IReadOnlyList<RiskAssessmentDto>> ReorderAsync(
        Guid ramsDocumentId,
        ReorderRiskAssessmentsDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a RAMS document exists
    /// </summary>
    Task<bool> RamsDocumentExistsAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default);
}
