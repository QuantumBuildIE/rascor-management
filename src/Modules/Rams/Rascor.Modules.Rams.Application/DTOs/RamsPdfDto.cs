namespace Rascor.Modules.Rams.Application.DTOs;

/// <summary>
/// Data transfer object for RAMS PDF generation
/// </summary>
public record RamsPdfDataDto
{
    // Document Info
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectReference { get; init; } = string.Empty;
    public string ProjectTypeDisplay { get; init; } = string.Empty;
    public string? ClientName { get; init; }
    public string? SiteAddress { get; init; }
    public string? AreaOfActivity { get; init; }
    public string? ProposedStartDate { get; init; }
    public string? ProposedEndDate { get; init; }
    public string? SafetyOfficerName { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;
    public string? DateApproved { get; init; }
    public string? ApprovedByName { get; init; }
    public string? MethodStatementBody { get; init; }

    // Related Data
    public List<RamsPdfRiskAssessmentDto> RiskAssessments { get; init; } = new();
    public List<RamsPdfMethodStepDto> MethodSteps { get; init; } = new();

    // Metadata
    public string GeneratedAt { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? CompanyLogo { get; init; }
}

/// <summary>
/// Risk assessment data for PDF generation
/// </summary>
public record RamsPdfRiskAssessmentDto
{
    public int Number { get; init; }
    public string TaskActivity { get; init; } = string.Empty;
    public string? LocationArea { get; init; }
    public string HazardIdentified { get; init; } = string.Empty;
    public string? WhoAtRisk { get; init; }
    public int InitialLikelihood { get; init; }
    public int InitialSeverity { get; init; }
    public int InitialRiskRating { get; init; }
    public string InitialRiskLevel { get; init; } = string.Empty;
    public string? ControlMeasures { get; init; }
    public string? RelevantLegislation { get; init; }
    public int ResidualLikelihood { get; init; }
    public int ResidualSeverity { get; init; }
    public int ResidualRiskRating { get; init; }
    public string ResidualRiskLevel { get; init; } = string.Empty;
}

/// <summary>
/// Method step data for PDF generation
/// </summary>
public record RamsPdfMethodStepDto
{
    public int StepNumber { get; init; }
    public string StepTitle { get; init; } = string.Empty;
    public string? DetailedProcedure { get; init; }
    public string? LinkedRiskAssessment { get; init; }
    public string? RequiredPermits { get; init; }
    public bool RequiresSignoff { get; init; }
}
