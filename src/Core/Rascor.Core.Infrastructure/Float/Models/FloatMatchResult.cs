namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Result of matching a Float entity (person or project) to a RASCOR entity (employee or site).
/// </summary>
/// <typeparam name="T">The type of RASCOR entity (Employee or Site)</typeparam>
public class FloatMatchResult<T> where T : class
{
    /// <summary>
    /// Whether a match was found.
    /// </summary>
    public bool IsMatched { get; set; }

    /// <summary>
    /// The matched RASCOR entity (if found).
    /// </summary>
    public T? MatchedEntity { get; set; }

    /// <summary>
    /// ID of the matched entity.
    /// </summary>
    public Guid? MatchedEntityId { get; set; }

    /// <summary>
    /// Display name of the matched entity.
    /// </summary>
    public string? MatchedEntityName { get; set; }

    /// <summary>
    /// Method used for matching: "Existing", "Auto-Email", "Auto-Name", "Auto-Fuzzy", "Manual", "None"
    /// </summary>
    public string MatchMethod { get; set; } = "None";

    /// <summary>
    /// Confidence score for the match (0.0 - 1.0).
    /// 1.0 = exact match (email or existing link)
    /// 0.9 = exact name match
    /// 0.8+ = high confidence fuzzy match
    /// Below 0.8 = suggested match only, not auto-linked
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Whether admin review is recommended for this match.
    /// True for fuzzy matches or when no match was found.
    /// </summary>
    public bool RequiresReview { get; set; }
}
