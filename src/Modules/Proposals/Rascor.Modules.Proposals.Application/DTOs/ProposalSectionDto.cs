namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalSectionDto
{
    public Guid Id { get; init; }
    public Guid ProposalId { get; init; }
    public Guid? SourceKitId { get; init; }
    public string? SourceKitName { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public decimal SectionCost { get; init; }
    public decimal SectionTotal { get; init; }
    public decimal SectionMargin { get; init; }
    public List<ProposalLineItemDto> LineItems { get; init; } = new();
}
