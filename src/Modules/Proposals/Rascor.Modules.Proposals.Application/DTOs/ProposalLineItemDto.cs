namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalLineItemDto
{
    public Guid Id { get; init; }
    public Guid ProposalSectionId { get; init; }
    public Guid? ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = string.Empty;
    public decimal UnitCost { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
    public decimal LineCost { get; init; }
    public decimal LineMargin { get; init; }
    public decimal MarginPercent { get; init; }
    public int SortOrder { get; init; }
    public string? Notes { get; init; }
}
