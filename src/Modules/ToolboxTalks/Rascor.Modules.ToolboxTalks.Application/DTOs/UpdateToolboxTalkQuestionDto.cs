using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.DTOs;

/// <summary>
/// DTO for updating a toolbox talk quiz question
/// If Id is null, a new question will be created
/// </summary>
public record UpdateToolboxTalkQuestionDto
{
    /// <summary>
    /// Question Id - null for new questions, existing Id for updates
    /// </summary>
    public Guid? Id { get; init; }

    public int QuestionNumber { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public QuestionType QuestionType { get; init; } = QuestionType.MultipleChoice;

    /// <summary>
    /// Options for multiple choice questions (as a list)
    /// </summary>
    public List<string>? Options { get; init; }

    public string CorrectAnswer { get; init; } = string.Empty;
    public int Points { get; init; } = 1;
}
