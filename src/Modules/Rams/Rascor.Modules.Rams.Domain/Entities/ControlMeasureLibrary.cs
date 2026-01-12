using Rascor.Core.Domain.Common;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Library of control measures that can be applied to hazards.
/// </summary>
public class ControlMeasureLibrary : TenantEntity
{
    public string Code { get; set; } = string.Empty;  // e.g., "CTL-001"
    public string Name { get; set; } = string.Empty;  // e.g., "Edge Protection"
    public string Description { get; set; } = string.Empty;  // Full control measure text

    // Hierarchy of controls classification
    public ControlHierarchy Hierarchy { get; set; }

    // Which hazard categories this control applies to
    public HazardCategory? ApplicableToCategory { get; set; }

    // Keywords for AI/search matching
    public string? Keywords { get; set; }

    // How much this control typically reduces risk
    public int TypicalLikelihoodReduction { get; set; } = 1;  // Reduce by 1-3
    public int TypicalSeverityReduction { get; set; } = 0;    // Usually doesn't change severity

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation - links to hazards this control applies to
    public ICollection<HazardControlLink> HazardControlLinks { get; set; } = new List<HazardControlLink>();
}
