using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.DTOs;

/// <summary>
/// Full DTO for a toolbox talk with all details including sections and questions
/// </summary>
public record ToolboxTalkDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ToolboxTalkFrequency Frequency { get; init; }
    public string FrequencyDisplay { get; init; } = string.Empty;
    public string? VideoUrl { get; init; }
    public VideoSource VideoSource { get; init; }
    public string VideoSourceDisplay { get; init; } = string.Empty;
    public string? AttachmentUrl { get; init; }
    public int MinimumVideoWatchPercent { get; init; }
    public bool RequiresQuiz { get; init; }
    public int? PassingScore { get; init; }
    public bool IsActive { get; init; }
    public ToolboxTalkStatus Status { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;
    public string? PdfUrl { get; init; }
    public string? PdfFileName { get; init; }
    public bool GeneratedFromVideo { get; init; }
    public bool GeneratedFromPdf { get; init; }

    // Child collections
    public List<ToolboxTalkSectionDto> Sections { get; init; } = new();
    public List<ToolboxTalkQuestionDto> Questions { get; init; } = new();

    // Completion stats (for list context)
    public ToolboxTalkCompletionStatsDto? CompletionStats { get; init; }

    // Audit
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Completion statistics for a toolbox talk
/// </summary>
public record ToolboxTalkCompletionStatsDto
{
    public int TotalAssignments { get; init; }
    public int CompletedCount { get; init; }
    public int OverdueCount { get; init; }
    public int PendingCount { get; init; }
    public int InProgressCount { get; init; }
    public decimal CompletionRate { get; init; }
}
