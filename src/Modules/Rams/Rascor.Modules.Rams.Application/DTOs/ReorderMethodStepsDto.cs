namespace Rascor.Modules.Rams.Application.DTOs;

public record ReorderMethodStepsDto
{
    public List<Guid> OrderedIds { get; init; } = new();
}
