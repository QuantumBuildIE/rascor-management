namespace Rascor.Modules.Proposals.Application.DTOs;

/// <summary>
/// Summary statistics for the proposals dashboard
/// </summary>
public record ProposalSummaryDto
{
    /// <summary>
    /// Total count of all proposals
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total value of active proposals in the pipeline (Draft, Submitted, UnderReview, Approved)
    /// </summary>
    public decimal PipelineValue { get; init; }

    /// <summary>
    /// Count of proposals won this month
    /// </summary>
    public int WonThisMonthCount { get; init; }

    /// <summary>
    /// Total value of proposals won this month
    /// </summary>
    public decimal WonThisMonthValue { get; init; }

    /// <summary>
    /// Overall conversion rate (Won / (Won + Lost)) as a percentage
    /// </summary>
    public decimal ConversionRate { get; init; }
}
