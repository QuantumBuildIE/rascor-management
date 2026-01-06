using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.DTOs;

/// <summary>
/// Lightweight DTO for toolbox talk lists
/// </summary>
public record ToolboxTalkListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ToolboxTalkFrequency Frequency { get; init; }
    public string FrequencyDisplay { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool HasVideo { get; init; }
    public bool RequiresQuiz { get; init; }

    // Counts
    public int SectionCount { get; init; }
    public int QuestionCount { get; init; }

    // Completion stats
    public ToolboxTalkCompletionStatsDto? CompletionStats { get; init; }

    // Audit
    public DateTime CreatedAt { get; init; }
}
