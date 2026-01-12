using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for RAMS reference library operations
/// </summary>
public class RamsLibraryService : IRamsLibraryService
{
    private readonly IRamsDbContext _context;
    private readonly ILogger<RamsLibraryService> _logger;

    public RamsLibraryService(
        IRamsDbContext context,
        ILogger<RamsLibraryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Hazards

    public async Task<IReadOnlyList<HazardLibraryDto>> GetAllHazardsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RamsHazardLibrary.AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var hazards = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return hazards.Select(MapToHazardDto).ToList();
    }

    public async Task<IReadOnlyList<HazardLibraryDto>> GetHazardsByCategoryAsync(
        HazardCategory category,
        CancellationToken cancellationToken = default)
    {
        var hazards = await _context.RamsHazardLibrary
            .Where(x => x.IsActive && x.Category == category)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return hazards.Select(MapToHazardDto).ToList();
    }

    public async Task<IReadOnlyList<HazardLibraryDto>> SearchHazardsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        var hazards = await _context.RamsHazardLibrary
            .Where(x => x.IsActive && (
                x.Name.ToLower().Contains(term) ||
                x.Code.ToLower().Contains(term) ||
                (x.Keywords != null && x.Keywords.ToLower().Contains(term)) ||
                (x.Description != null && x.Description.ToLower().Contains(term))
            ))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return hazards.Select(MapToHazardDto).ToList();
    }

    public async Task<HazardLibraryDto?> GetHazardByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hazard = await _context.RamsHazardLibrary
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return hazard == null ? null : MapToHazardDto(hazard);
    }

    public async Task<HazardLibraryDto> CreateHazardAsync(CreateHazardLibraryDto dto, CancellationToken cancellationToken = default)
    {
        if (await HazardCodeExistsAsync(dto.Code, null, cancellationToken))
            throw new InvalidOperationException("Hazard code already exists");

        var hazard = new HazardLibrary
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Keywords = dto.Keywords,
            DefaultLikelihood = dto.DefaultLikelihood,
            DefaultSeverity = dto.DefaultSeverity,
            TypicalWhoAtRisk = dto.TypicalWhoAtRisk,
            SortOrder = dto.SortOrder,
            IsActive = true
        };

        _context.RamsHazardLibrary.Add(hazard);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created hazard {Code}: {Name}", hazard.Code, hazard.Name);

        return MapToHazardDto(hazard);
    }

    public async Task<HazardLibraryDto> UpdateHazardAsync(Guid id, UpdateHazardLibraryDto dto, CancellationToken cancellationToken = default)
    {
        var hazard = await _context.RamsHazardLibrary
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (hazard == null)
            throw new InvalidOperationException("Hazard not found");

        if (await HazardCodeExistsAsync(dto.Code, id, cancellationToken))
            throw new InvalidOperationException("Hazard code already exists");

        hazard.Code = dto.Code;
        hazard.Name = dto.Name;
        hazard.Description = dto.Description;
        hazard.Category = dto.Category;
        hazard.Keywords = dto.Keywords;
        hazard.DefaultLikelihood = dto.DefaultLikelihood;
        hazard.DefaultSeverity = dto.DefaultSeverity;
        hazard.TypicalWhoAtRisk = dto.TypicalWhoAtRisk;
        hazard.IsActive = dto.IsActive;
        hazard.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated hazard {Id}", id);

        return MapToHazardDto(hazard);
    }

    public async Task DeleteHazardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hazard = await _context.RamsHazardLibrary
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (hazard == null)
            throw new InvalidOperationException("Hazard not found");

        _context.RamsHazardLibrary.Remove(hazard);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted hazard {Id}", id);
    }

    public async Task<bool> HazardCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RamsHazardLibrary.Where(x => x.Code == code);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Controls

    public async Task<IReadOnlyList<ControlMeasureLibraryDto>> GetAllControlsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RamsControlMeasureLibrary.AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var controls = await query
            .OrderBy(x => x.Hierarchy)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return controls.Select(MapToControlDto).ToList();
    }

    public async Task<IReadOnlyList<ControlMeasureLibraryDto>> GetControlsByCategoryAsync(
        HazardCategory category,
        CancellationToken cancellationToken = default)
    {
        var controls = await _context.RamsControlMeasureLibrary
            .Where(x => x.IsActive && (x.ApplicableToCategory == null || x.ApplicableToCategory == category))
            .OrderBy(x => x.Hierarchy)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return controls.Select(MapToControlDto).ToList();
    }

    public async Task<IReadOnlyList<ControlMeasureLibraryDto>> GetControlsByHierarchyAsync(
        ControlHierarchy hierarchy,
        CancellationToken cancellationToken = default)
    {
        var controls = await _context.RamsControlMeasureLibrary
            .Where(x => x.IsActive && x.Hierarchy == hierarchy)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return controls.Select(MapToControlDto).ToList();
    }

    public async Task<IReadOnlyList<ControlMeasureLibraryDto>> SearchControlsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        var controls = await _context.RamsControlMeasureLibrary
            .Where(x => x.IsActive && (
                x.Name.ToLower().Contains(term) ||
                x.Code.ToLower().Contains(term) ||
                (x.Keywords != null && x.Keywords.ToLower().Contains(term)) ||
                x.Description.ToLower().Contains(term)
            ))
            .OrderBy(x => x.Hierarchy)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return controls.Select(MapToControlDto).ToList();
    }

    public async Task<ControlMeasureLibraryDto?> GetControlByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var control = await _context.RamsControlMeasureLibrary
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return control == null ? null : MapToControlDto(control);
    }

    public async Task<ControlMeasureLibraryDto> CreateControlAsync(CreateControlMeasureLibraryDto dto, CancellationToken cancellationToken = default)
    {
        if (await ControlCodeExistsAsync(dto.Code, null, cancellationToken))
            throw new InvalidOperationException("Control code already exists");

        var control = new ControlMeasureLibrary
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            Hierarchy = dto.Hierarchy,
            ApplicableToCategory = dto.ApplicableToCategory,
            Keywords = dto.Keywords,
            TypicalLikelihoodReduction = dto.TypicalLikelihoodReduction,
            TypicalSeverityReduction = dto.TypicalSeverityReduction,
            SortOrder = dto.SortOrder,
            IsActive = true
        };

        _context.RamsControlMeasureLibrary.Add(control);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created control {Code}: {Name}", control.Code, control.Name);

        return MapToControlDto(control);
    }

    public async Task<ControlMeasureLibraryDto> UpdateControlAsync(Guid id, UpdateControlMeasureLibraryDto dto, CancellationToken cancellationToken = default)
    {
        var control = await _context.RamsControlMeasureLibrary
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (control == null)
            throw new InvalidOperationException("Control not found");

        if (await ControlCodeExistsAsync(dto.Code, id, cancellationToken))
            throw new InvalidOperationException("Control code already exists");

        control.Code = dto.Code;
        control.Name = dto.Name;
        control.Description = dto.Description;
        control.Hierarchy = dto.Hierarchy;
        control.ApplicableToCategory = dto.ApplicableToCategory;
        control.Keywords = dto.Keywords;
        control.TypicalLikelihoodReduction = dto.TypicalLikelihoodReduction;
        control.TypicalSeverityReduction = dto.TypicalSeverityReduction;
        control.IsActive = dto.IsActive;
        control.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated control {Id}", id);

        return MapToControlDto(control);
    }

    public async Task DeleteControlAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var control = await _context.RamsControlMeasureLibrary
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (control == null)
            throw new InvalidOperationException("Control not found");

        _context.RamsControlMeasureLibrary.Remove(control);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted control {Id}", id);
    }

    public async Task<bool> ControlCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RamsControlMeasureLibrary.Where(x => x.Code == code);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Legislation

    public async Task<IReadOnlyList<LegislationReferenceDto>> GetAllLegislationAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RamsLegislationReferences.AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var legislation = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return legislation.Select(MapToLegislationDto).ToList();
    }

    public async Task<IReadOnlyList<LegislationReferenceDto>> GetLegislationByJurisdictionAsync(
        string jurisdiction,
        CancellationToken cancellationToken = default)
    {
        var legislation = await _context.RamsLegislationReferences
            .Where(x => x.IsActive && x.Jurisdiction == jurisdiction)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return legislation.Select(MapToLegislationDto).ToList();
    }

    public async Task<IReadOnlyList<LegislationReferenceDto>> SearchLegislationAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        var legislation = await _context.RamsLegislationReferences
            .Where(x => x.IsActive && (
                x.Name.ToLower().Contains(term) ||
                x.Code.ToLower().Contains(term) ||
                (x.ShortName != null && x.ShortName.ToLower().Contains(term)) ||
                (x.Keywords != null && x.Keywords.ToLower().Contains(term)) ||
                (x.Description != null && x.Description.ToLower().Contains(term))
            ))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return legislation.Select(MapToLegislationDto).ToList();
    }

    public async Task<LegislationReferenceDto?> GetLegislationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var legislation = await _context.RamsLegislationReferences
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return legislation == null ? null : MapToLegislationDto(legislation);
    }

    public async Task<LegislationReferenceDto> CreateLegislationAsync(CreateLegislationReferenceDto dto, CancellationToken cancellationToken = default)
    {
        if (await LegislationCodeExistsAsync(dto.Code, null, cancellationToken))
            throw new InvalidOperationException("Legislation code already exists");

        var legislation = new LegislationReference
        {
            Code = dto.Code,
            Name = dto.Name,
            ShortName = dto.ShortName,
            Description = dto.Description,
            Jurisdiction = dto.Jurisdiction,
            Keywords = dto.Keywords,
            DocumentUrl = dto.DocumentUrl,
            ApplicableCategories = dto.ApplicableCategories,
            SortOrder = dto.SortOrder,
            IsActive = true
        };

        _context.RamsLegislationReferences.Add(legislation);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created legislation {Code}: {Name}", legislation.Code, legislation.Name);

        return MapToLegislationDto(legislation);
    }

    public async Task<LegislationReferenceDto> UpdateLegislationAsync(Guid id, UpdateLegislationReferenceDto dto, CancellationToken cancellationToken = default)
    {
        var legislation = await _context.RamsLegislationReferences
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (legislation == null)
            throw new InvalidOperationException("Legislation not found");

        if (await LegislationCodeExistsAsync(dto.Code, id, cancellationToken))
            throw new InvalidOperationException("Legislation code already exists");

        legislation.Code = dto.Code;
        legislation.Name = dto.Name;
        legislation.ShortName = dto.ShortName;
        legislation.Description = dto.Description;
        legislation.Jurisdiction = dto.Jurisdiction;
        legislation.Keywords = dto.Keywords;
        legislation.DocumentUrl = dto.DocumentUrl;
        legislation.ApplicableCategories = dto.ApplicableCategories;
        legislation.IsActive = dto.IsActive;
        legislation.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated legislation {Id}", id);

        return MapToLegislationDto(legislation);
    }

    public async Task DeleteLegislationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var legislation = await _context.RamsLegislationReferences
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (legislation == null)
            throw new InvalidOperationException("Legislation not found");

        _context.RamsLegislationReferences.Remove(legislation);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted legislation {Id}", id);
    }

    public async Task<bool> LegislationCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RamsLegislationReferences.Where(x => x.Code == code);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region SOPs

    public async Task<IReadOnlyList<SopReferenceDto>> GetAllSopsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RamsSopReferences.AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var sops = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.SopId)
            .ToListAsync(cancellationToken);

        return sops.Select(MapToSopDto).ToList();
    }

    public async Task<IReadOnlyList<SopReferenceDto>> SearchSopsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        var sops = await _context.RamsSopReferences
            .Where(x => x.IsActive && (
                x.SopId.ToLower().Contains(term) ||
                x.Topic.ToLower().Contains(term) ||
                (x.TaskKeywords != null && x.TaskKeywords.ToLower().Contains(term)) ||
                (x.Description != null && x.Description.ToLower().Contains(term))
            ))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.SopId)
            .ToListAsync(cancellationToken);

        return sops.Select(MapToSopDto).ToList();
    }

    public async Task<SopReferenceDto?> GetSopByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sop = await _context.RamsSopReferences
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return sop == null ? null : MapToSopDto(sop);
    }

    public async Task<SopReferenceDto> CreateSopAsync(CreateSopReferenceDto dto, CancellationToken cancellationToken = default)
    {
        if (await SopIdExistsAsync(dto.SopId, null, cancellationToken))
            throw new InvalidOperationException("SOP ID already exists");

        var sop = new SopReference
        {
            SopId = dto.SopId,
            Topic = dto.Topic,
            Description = dto.Description,
            TaskKeywords = dto.TaskKeywords,
            PolicySnippet = dto.PolicySnippet,
            ProcedureDetails = dto.ProcedureDetails,
            ApplicableLegislation = dto.ApplicableLegislation,
            DocumentUrl = dto.DocumentUrl,
            SortOrder = dto.SortOrder,
            IsActive = true
        };

        _context.RamsSopReferences.Add(sop);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created SOP {SopId}: {Topic}", sop.SopId, sop.Topic);

        return MapToSopDto(sop);
    }

    public async Task<SopReferenceDto> UpdateSopAsync(Guid id, UpdateSopReferenceDto dto, CancellationToken cancellationToken = default)
    {
        var sop = await _context.RamsSopReferences
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (sop == null)
            throw new InvalidOperationException("SOP not found");

        if (await SopIdExistsAsync(dto.SopId, id, cancellationToken))
            throw new InvalidOperationException("SOP ID already exists");

        sop.SopId = dto.SopId;
        sop.Topic = dto.Topic;
        sop.Description = dto.Description;
        sop.TaskKeywords = dto.TaskKeywords;
        sop.PolicySnippet = dto.PolicySnippet;
        sop.ProcedureDetails = dto.ProcedureDetails;
        sop.ApplicableLegislation = dto.ApplicableLegislation;
        sop.DocumentUrl = dto.DocumentUrl;
        sop.IsActive = dto.IsActive;
        sop.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated SOP {Id}", id);

        return MapToSopDto(sop);
    }

    public async Task DeleteSopAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sop = await _context.RamsSopReferences
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (sop == null)
            throw new InvalidOperationException("SOP not found");

        _context.RamsSopReferences.Remove(sop);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted SOP {Id}", id);
    }

    public async Task<bool> SopIdExistsAsync(string sopId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RamsSopReferences.Where(x => x.SopId == sopId);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Mapping

    private static HazardLibraryDto MapToHazardDto(HazardLibrary h) => new()
    {
        Id = h.Id,
        Code = h.Code,
        Name = h.Name,
        Description = h.Description,
        Category = h.Category,
        Keywords = h.Keywords,
        DefaultLikelihood = h.DefaultLikelihood,
        DefaultSeverity = h.DefaultSeverity,
        TypicalWhoAtRisk = h.TypicalWhoAtRisk,
        IsActive = h.IsActive,
        SortOrder = h.SortOrder
    };

    private static ControlMeasureLibraryDto MapToControlDto(ControlMeasureLibrary c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Name = c.Name,
        Description = c.Description,
        Hierarchy = c.Hierarchy,
        ApplicableToCategory = c.ApplicableToCategory,
        Keywords = c.Keywords,
        TypicalLikelihoodReduction = c.TypicalLikelihoodReduction,
        TypicalSeverityReduction = c.TypicalSeverityReduction,
        IsActive = c.IsActive,
        SortOrder = c.SortOrder
    };

    private static LegislationReferenceDto MapToLegislationDto(LegislationReference l) => new()
    {
        Id = l.Id,
        Code = l.Code,
        Name = l.Name,
        ShortName = l.ShortName,
        Description = l.Description,
        Jurisdiction = l.Jurisdiction,
        Keywords = l.Keywords,
        DocumentUrl = l.DocumentUrl,
        ApplicableCategories = l.ApplicableCategories,
        IsActive = l.IsActive,
        SortOrder = l.SortOrder
    };

    private static SopReferenceDto MapToSopDto(SopReference s) => new()
    {
        Id = s.Id,
        SopId = s.SopId,
        Topic = s.Topic,
        Description = s.Description,
        TaskKeywords = s.TaskKeywords,
        PolicySnippet = s.PolicySnippet,
        ProcedureDetails = s.ProcedureDetails,
        ApplicableLegislation = s.ApplicableLegislation,
        DocumentUrl = s.DocumentUrl,
        IsActive = s.IsActive,
        SortOrder = s.SortOrder
    };

    #endregion
}
