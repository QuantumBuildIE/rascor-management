using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Proposals.Domain.Entities;

public class ProposalContact : TenantEntity
{
    public Guid ProposalId { get; set; }
    public Guid? ContactId { get; set; }  // FK to Core.Contact (optional)

    // Can be linked to existing contact OR ad-hoc entry
    public string ContactName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;  // Quantity Surveyor, Site Manager, etc.
    public bool IsPrimary { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = null!;
}
