using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for RAMS reference library operations (Hazards, Controls, Legislation, SOPs)
/// </summary>
public interface IRamsLibraryService
{
    #region Hazards

    /// <summary>
    /// Get all hazards with optional filtering
    /// </summary>
    Task<IReadOnlyList<HazardLibraryDto>> GetAllHazardsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get hazards by category
    /// </summary>
    Task<IReadOnlyList<HazardLibraryDto>> GetHazardsByCategoryAsync(
        HazardCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search hazards by term
    /// </summary>
    Task<IReadOnlyList<HazardLibraryDto>> SearchHazardsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a hazard by ID
    /// </summary>
    Task<HazardLibraryDto?> GetHazardByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new hazard
    /// </summary>
    Task<HazardLibraryDto> CreateHazardAsync(CreateHazardLibraryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a hazard
    /// </summary>
    Task<HazardLibraryDto> UpdateHazardAsync(Guid id, UpdateHazardLibraryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a hazard
    /// </summary>
    Task DeleteHazardAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if hazard code exists
    /// </summary>
    Task<bool> HazardCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Controls

    /// <summary>
    /// Get all control measures with optional filtering
    /// </summary>
    Task<IReadOnlyList<ControlMeasureLibraryDto>> GetAllControlsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get controls by hazard category
    /// </summary>
    Task<IReadOnlyList<ControlMeasureLibraryDto>> GetControlsByCategoryAsync(
        HazardCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get controls by hierarchy level
    /// </summary>
    Task<IReadOnlyList<ControlMeasureLibraryDto>> GetControlsByHierarchyAsync(
        ControlHierarchy hierarchy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search controls by term
    /// </summary>
    Task<IReadOnlyList<ControlMeasureLibraryDto>> SearchControlsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a control by ID
    /// </summary>
    Task<ControlMeasureLibraryDto?> GetControlByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new control measure
    /// </summary>
    Task<ControlMeasureLibraryDto> CreateControlAsync(CreateControlMeasureLibraryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a control measure
    /// </summary>
    Task<ControlMeasureLibraryDto> UpdateControlAsync(Guid id, UpdateControlMeasureLibraryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a control measure
    /// </summary>
    Task DeleteControlAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if control code exists
    /// </summary>
    Task<bool> ControlCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Legislation

    /// <summary>
    /// Get all legislation references with optional filtering
    /// </summary>
    Task<IReadOnlyList<LegislationReferenceDto>> GetAllLegislationAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get legislation by jurisdiction
    /// </summary>
    Task<IReadOnlyList<LegislationReferenceDto>> GetLegislationByJurisdictionAsync(
        string jurisdiction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search legislation by term
    /// </summary>
    Task<IReadOnlyList<LegislationReferenceDto>> SearchLegislationAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a legislation reference by ID
    /// </summary>
    Task<LegislationReferenceDto?> GetLegislationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new legislation reference
    /// </summary>
    Task<LegislationReferenceDto> CreateLegislationAsync(CreateLegislationReferenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a legislation reference
    /// </summary>
    Task<LegislationReferenceDto> UpdateLegislationAsync(Guid id, UpdateLegislationReferenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a legislation reference
    /// </summary>
    Task DeleteLegislationAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if legislation code exists
    /// </summary>
    Task<bool> LegislationCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    #endregion

    #region SOPs

    /// <summary>
    /// Get all SOP references with optional filtering
    /// </summary>
    Task<IReadOnlyList<SopReferenceDto>> GetAllSopsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search SOPs by term
    /// </summary>
    Task<IReadOnlyList<SopReferenceDto>> SearchSopsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a SOP reference by ID
    /// </summary>
    Task<SopReferenceDto?> GetSopByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new SOP reference
    /// </summary>
    Task<SopReferenceDto> CreateSopAsync(CreateSopReferenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a SOP reference
    /// </summary>
    Task<SopReferenceDto> UpdateSopAsync(Guid id, UpdateSopReferenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a SOP reference
    /// </summary>
    Task DeleteSopAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if SOP ID exists
    /// </summary>
    Task<bool> SopIdExistsAsync(string sopId, Guid? excludeId = null, CancellationToken cancellationToken = default);

    #endregion
}
