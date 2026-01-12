using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for Method Step operations
/// </summary>
public class MethodStepService : IMethodStepService
{
    private readonly IRamsDbContext _context;
    private readonly ILogger<MethodStepService> _logger;

    public MethodStepService(
        IRamsDbContext context,
        ILogger<MethodStepService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MethodStepDto>> GetByRamsDocumentIdAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default)
    {
        var steps = await _context.RamsMethodSteps
            .Include(x => x.LinkedRiskAssessment)
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .OrderBy(x => x.StepNumber)
            .ToListAsync(cancellationToken);

        return steps.Select(MapToDto).ToList();
    }

    public async Task<MethodStepDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var step = await _context.RamsMethodSteps
            .Include(x => x.LinkedRiskAssessment)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return step == null ? null : MapToDto(step);
    }

    public async Task<MethodStepDto> CreateAsync(
        Guid ramsDocumentId,
        CreateMethodStepDto dto,
        CancellationToken cancellationToken = default)
    {
        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == ramsDocumentId, cancellationToken);

        if (ramsDocument == null)
            throw new InvalidOperationException("RAMS document not found");

        // Can only add to Draft or Rejected documents
        if (ramsDocument.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only add method steps to documents in Draft or Rejected status");

        // Validate linked risk assessment if provided
        if (dto.LinkedRiskAssessmentId.HasValue)
        {
            var riskAssessment = await _context.RamsRiskAssessments
                .FirstOrDefaultAsync(r => r.Id == dto.LinkedRiskAssessmentId.Value, cancellationToken);
            if (riskAssessment == null || riskAssessment.RamsDocumentId != ramsDocumentId)
                throw new InvalidOperationException("Invalid linked risk assessment");
        }

        // Get next step number if not specified
        var stepNumber = dto.StepNumber ?? await GetNextStepNumberAsync(ramsDocumentId, cancellationToken);

        var step = new MethodStep
        {
            RamsDocumentId = ramsDocumentId,
            StepNumber = stepNumber,
            StepTitle = dto.StepTitle,
            DetailedProcedure = dto.DetailedProcedure,
            LinkedRiskAssessmentId = dto.LinkedRiskAssessmentId,
            RequiredPermits = dto.RequiredPermits,
            RequiresSignoff = dto.RequiresSignoff
        };

        _context.RamsMethodSteps.Add(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created method step {Id} (Step {StepNumber}) for RAMS document {RamsDocumentId}",
            step.Id, step.StepNumber, ramsDocumentId);

        // Reload with linked risk assessment
        var createdStep = await _context.RamsMethodSteps
            .Include(x => x.LinkedRiskAssessment)
            .FirstOrDefaultAsync(x => x.Id == step.Id, cancellationToken);

        return MapToDto(createdStep!);
    }

    public async Task<MethodStepDto> UpdateAsync(
        Guid id,
        UpdateMethodStepDto dto,
        CancellationToken cancellationToken = default)
    {
        var step = await _context.RamsMethodSteps
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (step == null)
            throw new InvalidOperationException("Method step not found");

        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == step.RamsDocumentId, cancellationToken);

        // Can only edit in Draft or Rejected documents
        if (ramsDocument!.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only edit method steps in documents with Draft or Rejected status");

        // Validate linked risk assessment if provided
        if (dto.LinkedRiskAssessmentId.HasValue)
        {
            var riskAssessment = await _context.RamsRiskAssessments
                .FirstOrDefaultAsync(r => r.Id == dto.LinkedRiskAssessmentId.Value, cancellationToken);
            if (riskAssessment == null || riskAssessment.RamsDocumentId != step.RamsDocumentId)
                throw new InvalidOperationException("Invalid linked risk assessment");
        }

        step.StepTitle = dto.StepTitle;
        step.DetailedProcedure = dto.DetailedProcedure;
        step.LinkedRiskAssessmentId = dto.LinkedRiskAssessmentId;
        step.RequiredPermits = dto.RequiredPermits;
        step.RequiresSignoff = dto.RequiresSignoff;

        if (dto.StepNumber.HasValue)
            step.StepNumber = dto.StepNumber.Value;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated method step {Id}", id);

        // Reload with linked risk assessment
        var updatedStep = await _context.RamsMethodSteps
            .Include(x => x.LinkedRiskAssessment)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return MapToDto(updatedStep!);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var step = await _context.RamsMethodSteps
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (step == null)
            throw new InvalidOperationException("Method step not found");

        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == step.RamsDocumentId, cancellationToken);

        // Can only delete from Draft or Rejected documents
        if (ramsDocument!.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only delete method steps from documents in Draft or Rejected status");

        var ramsDocumentId = step.RamsDocumentId;

        _context.RamsMethodSteps.Remove(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted method step {Id}", id);

        // Renumber remaining steps
        await RenumberStepsAsync(ramsDocumentId, cancellationToken);
    }

    public async Task<IReadOnlyList<MethodStepDto>> ReorderAsync(
        Guid ramsDocumentId,
        ReorderMethodStepsDto dto,
        CancellationToken cancellationToken = default)
    {
        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == ramsDocumentId, cancellationToken);

        if (ramsDocument == null)
            throw new InvalidOperationException("RAMS document not found");

        if (ramsDocument.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only reorder method steps in documents with Draft or Rejected status");

        var steps = await _context.RamsMethodSteps
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .ToListAsync(cancellationToken);

        var stepDict = steps.ToDictionary(s => s.Id);

        // Validate all IDs belong to this document
        if (!dto.OrderedIds.All(id => stepDict.ContainsKey(id)))
            throw new InvalidOperationException("One or more method step IDs are invalid");

        // Update step numbers
        for (int i = 0; i < dto.OrderedIds.Count; i++)
        {
            var step = stepDict[dto.OrderedIds[i]];
            step.StepNumber = i + 1;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reordered {Count} method steps for RAMS document {RamsDocumentId}",
            dto.OrderedIds.Count, ramsDocumentId);

        // Return updated list
        return await GetByRamsDocumentIdAsync(ramsDocumentId, cancellationToken);
    }

    public async Task<MethodStepDto> InsertAtAsync(
        Guid ramsDocumentId,
        int position,
        CreateMethodStepDto dto,
        CancellationToken cancellationToken = default)
    {
        var ramsDocument = await _context.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == ramsDocumentId, cancellationToken);

        if (ramsDocument == null)
            throw new InvalidOperationException("RAMS document not found");

        if (ramsDocument.Status != RamsStatus.Draft && ramsDocument.Status != RamsStatus.Rejected)
            throw new InvalidOperationException("Can only add method steps to documents in Draft or Rejected status");

        if (position < 1)
            throw new InvalidOperationException("Position must be at least 1");

        // Validate linked risk assessment if provided
        if (dto.LinkedRiskAssessmentId.HasValue)
        {
            var riskAssessment = await _context.RamsRiskAssessments
                .FirstOrDefaultAsync(r => r.Id == dto.LinkedRiskAssessmentId.Value, cancellationToken);
            if (riskAssessment == null || riskAssessment.RamsDocumentId != ramsDocumentId)
                throw new InvalidOperationException("Invalid linked risk assessment");
        }

        // Shift existing steps
        var existingSteps = await _context.RamsMethodSteps
            .Where(x => x.RamsDocumentId == ramsDocumentId && x.StepNumber >= position)
            .ToListAsync(cancellationToken);

        foreach (var existingStep in existingSteps)
        {
            existingStep.StepNumber += 1;
        }

        // Create new step at position
        var step = new MethodStep
        {
            RamsDocumentId = ramsDocumentId,
            StepNumber = position,
            StepTitle = dto.StepTitle,
            DetailedProcedure = dto.DetailedProcedure,
            LinkedRiskAssessmentId = dto.LinkedRiskAssessmentId,
            RequiredPermits = dto.RequiredPermits,
            RequiresSignoff = dto.RequiresSignoff
        };

        _context.RamsMethodSteps.Add(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inserted method step {Id} at position {Position} for RAMS document {RamsDocumentId}",
            step.Id, position, ramsDocumentId);

        // Reload with linked risk assessment
        var createdStep = await _context.RamsMethodSteps
            .Include(x => x.LinkedRiskAssessment)
            .FirstOrDefaultAsync(x => x.Id == step.Id, cancellationToken);

        return MapToDto(createdStep!);
    }

    public async Task<bool> RamsDocumentExistsAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RamsDocuments
            .AnyAsync(d => d.Id == ramsDocumentId, cancellationToken);
    }

    private async Task<int> GetNextStepNumberAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken)
    {
        var maxStep = await _context.RamsMethodSteps
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .MaxAsync(x => (int?)x.StepNumber, cancellationToken);

        return (maxStep ?? 0) + 1;
    }

    private async Task RenumberStepsAsync(
        Guid ramsDocumentId,
        CancellationToken cancellationToken)
    {
        var steps = await _context.RamsMethodSteps
            .Where(x => x.RamsDocumentId == ramsDocumentId)
            .OrderBy(x => x.StepNumber)
            .ToListAsync(cancellationToken);

        int expectedNumber = 1;
        foreach (var step in steps)
        {
            if (step.StepNumber != expectedNumber)
            {
                step.StepNumber = expectedNumber;
            }
            expectedNumber++;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static MethodStepDto MapToDto(MethodStep step) => new()
    {
        Id = step.Id,
        RamsDocumentId = step.RamsDocumentId,
        StepNumber = step.StepNumber,
        StepTitle = step.StepTitle,
        DetailedProcedure = step.DetailedProcedure,
        LinkedRiskAssessmentId = step.LinkedRiskAssessmentId,
        LinkedRiskAssessmentTask = step.LinkedRiskAssessment?.TaskActivity,
        RequiredPermits = step.RequiredPermits,
        RequiresSignoff = step.RequiresSignoff,
        SignoffUrl = step.SignoffUrl,
        CreatedAt = step.CreatedAt,
        ModifiedAt = step.UpdatedAt
    };
}
