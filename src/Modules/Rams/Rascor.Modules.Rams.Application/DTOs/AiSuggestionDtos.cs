namespace Rascor.Modules.Rams.Application.DTOs;

/// <summary>
/// Request for AI-powered control measure suggestions
/// </summary>
public record ControlMeasureSuggestionRequestDto
{
    public string TaskActivity { get; init; } = string.Empty;
    public string HazardIdentified { get; init; } = string.Empty;
    public string? LocationArea { get; init; }
    public string? WhoAtRisk { get; init; }
    public int? InitialLikelihood { get; init; }
    public int? InitialSeverity { get; init; }
    public string? ProjectType { get; init; }
    public string? AdditionalContext { get; init; }
}

/// <summary>
/// Response containing AI-generated control measure suggestions
/// </summary>
public record ControlMeasureSuggestionResponseDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    // Library matches
    public List<HazardMatchDto> MatchedHazards { get; init; } = new();
    public List<ControlMatchDto> SuggestedControls { get; init; } = new();
    public List<LegislationMatchDto> RelevantLegislation { get; init; } = new();
    public List<SopMatchDto> RelevantSops { get; init; } = new();

    // AI-generated content
    public string? AiGeneratedControlMeasures { get; init; }
    public string? AiGeneratedLegislation { get; init; }
    public int? SuggestedResidualLikelihood { get; init; }
    public int? SuggestedResidualSeverity { get; init; }

    // Metadata
    public Guid? AuditLogId { get; init; }
    public bool UsedAi { get; init; }
}

/// <summary>
/// A matched hazard from the library
/// </summary>
public record HazardMatchDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public int DefaultLikelihood { get; init; }
    public int DefaultSeverity { get; init; }
    public double MatchScore { get; init; }
}

/// <summary>
/// A matched control measure from the library
/// </summary>
public record ControlMatchDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Hierarchy { get; init; } = string.Empty;
    public int LikelihoodReduction { get; init; }
    public int SeverityReduction { get; init; }
    public double MatchScore { get; init; }
}

/// <summary>
/// A matched legislation reference from the library
/// </summary>
public record LegislationMatchDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ShortName { get; init; }
    public double MatchScore { get; init; }
}

/// <summary>
/// A matched SOP reference from the library
/// </summary>
public record SopMatchDto
{
    public Guid Id { get; init; }
    public string SopId { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string? PolicySnippet { get; init; }
    public double MatchScore { get; init; }
}

/// <summary>
/// DTO for marking a suggestion as accepted/rejected
/// </summary>
public record AcceptSuggestionDto
{
    public Guid AuditLogId { get; init; }
    public bool Accepted { get; init; }
}
