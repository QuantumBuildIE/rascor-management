using Rascor.Core.Domain.Common;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Library of common hazards that can be selected when creating risk assessments.
/// </summary>
public class HazardLibrary : TenantEntity
{
    public string Code { get; set; } = string.Empty;  // e.g., "HAZ-001"
    public string Name { get; set; } = string.Empty;  // e.g., "Fall from Height"
    public string? Description { get; set; }
    public HazardCategory Category { get; set; }

    // Keywords for AI/search matching
    public string? Keywords { get; set; }  // Comma-separated: "fall,height,ladder,scaffold"

    // Suggested values when this hazard is selected
    public int DefaultLikelihood { get; set; } = 3;
    public int DefaultSeverity { get; set; } = 4;

    // Who is typically at risk
    public string? TypicalWhoAtRisk { get; set; }  // "Employees, Contractors"

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<HazardControlLink> HazardControlLinks { get; set; } = new List<HazardControlLink>();
}
