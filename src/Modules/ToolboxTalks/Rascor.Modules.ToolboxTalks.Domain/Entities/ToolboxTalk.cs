using Rascor.Core.Domain.Common;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Domain.Entities;

/// <summary>
/// Represents a toolbox talk - a safety briefing or training session
/// that employees must complete, optionally with video content and quiz assessment
/// </summary>
public class ToolboxTalk : TenantEntity
{
    /// <summary>
    /// Title of the toolbox talk
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the toolbox talk content and purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// How often employees must complete this toolbox talk
    /// </summary>
    public ToolboxTalkFrequency Frequency { get; set; } = ToolboxTalkFrequency.Once;

    /// <summary>
    /// URL to the video content (if any)
    /// </summary>
    public string? VideoUrl { get; set; }

    /// <summary>
    /// Source platform for the video
    /// </summary>
    public VideoSource VideoSource { get; set; } = VideoSource.None;

    /// <summary>
    /// URL to any attachment (PDF, document, etc.)
    /// </summary>
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// Minimum percentage of video that must be watched to mark as complete (0-100)
    /// Default is 90%
    /// </summary>
    public int MinimumVideoWatchPercent { get; set; } = 90;

    /// <summary>
    /// Whether a quiz must be passed to complete this toolbox talk
    /// </summary>
    public bool RequiresQuiz { get; set; } = false;

    /// <summary>
    /// Minimum score (percentage) required to pass the quiz
    /// Only applicable if RequiresQuiz is true
    /// </summary>
    public int? PassingScore { get; set; } = 80;

    /// <summary>
    /// Whether this toolbox talk is currently active and available to employees
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// Content sections within this toolbox talk
    /// </summary>
    public ICollection<ToolboxTalkSection> Sections { get; set; } = new List<ToolboxTalkSection>();

    /// <summary>
    /// Quiz questions for this toolbox talk
    /// </summary>
    public ICollection<ToolboxTalkQuestion> Questions { get; set; } = new List<ToolboxTalkQuestion>();

    /// <summary>
    /// Translations of this toolbox talk in different languages
    /// </summary>
    public ICollection<ToolboxTalkTranslation> Translations { get; set; } = new List<ToolboxTalkTranslation>();

    /// <summary>
    /// Video translations for this toolbox talk
    /// </summary>
    public ICollection<ToolboxTalkVideoTranslation> VideoTranslations { get; set; } = new List<ToolboxTalkVideoTranslation>();
}
