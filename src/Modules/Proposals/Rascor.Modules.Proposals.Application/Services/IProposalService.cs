using Rascor.Core.Application.Models;
using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Services;

public interface IProposalService
{
    // Proposals
    Task<PaginatedList<ProposalListDto>> GetProposalsAsync(
        string? search, string? status, Guid? companyId,
        int pageNumber, int pageSize, string? sortColumn, string? sortDirection);
    Task<ProposalSummaryDto> GetSummaryAsync();
    Task<ProposalDto?> GetProposalByIdAsync(Guid id, bool includeCosting = false);
    Task<ProposalDto> CreateProposalAsync(CreateProposalDto dto);
    Task<ProposalDto> UpdateProposalAsync(Guid id, UpdateProposalDto dto);
    Task DeleteProposalAsync(Guid id);
    Task<string> GenerateProposalNumberAsync();

    // Sections
    Task<ProposalSectionDto> AddSectionAsync(CreateProposalSectionDto dto);
    Task<ProposalSectionDto> UpdateSectionAsync(Guid id, UpdateProposalSectionDto dto);
    Task DeleteSectionAsync(Guid id);

    // Line Items
    Task<ProposalLineItemDto> AddLineItemAsync(CreateProposalLineItemDto dto);
    Task<ProposalLineItemDto> UpdateLineItemAsync(Guid id, UpdateProposalLineItemDto dto);
    Task DeleteLineItemAsync(Guid id);

    // Contacts
    Task<ProposalContactDto> AddContactAsync(CreateProposalContactDto dto);
    Task<ProposalContactDto> UpdateContactAsync(Guid id, UpdateProposalContactDto dto);
    Task DeleteContactAsync(Guid id);
}
