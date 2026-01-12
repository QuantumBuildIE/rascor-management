using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record UpdateControlMeasureLibraryDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ControlHierarchy Hierarchy { get; init; }
    public HazardCategory? ApplicableToCategory { get; init; }
    public string? Keywords { get; init; }
    public int TypicalLikelihoodReduction { get; init; }
    public int TypicalSeverityReduction { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}
