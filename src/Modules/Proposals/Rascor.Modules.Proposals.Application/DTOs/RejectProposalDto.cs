namespace Rascor.Modules.Proposals.Application.DTOs;

public record RejectProposalDto
{
    public string Reason { get; init; } = string.Empty;
}
