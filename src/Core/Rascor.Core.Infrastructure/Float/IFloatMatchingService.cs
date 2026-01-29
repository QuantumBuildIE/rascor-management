using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Float.Models;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Service for matching Float entities (people and projects) to RASCOR entities (employees and sites).
/// Provides automatic matching with fallback flagging for admin review.
/// </summary>
public interface IFloatMatchingService
{
    /// <summary>
    /// Match a Float person to a RASCOR employee.
    /// Tries matching in order: existing link, email, exact name, fuzzy name.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="floatPerson">The Float person to match</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Match result with confidence score and matched entity (if found)</returns>
    Task<FloatMatchResult<Employee>> MatchPersonToEmployeeAsync(
        Guid tenantId,
        FloatPerson floatPerson,
        CancellationToken ct = default);

    /// <summary>
    /// Match a Float project to a RASCOR site.
    /// Tries matching in order: existing link, exact name, fuzzy name.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="floatProject">The Float project to match</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Match result with confidence score and matched entity (if found)</returns>
    Task<FloatMatchResult<Site>> MatchProjectToSiteAsync(
        Guid tenantId,
        FloatProject floatProject,
        CancellationToken ct = default);

    /// <summary>
    /// Sync and match all Float people and projects for a tenant.
    /// Fetches data from Float API, attempts auto-matching, and creates
    /// unmatched items for entities that couldn't be matched.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Summary of sync results</returns>
    Task<FloatSyncResult> SyncAndMatchAllAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Manually link a Float person to an employee (admin action).
    /// Updates both the employee's FloatPersonId and resolves the unmatched item.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="floatPersonId">The Float person ID</param>
    /// <param name="employeeId">The RASCOR employee ID to link to</param>
    /// <param name="resolvedBy">Username of the admin performing the action</param>
    /// <param name="ct">Cancellation token</param>
    Task LinkPersonToEmployeeAsync(
        Guid tenantId,
        int floatPersonId,
        Guid employeeId,
        string resolvedBy,
        CancellationToken ct = default);

    /// <summary>
    /// Manually link a Float project to a site (admin action).
    /// Updates both the site's FloatProjectId and resolves the unmatched item.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="floatProjectId">The Float project ID</param>
    /// <param name="siteId">The RASCOR site ID to link to</param>
    /// <param name="resolvedBy">Username of the admin performing the action</param>
    /// <param name="ct">Cancellation token</param>
    Task LinkProjectToSiteAsync(
        Guid tenantId,
        int floatProjectId,
        Guid siteId,
        string resolvedBy,
        CancellationToken ct = default);

    /// <summary>
    /// Ignore an unmatched item (admin decides it's not needed in RASCOR).
    /// Marks the item as "Ignored" so it won't appear in admin review.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="unmatchedItemId">The unmatched item ID</param>
    /// <param name="resolvedBy">Username of the admin performing the action</param>
    /// <param name="ct">Cancellation token</param>
    Task IgnoreUnmatchedItemAsync(
        Guid tenantId,
        Guid unmatchedItemId,
        string resolvedBy,
        CancellationToken ct = default);

    /// <summary>
    /// Get all pending unmatched items for admin review.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="itemType">Optional filter: "Person" or "Project"</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of unmatched items</returns>
    Task<List<FloatUnmatchedItem>> GetPendingUnmatchedItemsAsync(
        Guid tenantId,
        string? itemType = null,
        CancellationToken ct = default);
}
