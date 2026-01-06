namespace Rascor.Modules.Proposals.Application.DTOs;

public record UpdateProposalLineItemDto
{
    public Guid? ProductId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = "Each";
    public decimal UnitCost { get; init; }
    public decimal UnitPrice { get; init; }
    public int SortOrder { get; init; }
    public string? Notes { get; init; }
}
