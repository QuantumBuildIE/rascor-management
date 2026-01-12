using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record RiskAssessmentDto
{
    public Guid Id { get; init; }
    public Guid RamsDocumentId { get; init; }
    public string TaskActivity { get; init; } = string.Empty;
    public string? LocationArea { get; init; }
    public string HazardIdentified { get; init; } = string.Empty;
    public string? WhoAtRisk { get; init; }

    // Initial risk
    public int InitialLikelihood { get; init; }
    public int InitialSeverity { get; init; }
    public int InitialRiskRating { get; init; }
    public RiskLevel InitialRiskLevel { get; init; }
    public string InitialRiskLevelDisplay => InitialRiskLevel.ToString();

    // Controls
    public string? ControlMeasures { get; init; }
    public string? RelevantLegislation { get; init; }
    public string? ReferenceSops { get; init; }

    // Residual risk
    public int ResidualLikelihood { get; init; }
    public int ResidualSeverity { get; init; }
    public int ResidualRiskRating { get; init; }
    public RiskLevel ResidualRiskLevel { get; init; }
    public string ResidualRiskLevelDisplay => ResidualRiskLevel.ToString();

    // AI
    public bool IsAiGenerated { get; init; }
    public DateTime? AiGeneratedAt { get; init; }

    public int SortOrder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}
