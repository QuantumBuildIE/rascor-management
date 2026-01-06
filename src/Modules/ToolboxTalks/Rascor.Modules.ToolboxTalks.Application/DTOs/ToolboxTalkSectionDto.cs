namespace Rascor.Modules.ToolboxTalks.Application.DTOs;

/// <summary>
/// DTO for a toolbox talk section
/// </summary>
public record ToolboxTalkSectionDto
{
    public Guid Id { get; init; }
    public Guid ToolboxTalkId { get; init; }
    public int SectionNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool RequiresAcknowledgment { get; init; }
}
