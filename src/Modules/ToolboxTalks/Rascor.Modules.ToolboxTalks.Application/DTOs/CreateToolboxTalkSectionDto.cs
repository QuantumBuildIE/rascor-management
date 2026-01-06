namespace Rascor.Modules.ToolboxTalks.Application.DTOs;

/// <summary>
/// DTO for creating a new toolbox talk section
/// </summary>
public record CreateToolboxTalkSectionDto
{
    public int SectionNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool RequiresAcknowledgment { get; init; } = true;
}
