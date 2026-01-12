using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for Method Step operations within RAMS documents
/// </summary>
public interface IMethodStepService
{
    /// <summary>
    /// Get all method steps for a RAMS document
    /// </summary>
    Task<IReadOnlyList<MethodStepDto>> GetByRamsDocumentIdAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a method step by ID
    /// </summary>
    Task<MethodStepDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new method step
    /// </summary>
    Task<MethodStepDto> CreateAsync(
        Guid ramsDocumentId,
        CreateMethodStepDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing method step
    /// </summary>
    Task<MethodStepDto> UpdateAsync(
        Guid id,
        UpdateMethodStepDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a method step
    /// </summary>
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorder method steps within a RAMS document
    /// </summary>
    Task<IReadOnlyList<MethodStepDto>> ReorderAsync(
        Guid ramsDocumentId,
        ReorderMethodStepsDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert a method step at a specific position
    /// </summary>
    Task<MethodStepDto> InsertAtAsync(
        Guid ramsDocumentId,
        int position,
        CreateMethodStepDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a RAMS document exists
    /// </summary>
    Task<bool> RamsDocumentExistsAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default);
}
