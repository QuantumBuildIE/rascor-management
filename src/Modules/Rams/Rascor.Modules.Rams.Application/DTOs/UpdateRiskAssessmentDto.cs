namespace Rascor.Modules.Rams.Application.DTOs;

public record UpdateRiskAssessmentDto
{
    public string TaskActivity { get; init; } = string.Empty;
    public string? LocationArea { get; init; }
    public string HazardIdentified { get; init; } = string.Empty;
    public string? WhoAtRisk { get; init; }

    public int InitialLikelihood { get; init; }
    public int InitialSeverity { get; init; }

    public string? ControlMeasures { get; init; }
    public string? RelevantLegislation { get; init; }
    public string? ReferenceSops { get; init; }

    public int ResidualLikelihood { get; init; }
    public int ResidualSeverity { get; init; }

    public int? SortOrder { get; init; }
}
