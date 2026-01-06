namespace Rascor.Modules.Proposals.Application.DTOs;

public record CreateProposalSectionDto
{
    public Guid ProposalId { get; init; }
    public Guid? SourceKitId { get; init; }  // If provided, auto-populate line items from kit
    public string SectionName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}
