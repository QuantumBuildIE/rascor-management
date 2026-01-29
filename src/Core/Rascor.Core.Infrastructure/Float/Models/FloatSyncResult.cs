namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Result of synchronizing and matching all Float entities for a tenant.
/// </summary>
public class FloatSyncResult
{
    /// <summary>
    /// Number of Float people successfully matched to RASCOR employees.
    /// </summary>
    public int PeopleMatched { get; set; }

    /// <summary>
    /// Number of Float people that could not be matched.
    /// </summary>
    public int PeopleUnmatched { get; set; }

    /// <summary>
    /// Number of Float projects successfully matched to RASCOR sites.
    /// </summary>
    public int ProjectsMatched { get; set; }

    /// <summary>
    /// Number of Float projects that could not be matched.
    /// </summary>
    public int ProjectsUnmatched { get; set; }

    /// <summary>
    /// Number of new unmatched items created for admin review.
    /// </summary>
    public int NewUnmatchedItems { get; set; }

    /// <summary>
    /// Any errors that occurred during the sync.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Whether the sync completed successfully (no errors).
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;

    /// <summary>
    /// Total number of people processed.
    /// </summary>
    public int TotalPeople => PeopleMatched + PeopleUnmatched;

    /// <summary>
    /// Total number of projects processed.
    /// </summary>
    public int TotalProjects => ProjectsMatched + ProjectsUnmatched;
}
