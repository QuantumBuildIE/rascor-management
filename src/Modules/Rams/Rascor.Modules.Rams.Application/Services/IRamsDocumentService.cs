using Rascor.Core.Application.Models;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for RAMS document operations
/// </summary>
public interface IRamsDocumentService
{
    /// <summary>
    /// Get all RAMS documents with optional status filter
    /// </summary>
    Task<PaginatedList<RamsDocumentListDto>> GetDocumentsAsync(
        string? search = null,
        RamsStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortColumn = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a RAMS document by ID with full details
    /// </summary>
    Task<RamsDocumentDto?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new RAMS document
    /// </summary>
    Task<RamsDocumentDto> CreateDocumentAsync(CreateRamsDocumentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing RAMS document
    /// </summary>
    Task<RamsDocumentDto> UpdateDocumentAsync(Guid id, UpdateRamsDocumentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a RAMS document (soft delete)
    /// </summary>
    Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a RAMS document for review
    /// </summary>
    Task<RamsDocumentDto> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a RAMS document
    /// </summary>
    Task<RamsDocumentDto> ApproveAsync(Guid id, ApprovalDto? dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject a RAMS document
    /// </summary>
    Task<RamsDocumentDto> RejectAsync(Guid id, ApprovalDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a project reference already exists
    /// </summary>
    Task<bool> ProjectReferenceExistsAsync(string projectReference, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
