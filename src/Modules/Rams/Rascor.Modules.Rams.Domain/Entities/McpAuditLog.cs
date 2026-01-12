using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Audit log for AI/MCP requests and responses in the RAMS module.
/// Tracks usage, acceptance rates, and provides data for improving suggestions.
/// </summary>
public class McpAuditLog : TenantEntity
{
    public Guid? RamsDocumentId { get; set; }
    public Guid? RiskAssessmentId { get; set; }

    /// <summary>
    /// Type of AI request (e.g., "ControlMeasureSuggestion", "HazardLookup")
    /// </summary>
    public string RequestType { get; set; } = string.Empty;

    /// <summary>
    /// The prompt/input sent to AI
    /// </summary>
    public string InputPrompt { get; set; } = string.Empty;

    /// <summary>
    /// JSON of context data sent with the request
    /// </summary>
    public string? InputContext { get; set; }

    /// <summary>
    /// Full AI response (JSON)
    /// </summary>
    public string? AiResponse { get; set; }

    /// <summary>
    /// Parsed/extracted useful content from the AI response
    /// </summary>
    public string? ExtractedContent { get; set; }

    /// <summary>
    /// AI model used (e.g., "claude-3-sonnet")
    /// </summary>
    public string? ModelUsed { get; set; }

    /// <summary>
    /// Number of input tokens consumed
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// Number of output tokens generated
    /// </summary>
    public int? OutputTokens { get; set; }

    /// <summary>
    /// Estimated cost in USD
    /// </summary>
    public decimal? CostEstimate { get; set; }

    /// <summary>
    /// Whether the user accepted the AI suggestion
    /// </summary>
    public bool WasAccepted { get; set; }

    /// <summary>
    /// When the request was made
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// How long the request took in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Error message if the request failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    // Navigation
    public virtual RamsDocument? RamsDocument { get; set; }
    public virtual RiskAssessment? RiskAssessment { get; set; }
}
