using Rascor.Core.Domain.Common;

namespace Rascor.Modules.ToolboxTalks.Domain.Entities;

/// <summary>
/// Represents a content section within a toolbox talk
/// Sections contain HTML content that employees must read through
/// </summary>
public class ToolboxTalkSection : BaseEntity
{
    /// <summary>
    /// Foreign key to the parent toolbox talk
    /// </summary>
    public Guid ToolboxTalkId { get; set; }

    /// <summary>
    /// Order number for displaying sections in sequence
    /// </summary>
    public int SectionNumber { get; set; }

    /// <summary>
    /// Title of this section
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// HTML content of the section
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether the employee must acknowledge reading this section before proceeding
    /// </summary>
    public bool RequiresAcknowledgment { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// Parent toolbox talk
    /// </summary>
    public ToolboxTalk ToolboxTalk { get; set; } = null!;
}
