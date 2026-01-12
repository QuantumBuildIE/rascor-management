using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record UpdateHazardLibraryDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public HazardCategory Category { get; init; }
    public string? Keywords { get; init; }
    public int DefaultLikelihood { get; init; }
    public int DefaultSeverity { get; init; }
    public string? TypicalWhoAtRisk { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}
