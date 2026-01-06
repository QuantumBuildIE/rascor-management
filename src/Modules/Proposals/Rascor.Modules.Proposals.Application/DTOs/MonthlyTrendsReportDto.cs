namespace Rascor.Modules.Proposals.Application.DTOs;

public record MonthlyTrendsReportDto
{
    public List<MonthlyDataPointDto> DataPoints { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
}

public record MonthlyDataPointDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public int ProposalsCreated { get; init; }
    public int ProposalsWon { get; init; }
    public int ProposalsLost { get; init; }
    public decimal ValueCreated { get; init; }
    public decimal ValueWon { get; init; }
    public decimal ValueLost { get; init; }
    public decimal ConversionRate { get; init; }
}
