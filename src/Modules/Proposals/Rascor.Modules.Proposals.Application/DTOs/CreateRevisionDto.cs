namespace Rascor.Modules.Proposals.Application.DTOs;

public record CreateRevisionDto
{
    public string? Notes { get; init; }  // Notes for why revision was created
}
