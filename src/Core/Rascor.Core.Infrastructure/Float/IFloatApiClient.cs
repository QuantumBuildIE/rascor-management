using Rascor.Core.Infrastructure.Float.Models;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Client for interacting with the Float.com scheduling API.
/// Provides methods to fetch people, projects, and scheduled tasks.
/// </summary>
public interface IFloatApiClient
{
    /// <summary>
    /// Indicates whether the Float API is configured and ready to use.
    /// When false, methods will return empty results.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Gets all people (team members) from Float.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all people in Float</returns>
    Task<List<FloatPerson>> GetPeopleAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all projects from Float.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all projects in Float</returns>
    Task<List<FloatProject>> GetProjectsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all scheduled tasks for a specific date.
    /// </summary>
    /// <param name="date">The date to fetch tasks for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of tasks scheduled for the specified date</returns>
    Task<List<FloatTask>> GetTasksForDateAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Gets all scheduled tasks within a date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of tasks scheduled within the date range</returns>
    Task<List<FloatTask>> GetTasksForDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
}
