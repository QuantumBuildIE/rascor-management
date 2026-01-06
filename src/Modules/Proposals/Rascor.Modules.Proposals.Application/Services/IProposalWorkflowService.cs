using Rascor.Modules.Proposals.Application.DTOs;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for managing proposal workflow status transitions and versioning
/// </summary>
public interface IProposalWorkflowService
{
    // Status transitions
    Task<ProposalDto> SubmitAsync(Guid proposalId, SubmitProposalDto dto);
    Task<ProposalDto> ApproveAsync(Guid proposalId, ApproveProposalDto dto);
    Task<ProposalDto> RejectAsync(Guid proposalId, RejectProposalDto dto);
    Task<ProposalDto> MarkWonAsync(Guid proposalId, WinProposalDto dto);
    Task<ProposalDto> MarkLostAsync(Guid proposalId, LoseProposalDto dto);
    Task<ProposalDto> CancelAsync(Guid proposalId);

    // Versioning
    Task<ProposalDto> CreateRevisionAsync(Guid proposalId, CreateRevisionDto dto);
    Task<List<ProposalListDto>> GetRevisionsAsync(Guid proposalId);

    // Expiry
    Task<int> ExpireOverdueProposalsAsync();  // Called by background job

    // Validation
    bool CanTransitionTo(ProposalStatus currentStatus, ProposalStatus newStatus);
    Task ValidateForSubmissionAsync(Guid proposalId);
}
