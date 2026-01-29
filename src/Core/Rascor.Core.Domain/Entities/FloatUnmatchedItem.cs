using Rascor.Core.Domain.Common;

namespace Rascor.Core.Domain.Entities;

/// <summary>
/// Tracks Float items (people or projects) that couldn't be auto-matched to RASCOR entities.
/// Used for admin review and manual linking.
/// </summary>
public class FloatUnmatchedItem : TenantEntity
{
    /// <summary>
    /// Type of Float item: "Person" or "Project"
    /// </summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>
    /// Float ID (people_id or project_id from Float API)
    /// </summary>
    public int FloatId { get; set; }

    /// <summary>
    /// Name from Float (person name or project name)
    /// </summary>
    public string FloatName { get; set; } = string.Empty;

    /// <summary>
    /// Email from Float (for people only)
    /// </summary>
    public string? FloatEmail { get; set; }

    /// <summary>
    /// Suggested match ID (EmployeeId or SiteId) if fuzzy match found
    /// </summary>
    public Guid? SuggestedMatchId { get; set; }

    /// <summary>
    /// Name of the suggested match for display
    /// </summary>
    public string? SuggestedMatchName { get; set; }

    /// <summary>
    /// Confidence score for the suggested match (0.0 - 1.0)
    /// </summary>
    public decimal? MatchConfidence { get; set; }

    /// <summary>
    /// Status of this unmatched item: "Pending", "Linked", "Ignored"
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Final linked entity ID (EmployeeId or SiteId) when resolved
    /// </summary>
    public Guid? LinkedToId { get; set; }

    /// <summary>
    /// When this item was resolved (linked or ignored)
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// User who resolved this item
    /// </summary>
    public string? ResolvedBy { get; set; }
}
