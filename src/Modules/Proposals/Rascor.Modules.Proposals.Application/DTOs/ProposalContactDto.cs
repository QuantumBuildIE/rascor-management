namespace Rascor.Modules.Proposals.Application.DTOs;

public record ProposalContactDto
{
    public Guid Id { get; init; }
    public Guid ProposalId { get; init; }
    public Guid? ContactId { get; init; }
    public string ContactName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string Role { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}
