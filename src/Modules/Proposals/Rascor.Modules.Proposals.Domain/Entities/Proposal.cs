using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Proposals.Domain.Entities;

public class Proposal : TenantEntity
{
    public string ProposalNumber { get; set; } = string.Empty;  // Auto: PROP-2024-0001
    public int Version { get; set; } = 1;
    public Guid? ParentProposalId { get; set; }  // For revisions - links to original

    // Client
    public Guid CompanyId { get; set; }  // FK to Core.Company
    public string CompanyName { get; set; } = string.Empty;  // Cached
    public Guid? PrimaryContactId { get; set; }  // FK to Core.Contact
    public string? PrimaryContactName { get; set; }  // Cached

    // Project Details
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectAddress { get; set; }
    public string? ProjectDescription { get; set; }

    // Dates
    public DateTime ProposalDate { get; set; }
    public DateTime? ValidUntilDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? WonDate { get; set; }
    public DateTime? LostDate { get; set; }

    // Status
    public ProposalStatus Status { get; set; } = ProposalStatus.Draft;
    public string? WonLostReason { get; set; }

    // Pricing
    public string Currency { get; set; } = "EUR";
    public decimal Subtotal { get; set; }  // Sum of section totals
    public decimal DiscountPercent { get; set; }  // e.g., 5 for 5%
    public decimal DiscountAmount { get; set; }  // Calculated or manual
    public decimal NetTotal { get; set; }  // Subtotal - Discount
    public decimal VatRate { get; set; } = 23m;  // Irish VAT default
    public decimal VatAmount { get; set; }  // Calculated
    public decimal GrandTotal { get; set; }  // Net + VAT

    // Margin (internal, not shown to client)
    public decimal TotalCost { get; set; }  // Sum of line costs
    public decimal TotalMargin { get; set; }  // NetTotal - TotalCost
    public decimal MarginPercent { get; set; }  // Margin / NetTotal * 100

    // Terms
    public string? PaymentTerms { get; set; }  // e.g., "30 days from invoice"
    public string? TermsAndConditions { get; set; }

    // Notes & Attachments
    public string? Notes { get; set; }
    public string? DrawingFileName { get; set; }
    public string? DrawingUrl { get; set; }

    // Navigation
    public Proposal? ParentProposal { get; set; }
    public ICollection<Proposal> Revisions { get; set; } = new List<Proposal>();
    public ICollection<ProposalSection> Sections { get; set; } = new List<ProposalSection>();
    public ICollection<ProposalContact> Contacts { get; set; } = new List<ProposalContact>();
}
