using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Application.DTOs;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for managing proposal workflow status transitions and versioning
/// </summary>
public class ProposalWorkflowService : IProposalWorkflowService
{
    private readonly IProposalsDbContext _context;
    private readonly IProposalService _proposalService;
    private readonly IProposalCalculationService _calculationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RejectProposalDto> _rejectValidator;
    private readonly IValidator<LoseProposalDto> _loseValidator;
    private readonly IValidator<CreateRevisionDto> _revisionValidator;

    // Status transition map - defines which transitions are allowed from each status
    private static readonly Dictionary<ProposalStatus, ProposalStatus[]> AllowedTransitions = new()
    {
        { ProposalStatus.Draft, new[] { ProposalStatus.Submitted, ProposalStatus.Cancelled } },
        { ProposalStatus.Submitted, new[] { ProposalStatus.UnderReview, ProposalStatus.Approved, ProposalStatus.Rejected, ProposalStatus.Cancelled } },
        { ProposalStatus.UnderReview, new[] { ProposalStatus.Approved, ProposalStatus.Rejected, ProposalStatus.Cancelled } },
        { ProposalStatus.Approved, new[] { ProposalStatus.Won, ProposalStatus.Lost, ProposalStatus.Cancelled } },
        { ProposalStatus.Rejected, new[] { ProposalStatus.Draft } },  // Can revise
        { ProposalStatus.Won, Array.Empty<ProposalStatus>() },  // Terminal
        { ProposalStatus.Lost, new[] { ProposalStatus.Draft } },  // Can revise
        { ProposalStatus.Expired, new[] { ProposalStatus.Draft } },  // Can revise
        { ProposalStatus.Cancelled, Array.Empty<ProposalStatus>() }  // Terminal
    };

    public ProposalWorkflowService(
        IProposalsDbContext context,
        IProposalService proposalService,
        IProposalCalculationService calculationService,
        ICurrentUserService currentUserService,
        IValidator<RejectProposalDto> rejectValidator,
        IValidator<LoseProposalDto> loseValidator,
        IValidator<CreateRevisionDto> revisionValidator)
    {
        _context = context;
        _proposalService = proposalService;
        _calculationService = calculationService;
        _currentUserService = currentUserService;
        _rejectValidator = rejectValidator;
        _loseValidator = loseValidator;
        _revisionValidator = revisionValidator;
    }

    #region Status Transitions

    public async Task<ProposalDto> SubmitAsync(Guid proposalId, SubmitProposalDto dto)
    {
        var proposal = await GetProposalOrThrowAsync(proposalId);

        ValidateTransition(proposal.Status, ProposalStatus.Submitted);
        await ValidateForSubmissionAsync(proposalId);

        proposal.Status = ProposalStatus.Submitted;
        proposal.SubmittedDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.Notes))
            proposal.Notes = AppendNote(proposal.Notes, $"Submitted: {dto.Notes}");
        else
            proposal.Notes = AppendNote(proposal.Notes, "Submitted for approval");

        await _context.SaveChangesAsync();
        return (await _proposalService.GetProposalByIdAsync(proposalId, true))!;
    }

    public async Task<ProposalDto> ApproveAsync(Guid proposalId, ApproveProposalDto dto)
    {
        var proposal = await GetProposalOrThrowAsync(proposalId);

        ValidateTransition(proposal.Status, ProposalStatus.Approved);

        proposal.Status = ProposalStatus.Approved;
        proposal.ApprovedDate = DateTime.UtcNow;
        proposal.ApprovedBy = _currentUserService.UserName ?? "System";

        if (!string.IsNullOrEmpty(dto.Notes))
            proposal.Notes = AppendNote(proposal.Notes, $"Approved by {proposal.ApprovedBy}: {dto.Notes}");
        else
            proposal.Notes = AppendNote(proposal.Notes, $"Approved by {proposal.ApprovedBy}");

        await _context.SaveChangesAsync();
        return (await _proposalService.GetProposalByIdAsync(proposalId, true))!;
    }

    public async Task<ProposalDto> RejectAsync(Guid proposalId, RejectProposalDto dto)
    {
        await _rejectValidator.ValidateAndThrowAsync(dto);

        var proposal = await GetProposalOrThrowAsync(proposalId);

        ValidateTransition(proposal.Status, ProposalStatus.Rejected);

        proposal.Status = ProposalStatus.Rejected;
        proposal.WonLostReason = dto.Reason;
        proposal.Notes = AppendNote(proposal.Notes, $"Rejected by {_currentUserService.UserName ?? "System"}: {dto.Reason}");

        await _context.SaveChangesAsync();
        return (await _proposalService.GetProposalByIdAsync(proposalId, true))!;
    }

    public async Task<ProposalDto> MarkWonAsync(Guid proposalId, WinProposalDto dto)
    {
        var proposal = await GetProposalOrThrowAsync(proposalId);

        ValidateTransition(proposal.Status, ProposalStatus.Won);

        proposal.Status = ProposalStatus.Won;
        proposal.WonDate = dto.WonDate ?? DateTime.UtcNow;
        proposal.WonLostReason = dto.Reason;

        if (!string.IsNullOrEmpty(dto.Reason))
            proposal.Notes = AppendNote(proposal.Notes, $"Won: {dto.Reason}");
        else
            proposal.Notes = AppendNote(proposal.Notes, "Marked as Won");

        await _context.SaveChangesAsync();
        return (await _proposalService.GetProposalByIdAsync(proposalId, true))!;
    }

    public async Task<ProposalDto> MarkLostAsync(Guid proposalId, LoseProposalDto dto)
    {
        await _loseValidator.ValidateAndThrowAsync(dto);

        var proposal = await GetProposalOrThrowAsync(proposalId);

        ValidateTransition(proposal.Status, ProposalStatus.Lost);

        proposal.Status = ProposalStatus.Lost;
        proposal.LostDate = dto.LostDate ?? DateTime.UtcNow;
        proposal.WonLostReason = dto.Reason;
        proposal.Notes = AppendNote(proposal.Notes, $"Lost: {dto.Reason}");

        await _context.SaveChangesAsync();
        return (await _proposalService.GetProposalByIdAsync(proposalId, true))!;
    }

    public async Task<ProposalDto> CancelAsync(Guid proposalId)
    {
        var proposal = await GetProposalOrThrowAsync(proposalId);

        ValidateTransition(proposal.Status, ProposalStatus.Cancelled);

        proposal.Status = ProposalStatus.Cancelled;
        proposal.Notes = AppendNote(proposal.Notes, $"Cancelled by {_currentUserService.UserName ?? "System"}");

        await _context.SaveChangesAsync();
        return (await _proposalService.GetProposalByIdAsync(proposalId, true))!;
    }

    #endregion

    #region Versioning

    public async Task<ProposalDto> CreateRevisionAsync(Guid proposalId, CreateRevisionDto dto)
    {
        var original = await _context.Proposals
            .Include(p => p.Sections)
            .ThenInclude(s => s.LineItems)
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == proposalId);

        if (original == null)
            throw new InvalidOperationException("Proposal not found");

        // Can only revise from certain statuses (not Draft - drafts should be edited directly)
        // Approved proposals can be revised when client requests changes before accepting/rejecting
        var revisableStatuses = new[]
        {
            ProposalStatus.Rejected,
            ProposalStatus.Lost,
            ProposalStatus.Expired,
            ProposalStatus.Approved
        };

        if (!revisableStatuses.Contains(original.Status))
            throw new InvalidOperationException($"Cannot create revision from status '{original.Status}'");

        // Determine new version number - find the max version in the revision chain
        var rootId = original.ParentProposalId ?? original.Id;
        var maxVersion = await _context.Proposals
            .Where(p => p.Id == rootId || p.ParentProposalId == rootId)
            .MaxAsync(p => p.Version);

        // Generate new proposal number
        var proposalNumber = await _proposalService.GenerateProposalNumberAsync();

        // Create new proposal as copy
        var revision = new Proposal
        {
            // New identity
            Id = Guid.NewGuid(),
            TenantId = original.TenantId,
            ProposalNumber = proposalNumber,
            Version = maxVersion + 1,
            ParentProposalId = rootId,  // Always link to original root

            // Reset status
            Status = ProposalStatus.Draft,
            SubmittedDate = null,
            ApprovedDate = null,
            ApprovedBy = null,
            WonDate = null,
            LostDate = null,
            WonLostReason = null,

            // Copy client details
            CompanyId = original.CompanyId,
            CompanyName = original.CompanyName,
            PrimaryContactId = original.PrimaryContactId,
            PrimaryContactName = original.PrimaryContactName,

            // Copy project details
            ProjectName = original.ProjectName,
            ProjectAddress = original.ProjectAddress,
            ProjectDescription = original.ProjectDescription,

            // Update dates
            ProposalDate = DateTime.UtcNow,
            ValidUntilDate = original.ValidUntilDate.HasValue
                ? DateTime.UtcNow.AddDays((original.ValidUntilDate.Value - original.ProposalDate).Days)
                : null,

            // Copy pricing settings
            Currency = original.Currency,
            VatRate = original.VatRate,
            DiscountPercent = original.DiscountPercent,

            // Copy terms
            PaymentTerms = original.PaymentTerms,
            TermsAndConditions = original.TermsAndConditions,

            // Copy attachments
            DrawingFileName = original.DrawingFileName,
            DrawingUrl = original.DrawingUrl,

            // New notes
            Notes = $"Revision of {original.ProposalNumber} (v{original.Version})" +
                    (string.IsNullOrEmpty(dto.Notes) ? "" : $"\n{dto.Notes}"),

            // Audit
            CreatedBy = _currentUserService.UserName ?? "System"
        };

        _context.Proposals.Add(revision);
        await _context.SaveChangesAsync();

        // Copy sections
        foreach (var originalSection in original.Sections.OrderBy(s => s.SortOrder))
        {
            var newSection = new ProposalSection
            {
                Id = Guid.NewGuid(),
                TenantId = original.TenantId,
                ProposalId = revision.Id,
                SourceKitId = originalSection.SourceKitId,
                SectionName = originalSection.SectionName,
                Description = originalSection.Description,
                SortOrder = originalSection.SortOrder,
                SectionCost = originalSection.SectionCost,
                SectionTotal = originalSection.SectionTotal,
                SectionMargin = originalSection.SectionMargin,
                CreatedBy = _currentUserService.UserName ?? "System"
            };

            _context.ProposalSections.Add(newSection);
            await _context.SaveChangesAsync();

            // Copy line items
            foreach (var originalItem in originalSection.LineItems.OrderBy(i => i.SortOrder))
            {
                var newItem = new ProposalLineItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = original.TenantId,
                    ProposalSectionId = newSection.Id,
                    ProductId = originalItem.ProductId,
                    ProductCode = originalItem.ProductCode,
                    Description = originalItem.Description,
                    Quantity = originalItem.Quantity,
                    Unit = originalItem.Unit,
                    UnitCost = originalItem.UnitCost,
                    UnitPrice = originalItem.UnitPrice,
                    LineTotal = originalItem.LineTotal,
                    LineCost = originalItem.LineCost,
                    LineMargin = originalItem.LineMargin,
                    MarginPercent = originalItem.MarginPercent,
                    SortOrder = originalItem.SortOrder,
                    Notes = originalItem.Notes,
                    CreatedBy = _currentUserService.UserName ?? "System"
                };

                _context.ProposalLineItems.Add(newItem);
            }
        }

        // Copy contacts
        foreach (var originalContact in original.Contacts)
        {
            var newContact = new ProposalContact
            {
                Id = Guid.NewGuid(),
                TenantId = original.TenantId,
                ProposalId = revision.Id,
                ContactId = originalContact.ContactId,
                ContactName = originalContact.ContactName,
                Email = originalContact.Email,
                Phone = originalContact.Phone,
                Role = originalContact.Role,
                IsPrimary = originalContact.IsPrimary,
                CreatedBy = _currentUserService.UserName ?? "System"
            };

            _context.ProposalContacts.Add(newContact);
        }

        await _context.SaveChangesAsync();

        // Recalculate all totals
        await _calculationService.RecalculateAllAsync(revision.Id);

        return (await _proposalService.GetProposalByIdAsync(revision.Id, true))!;
    }

    public async Task<List<ProposalListDto>> GetRevisionsAsync(Guid proposalId)
    {
        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        var rootId = proposal.ParentProposalId ?? proposal.Id;

        var revisions = await _context.Proposals
            .Where(p => p.Id == rootId || p.ParentProposalId == rootId)
            .OrderBy(p => p.Version)
            .Select(p => new ProposalListDto
            {
                Id = p.Id,
                ProposalNumber = p.ProposalNumber,
                Version = p.Version,
                ProjectName = p.ProjectName,
                CompanyName = p.CompanyName,
                ProposalDate = p.ProposalDate,
                ValidUntilDate = p.ValidUntilDate,
                Status = p.Status.ToString(),
                GrandTotal = p.GrandTotal,
                Currency = p.Currency,
                MarginPercent = null,  // Don't expose margin in list view
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return revisions;
    }

    #endregion

    #region Expiry

    public async Task<int> ExpireOverdueProposalsAsync()
    {
        var today = DateTime.UtcNow.Date;

        // Find proposals that are past their validity date and still in active status
        var expirableStatuses = new[]
        {
            ProposalStatus.Draft,
            ProposalStatus.Submitted,
            ProposalStatus.UnderReview,
            ProposalStatus.Approved
        };

        var overdueProposals = await _context.Proposals
            .Where(p => p.ValidUntilDate.HasValue
                        && p.ValidUntilDate.Value.Date < today
                        && expirableStatuses.Contains(p.Status))
            .ToListAsync();

        foreach (var proposal in overdueProposals)
        {
            proposal.Status = ProposalStatus.Expired;
            proposal.Notes = AppendNote(proposal.Notes, "Automatically expired - past validity date");
        }

        await _context.SaveChangesAsync();

        return overdueProposals.Count;
    }

    #endregion

    #region Validation

    public bool CanTransitionTo(ProposalStatus currentStatus, ProposalStatus newStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var allowed)
               && allowed.Contains(newStatus);
    }

    public async Task ValidateForSubmissionAsync(Guid proposalId)
    {
        var proposal = await _context.Proposals
            .Include(p => p.Sections)
            .ThenInclude(s => s.LineItems)
            .FirstOrDefaultAsync(p => p.Id == proposalId);

        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        var errors = new List<string>();

        if (!proposal.Sections.Any())
            errors.Add("Proposal must have at least one section");

        foreach (var section in proposal.Sections)
        {
            if (!section.LineItems.Any())
                errors.Add($"Section '{section.SectionName}' must have at least one line item");
        }

        if (proposal.GrandTotal <= 0)
            errors.Add("Proposal total must be greater than zero");

        if (errors.Any())
            throw new InvalidOperationException(string.Join("; ", errors));
    }

    #endregion

    #region Private Helpers

    private async Task<Proposal> GetProposalOrThrowAsync(Guid proposalId)
    {
        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");
        return proposal;
    }

    private void ValidateTransition(ProposalStatus currentStatus, ProposalStatus newStatus)
    {
        if (!CanTransitionTo(currentStatus, newStatus))
            throw new InvalidOperationException($"Cannot transition from '{currentStatus}' to '{newStatus}'");
    }

    /// <summary>
    /// Helper to append timestamped notes
    /// </summary>
    private static string AppendNote(string? existingNotes, string newNote)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var entry = $"[{timestamp}] {newNote}";
        return string.IsNullOrEmpty(existingNotes) ? entry : $"{existingNotes}\n{entry}";
    }

    #endregion
}
