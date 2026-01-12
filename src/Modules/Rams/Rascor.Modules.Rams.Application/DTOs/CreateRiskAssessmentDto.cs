namespace Rascor.Modules.Rams.Application.DTOs;

public record CreateRiskAssessmentDto
{
    public string TaskActivity { get; init; } = string.Empty;
    public string? LocationArea { get; init; }
    public string HazardIdentified { get; init; } = string.Empty;
    public string? WhoAtRisk { get; init; }

    // Initial risk (1-5 scale)
    public int InitialLikelihood { get; init; }
    public int InitialSeverity { get; init; }

    // Controls (can be populated manually or by AI)
    public string? ControlMeasures { get; init; }
    public string? RelevantLegislation { get; init; }
    public string? ReferenceSops { get; init; }

    // Residual risk (1-5 scale)
    public int ResidualLikelihood { get; init; }
    public int ResidualSeverity { get; init; }

    public int? SortOrder { get; init; }
}
