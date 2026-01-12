using Rascor.Core.Domain.Common;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Domain.Entities;

public class RiskAssessment : TenantEntity
{
    public Guid RamsDocumentId { get; set; }
    public RamsDocument RamsDocument { get; set; } = null!;

    // Task/Activity being assessed
    public string TaskActivity { get; set; } = string.Empty;
    public string? LocationArea { get; set; }

    // Hazard identification
    public string HazardIdentified { get; set; } = string.Empty;
    public string? WhoAtRisk { get; set; }  // "Employees", "Public", "Contractors", etc.

    // Initial risk rating (before controls)
    public int InitialLikelihood { get; set; }  // 1-5
    public int InitialSeverity { get; set; }    // 1-5
    public int InitialRiskRating => InitialLikelihood * InitialSeverity;
    public RiskLevel InitialRiskLevel => CalculateRiskLevel(InitialRiskRating);

    // Control measures
    public string? ControlMeasures { get; set; }
    public string? RelevantLegislation { get; set; }
    public string? ReferenceSops { get; set; }  // SOP IDs or links

    // Residual risk rating (after controls)
    public int ResidualLikelihood { get; set; }  // 1-5
    public int ResidualSeverity { get; set; }    // 1-5
    public int ResidualRiskRating => ResidualLikelihood * ResidualSeverity;
    public RiskLevel ResidualRiskLevel => CalculateRiskLevel(ResidualRiskRating);

    // AI-generated flag
    public bool IsAiGenerated { get; set; }
    public DateTime? AiGeneratedAt { get; set; }

    // Display order
    public int SortOrder { get; set; }

    private static RiskLevel CalculateRiskLevel(int score) => score switch
    {
        <= 4 => RiskLevel.Low,
        <= 12 => RiskLevel.Medium,
        _ => RiskLevel.High
    };
}
