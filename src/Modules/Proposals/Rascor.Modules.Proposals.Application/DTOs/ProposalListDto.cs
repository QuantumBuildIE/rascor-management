namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalListDto
{
    public Guid Id { get; init; }
    public string ProposalNumber { get; init; } = string.Empty;
    public int Version { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public DateTime ProposalDate { get; init; }
    public DateTime? ValidUntilDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal GrandTotal { get; init; }
    public string Currency { get; init; } = "EUR";
    public decimal? MarginPercent { get; init; }  // Only if ViewCostings
    public DateTime CreatedAt { get; init; }
}
