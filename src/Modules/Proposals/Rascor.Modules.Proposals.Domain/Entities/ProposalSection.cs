using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Proposals.Domain.Entities;

public class ProposalSection : TenantEntity
{
    public Guid ProposalId { get; set; }
    public Guid? SourceKitId { get; set; }  // FK to ProductKit if created from kit

    public string SectionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    // Calculated totals
    public decimal SectionCost { get; set; }  // Sum of line costs
    public decimal SectionTotal { get; set; }  // Sum of line totals
    public decimal SectionMargin { get; set; }  // Total - Cost

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public ICollection<ProposalLineItem> LineItems { get; set; } = new List<ProposalLineItem>();
}
