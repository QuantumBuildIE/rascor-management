namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalsByCompanyReportDto
{
    public List<CompanyProposalSummaryDto> Companies { get; init; } = new();
    public int TotalCompanies { get; init; }
    public DateTime GeneratedAt { get; init; }
}

public record CompanyProposalSummaryDto
{
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public int TotalProposals { get; init; }
    public int WonCount { get; init; }
    public int LostCount { get; init; }
    public decimal TotalValue { get; init; }
    public decimal WonValue { get; init; }
    public decimal ConversionRate { get; init; }
}
