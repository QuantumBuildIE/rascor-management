namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalDto
{
    public Guid Id { get; init; }
    public string ProposalNumber { get; init; } = string.Empty;
    public int Version { get; init; }
    public Guid? ParentProposalId { get; init; }

    // Client
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public Guid? PrimaryContactId { get; init; }
    public string? PrimaryContactName { get; init; }

    // Project
    public string ProjectName { get; init; } = string.Empty;
    public string? ProjectAddress { get; init; }
    public string? ProjectDescription { get; init; }

    // Dates
    public DateTime ProposalDate { get; init; }
    public DateTime? ValidUntilDate { get; init; }
    public DateTime? SubmittedDate { get; init; }
    public DateTime? ApprovedDate { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? WonDate { get; init; }
    public DateTime? LostDate { get; init; }

    // Status
    public string Status { get; init; } = string.Empty;
    public string? WonLostReason { get; init; }

    // Pricing
    public string Currency { get; init; } = "EUR";
    public decimal Subtotal { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal NetTotal { get; init; }
    public decimal VatRate { get; init; }
    public decimal VatAmount { get; init; }
    public decimal GrandTotal { get; init; }

    // Margin (only if user has ViewCostings permission)
    public decimal? TotalCost { get; init; }
    public decimal? TotalMargin { get; init; }
    public decimal? MarginPercent { get; init; }

    // Terms
    public string? PaymentTerms { get; init; }
    public string? TermsAndConditions { get; init; }

    // Notes & Attachments
    public string? Notes { get; init; }
    public string? DrawingFileName { get; init; }
    public string? DrawingUrl { get; init; }

    // Child collections
    public List<ProposalSectionDto> Sections { get; init; } = new();
    public List<ProposalContactDto> Contacts { get; init; } = new();

    // Audit
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime? UpdatedAt { get; init; }
}
