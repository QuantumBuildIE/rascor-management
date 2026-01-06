namespace Rascor.Modules.Proposals.Application.DTOs;

public record CreateProposalDto
{
    public Guid CompanyId { get; init; }
    public Guid? PrimaryContactId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string? ProjectAddress { get; init; }
    public string? ProjectDescription { get; init; }
    public DateTime ProposalDate { get; init; }
    public DateTime? ValidUntilDate { get; init; }
    public string Currency { get; init; } = "EUR";
    public decimal VatRate { get; init; } = 23m;
    public decimal DiscountPercent { get; init; }
    public string? PaymentTerms { get; init; }
    public string? TermsAndConditions { get; init; }
    public string? Notes { get; init; }
}
