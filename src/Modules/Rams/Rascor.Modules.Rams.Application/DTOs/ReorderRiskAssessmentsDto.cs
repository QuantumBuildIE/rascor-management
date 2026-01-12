namespace Rascor.Modules.Rams.Application.DTOs;

public record ReorderRiskAssessmentsDto
{
    public List<Guid> OrderedIds { get; init; } = new();
}
