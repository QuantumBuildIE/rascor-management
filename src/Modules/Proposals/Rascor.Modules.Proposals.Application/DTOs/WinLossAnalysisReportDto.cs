namespace Rascor.Modules.Proposals.Application.DTOs;

public record WinLossAnalysisReportDto
{
    public List<WinLossReasonDto> WinReasons { get; init; } = new();
    public List<WinLossReasonDto> LossReasons { get; init; } = new();
    public decimal AverageTimeToWinDays { get; init; }
    public decimal AverageTimeToLossDays { get; init; }
    public DateTime GeneratedAt { get; init; }
}

public record WinLossReasonDto
{
    public string Reason { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Value { get; init; }
    public decimal Percentage { get; init; }
}
