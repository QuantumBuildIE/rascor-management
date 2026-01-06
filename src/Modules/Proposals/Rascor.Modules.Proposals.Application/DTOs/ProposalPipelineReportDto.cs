namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalPipelineReportDto
{
    public decimal TotalPipelineValue { get; init; }
    public int TotalProposals { get; init; }
    public List<PipelineStageDto> Stages { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
}

public record PipelineStageDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Value { get; init; }
    public decimal Percentage { get; init; }  // % of total pipeline
}
