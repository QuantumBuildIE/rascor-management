using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Links hazards to their suggested control measures.
/// </summary>
public class HazardControlLink : BaseEntity
{
    public Guid HazardLibraryId { get; set; }
    public HazardLibrary HazardLibrary { get; set; } = null!;

    public Guid ControlMeasureLibraryId { get; set; }
    public ControlMeasureLibrary ControlMeasureLibrary { get; set; } = null!;

    public int SortOrder { get; set; }  // Order of controls for this hazard
}
