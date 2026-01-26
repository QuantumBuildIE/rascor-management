using Rascor.Core.Domain.Common;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Domain.Entities;

/// <summary>
/// Represents a quiz question for a toolbox talk assessment
/// Questions can be multiple choice, true/false, or short answer
/// </summary>
public class ToolboxTalkQuestion : BaseEntity
{
    /// <summary>
    /// Foreign key to the parent toolbox talk
    /// </summary>
    public Guid ToolboxTalkId { get; set; }

    /// <summary>
    /// Order number for displaying questions in sequence
    /// </summary>
    public int QuestionNumber { get; set; }

    /// <summary>
    /// The question text
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Type of question (determines how options and answers are handled)
    /// </summary>
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

    /// <summary>
    /// JSON array of options for multiple choice questions
    /// Format: ["Option A", "Option B", "Option C", "Option D"]
    /// Null for true/false and short answer questions
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// The correct answer
    /// For multiple choice: the exact text of the correct option
    /// For true/false: "True" or "False"
    /// For short answer: expected answer text (may support partial matching)
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>
    /// Points awarded for correctly answering this question
    /// </summary>
    public int Points { get; set; } = 1;

    /// <summary>
    /// Source of this question content (Manual, Video, Pdf)
    /// </summary>
    public ContentSource Source { get; set; } = ContentSource.Manual;

    /// <summary>
    /// Whether this question is generated from the final portion of the video
    /// Questions from the final portion ensure users watched until the end
    /// </summary>
    public bool IsFromVideoFinalPortion { get; set; } = false;

    /// <summary>
    /// Video timestamp range this question corresponds to (e.g., "2:30-4:15")
    /// Only applicable when Source is Video
    /// </summary>
    public string? VideoTimestamp { get; set; }

    // Navigation properties

    /// <summary>
    /// Parent toolbox talk
    /// </summary>
    public ToolboxTalk ToolboxTalk { get; set; } = null!;
}
