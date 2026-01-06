using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Proposals.Domain.Entities;

public class ProposalLineItem : TenantEntity
{
    public Guid ProposalSectionId { get; set; }
    public Guid? ProductId { get; set; }  // FK to Product (optional - can be ad-hoc items)
    public string? ProductCode { get; set; }  // Cached from Product

    public string Description { get; set; } = string.Empty;  // Can override product name
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Each";  // e.g., Each, m², hours

    // Pricing
    public decimal UnitCost { get; set; }  // Cost price (from Product or manual)
    public decimal UnitPrice { get; set; }  // Sell price for this quote
    public decimal LineTotal { get; set; }  // Quantity × UnitPrice
    public decimal LineCost { get; set; }  // Quantity × UnitCost
    public decimal LineMargin { get; set; }  // LineTotal - LineCost
    public decimal MarginPercent { get; set; }  // LineMargin / LineTotal * 100

    public int SortOrder { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public ProposalSection Section { get; set; } = null!;
}
