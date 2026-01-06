namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalConversionReportDto
{
    public int TotalProposals { get; init; }
    public int WonCount { get; init; }
    public int LostCount { get; init; }
    public int PendingCount { get; init; }
    public int CancelledCount { get; init; }
    public decimal WonValue { get; init; }
    public decimal LostValue { get; init; }
    public decimal ConversionRate { get; init; }  // Won / (Won + Lost) * 100
    public decimal WinRate { get; init; }  // Won / Total * 100
    public decimal AverageProposalValue { get; init; }
    public decimal AverageWonValue { get; init; }
    public DateTime GeneratedAt { get; init; }
}
