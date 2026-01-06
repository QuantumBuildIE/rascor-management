namespace Rascor.Modules.Proposals.Application.DTOs;

public record WinProposalDto
{
    public string? Reason { get; init; }
    public DateTime? WonDate { get; init; }  // Defaults to now if not provided
}
