using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record CreateHazardLibraryDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public HazardCategory Category { get; init; }
    public string? Keywords { get; init; }
    public int DefaultLikelihood { get; init; } = 3;
    public int DefaultSeverity { get; init; } = 3;
    public string? TypicalWhoAtRisk { get; init; }
    public int SortOrder { get; init; }
}
