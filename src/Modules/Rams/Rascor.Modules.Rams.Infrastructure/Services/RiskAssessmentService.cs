using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for Risk Assessment operations
/// </summary>
public class RiskAssessmentService : IRiskAssessmentService
{
    private readonly IRamsDbContext _context;
    private readonly ILogger<RiskAssessmentService> _logger;

    public RiskAssessmentService(
        IRamsDbContext context,
        ILogger<RiskAssessmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RiskAssessmentDto>> GetByRamsDocumentIdAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default)
    {
        var assessments = await _context.RamsRiskAssessments
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return assessments.Select(MapToDto).ToList();
    }

    public async Task<RiskAssessmentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var assessment = await _context.RamsRiskAssessments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return assessment == null ? null : MapToDto(assessment);
    }

    public async Task<RiskAssessmentDto> CreateAsync(
        Guid ramsDocumentId,
        CreateRiskAssessmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == ramsDocumentId, cancellationToken);

        if (ramsDocument == null)
            throw new InvalidOperationException("RAMS document not found");

        // Can only add to Draft or Rejected documents
        if (ramsDocument.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only add risk assessments to documents in Draft or Rejected status");

        // Get next sort order if not specified
        var sortOrder = dto.SortOrder ?? await GetNextSortOrderAsync(ramsDocumentId, cancellationToken);

        var assessment = new RiskAssessment
        {
            RamsDocumentId = ramsDocumentId,
            TaskActivity = dto.TaskActivity,
            LocationArea = dto.LocationArea,
            HazardIdentified = dto.HazardIdentified,
            WhoAtRisk = dto.WhoAtRisk,
            InitialLikelihood = dto.InitialLikelihood,
            InitialSeverity = dto.InitialSeverity,
            ControlMeasures = dto.ControlMeasures,
            RelevantLegislation = dto.RelevantLegislation,
            ReferenceSops = dto.ReferenceSops,
            ResidualLikelihood = dto.ResidualLikelihood,
            ResidualSeverity = dto.ResidualSeverity,
            SortOrder = sortOrder,
            IsAiGenerated = false
        };

        _context.RamsRiskAssessments.Add(assessment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created risk assessment {Id} for RAMS document {RamsDocumentId}",
            assessment.Id, ramsDocumentId);

        return MapToDto(assessment);
    }

    public async Task<RiskAssessmentDto> UpdateAsync(
        Guid id,
        UpdateRiskAssessmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var assessment = await _context.RamsRiskAssessments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (assessment == null)
            throw new InvalidOperationException("Risk assessment not found");

        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == assessment.RamsDocumentId, cancellationToken);

        // Can only edit in Draft or Rejected documents
        if (ramsDocument!.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only edit risk assessments in documents with Draft or Rejected status");

        assessment.TaskActivity = dto.TaskActivity;
        assessment.LocationArea = dto.LocationArea;
        assessment.HazardIdentified = dto.HazardIdentified;
        assessment.WhoAtRisk = dto.WhoAtRisk;
        assessment.InitialLikelihood = dto.InitialLikelihood;
        assessment.InitialSeverity = dto.InitialSeverity;
        assessment.ControlMeasures = dto.ControlMeasures;
        assessment.RelevantLegislation = dto.RelevantLegislation;
        assessment.ReferenceSops = dto.ReferenceSops;
        assessment.ResidualLikelihood = dto.ResidualLikelihood;
        assessment.ResidualSeverity = dto.ResidualSeverity;

        if (dto.SortOrder.HasValue)
            assessment.SortOrder = dto.SortOrder.Value;

        // Clear AI flag if manually edited
        if (assessment.IsAiGenerated)
        {
            assessment.IsAiGenerated = false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated risk assessment {Id}", id);

        return MapToDto(assessment);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var assessment = await _context.RamsRiskAssessments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (assessment == null)
            throw new InvalidOperationException("Risk assessment not found");

        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == assessment.RamsDocumentId, cancellationToken);

        // Can only delete from Draft or Rejected documents
        if (ramsDocument!.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only delete risk assessments from documents in Draft or Rejected status");

        _context.RamsRiskAssessments.Remove(assessment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted risk assessment {Id}", id);
    }

    public async Task<IReadOnlyList<RiskAssessmentDto>> ReorderAsync(
        Guid ramsDocumentId,
        ReorderRiskAssessmentsDto dto,
        CancellationToken cancellationToken = default)
    {
        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == ramsDocumentId, cancellationToken);

        if (ramsDocument == null)
            throw new InvalidOperationException("RAMS document not found");

        if (ramsDocument.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only reorder risk assessments in documents with Draft or Rejected status");

        var assessments = await _context.RamsRiskAssessments
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .ToListAsync(cancellationToken);

        var assessmentDict = assessments.ToDictionary(a => a.Id);

        // Validate all IDs belong to this document
        if (!dto.OrderedIds.All(id => assessmentDict.ContainsKey(id)))
            throw new InvalidOperationException("One or more risk assessment IDs are invalid");

        // Update sort orders
        for (int i = 0; i < dto.OrderedIds.Count; i++)
        {
            var assessment = assessmentDict[dto.OrderedIds[i]];
            assessment.SortOrder = i + 1;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reordered {Count} risk assessments for RAMS document {RamsDocumentId}",
            dto.OrderedIds.Count, ramsDocumentId);

        // Return updated list
        return await GetByRamsDocumentIdAsync(ramsDocumentId, cancellationToken);
    }

    public async Task<bool> RamsDocumentExistsAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RamsDocuments
            .AnyAsync(d => d.Id == ramsDocumentId, cancellationToken);
    }

    private async Task<int> GetNextSortOrderAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken)
    {
        var maxOrder = await _context.RamsRiskAssessments
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .MaxAsync(x => (int?)x.SortOrder, cancellationToken);

        return (maxOrder ?? 0) + 1;
    }

    private static RiskAssessmentDto MapToDto(RiskAssessment assessment) => new()
    {
        Id = assessment.Id,
        RamsDocumentId = assessment.RamsDocumentId,
        TaskActivity = assessment.TaskActivity,
        LocationArea = assessment.LocationArea,
        HazardIdentified = assessment.HazardIdentified,
        WhoAtRisk = assessment.WhoAtRisk,
        InitialLikelihood = assessment.InitialLikelihood,
        InitialSeverity = assessment.InitialSeverity,
        InitialRiskRating = assessment.InitialRiskRating,
        InitialRiskLevel = assessment.InitialRiskLevel,
        ControlMeasures = assessment.ControlMeasures,
        RelevantLegislation = assessment.RelevantLegislation,
        ReferenceSops = assessment.ReferenceSops,
        ResidualLikelihood = assessment.ResidualLikelihood,
        ResidualSeverity = assessment.ResidualSeverity,
        ResidualRiskRating = assessment.ResidualRiskRating,
        ResidualRiskLevel = assessment.ResidualRiskLevel,
        IsAiGenerated = assessment.IsAiGenerated,
        AiGeneratedAt = assessment.AiGeneratedAt,
        SortOrder = assessment.SortOrder,
        CreatedAt = assessment.CreatedAt,
        ModifiedAt = assessment.UpdatedAt
    };
}
