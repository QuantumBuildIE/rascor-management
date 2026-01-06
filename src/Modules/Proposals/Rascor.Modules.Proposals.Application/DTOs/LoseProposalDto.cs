namespace Rascor.Modules.Proposals.Application.DTOs;

public record LoseProposalDto
{
    public string Reason { get; init; } = string.Empty;
    public DateTime? LostDate { get; init; }  // Defaults to now if not provided
}
