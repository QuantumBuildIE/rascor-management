namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalsByStatusReportDto
{
    public List<StatusBreakdownDto> Statuses { get; init; } = new();
    public int TotalCount { get; init; }
    public decimal TotalValue { get; init; }
    public DateTime GeneratedAt { get; init; }
}

public record StatusBreakdownDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Value { get; init; }
    public decimal AverageValue { get; init; }
    public decimal PercentageOfTotal { get; init; }
}
