namespace Rascor.Modules.Proposals.Application.DTOs;

public record UpdateProposalSectionDto
{
    public string SectionName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}
