namespace Rascor.Modules.ToolboxTalks.Application.DTOs;

/// <summary>
/// DTO for updating a toolbox talk section
/// If Id is null, a new section will be created
/// </summary>
public record UpdateToolboxTalkSectionDto
{
    /// <summary>
    /// Section Id - null for new sections, existing Id for updates
    /// </summary>
    public Guid? Id { get; init; }

    public int SectionNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool RequiresAcknowledgment { get; init; } = true;
}
