using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Data;
using Rascor.Core.Infrastructure.Float.Models;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Service for auto-matching Float people/projects to RASCOR employees/sites.
/// Implements multiple matching strategies with confidence scoring and fallback flagging.
/// </summary>
public class FloatMatchingService : IFloatMatchingService
{
    private readonly IFloatApiClient _floatApiClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FloatMatchingService> _logger;

    /// <summary>
    /// Minimum confidence score for auto-linking (fuzzy matches below this require manual review).
    /// </summary>
    private const decimal AutoLinkThreshold = 0.8m;

    public FloatMatchingService(
        IFloatApiClient floatApiClient,
        ApplicationDbContext dbContext,
        ILogger<FloatMatchingService> logger)
    {
        _floatApiClient = floatApiClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FloatMatchResult<Employee>> MatchPersonToEmployeeAsync(
        Guid tenantId,
        FloatPerson floatPerson,
        CancellationToken ct = default)
    {
        // 1. Check if already linked
        var existingLink = await _dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId
                && e.FloatPersonId == floatPerson.PeopleId
                && !e.IsDeleted, ct);

        if (existingLink != null)
        {
            return new FloatMatchResult<Employee>
            {
                IsMatched = true,
                MatchedEntity = existingLink,
                MatchedEntityId = existingLink.Id,
                MatchedEntityName = existingLink.FullName,
                MatchMethod = "Existing",
                Confidence = 1.0m,
                RequiresReview = false
            };
        }

        // Get all active employees for matching (without Float links)
        var employees = await _dbContext.Employees
            .IgnoreQueryFilters()
            .Where(e => e.TenantId == tenantId && !e.IsDeleted && e.IsActive && e.FloatPersonId == null)
            .ToListAsync(ct);

        // 2. Try exact email match (highest confidence)
        if (!string.IsNullOrWhiteSpace(floatPerson.Email))
        {
            var emailMatch = employees.FirstOrDefault(e =>
                !string.IsNullOrWhiteSpace(e.Email) &&
                e.Email.Equals(floatPerson.Email, StringComparison.OrdinalIgnoreCase));

            if (emailMatch != null && floatPerson.PeopleId.HasValue)
            {
                LinkEmployeeToFloatPerson(emailMatch, floatPerson.PeopleId.Value, "Auto-Email");

                return new FloatMatchResult<Employee>
                {
                    IsMatched = true,
                    MatchedEntity = emailMatch,
                    MatchedEntityId = emailMatch.Id,
                    MatchedEntityName = emailMatch.FullName,
                    MatchMethod = "Auto-Email",
                    Confidence = 1.0m,
                    RequiresReview = false
                };
            }
        }

        // 3. Try exact name match
        var nameMatch = employees.FirstOrDefault(e =>
            e.FullName.Equals(floatPerson.Name, StringComparison.OrdinalIgnoreCase));

        if (nameMatch != null && floatPerson.PeopleId.HasValue)
        {
            LinkEmployeeToFloatPerson(nameMatch, floatPerson.PeopleId.Value, "Auto-Name");

            return new FloatMatchResult<Employee>
            {
                IsMatched = true,
                MatchedEntity = nameMatch,
                MatchedEntityId = nameMatch.Id,
                MatchedEntityName = nameMatch.FullName,
                MatchMethod = "Auto-Name",
                Confidence = 0.95m,
                RequiresReview = false
            };
        }

        // 4. Try fuzzy name match
        var fuzzyMatch = FindBestFuzzyMatch(floatPerson.Name, employees);

        if (fuzzyMatch.Match != null && fuzzyMatch.Score >= AutoLinkThreshold && floatPerson.PeopleId.HasValue)
        {
            // High confidence fuzzy match - auto-link but flag for review
            LinkEmployeeToFloatPerson(fuzzyMatch.Match, floatPerson.PeopleId.Value, "Auto-Fuzzy");

            return new FloatMatchResult<Employee>
            {
                IsMatched = true,
                MatchedEntity = fuzzyMatch.Match,
                MatchedEntityId = fuzzyMatch.Match.Id,
                MatchedEntityName = fuzzyMatch.Match.FullName,
                MatchMethod = "Auto-Fuzzy",
                Confidence = fuzzyMatch.Score,
                RequiresReview = true // Flag for admin to verify fuzzy matches
            };
        }

        // 5. No match - return unmatched result with suggested match if available
        var result = new FloatMatchResult<Employee>
        {
            IsMatched = false,
            MatchMethod = "None",
            Confidence = 0,
            RequiresReview = true
        };

        // Add suggested match if we have a low-confidence fuzzy match
        if (fuzzyMatch.Match != null)
        {
            result.MatchedEntity = fuzzyMatch.Match;
            result.MatchedEntityId = fuzzyMatch.Match.Id;
            result.MatchedEntityName = fuzzyMatch.Match.FullName;
            result.Confidence = fuzzyMatch.Score;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<FloatMatchResult<Site>> MatchProjectToSiteAsync(
        Guid tenantId,
        FloatProject floatProject,
        CancellationToken ct = default)
    {
        // 1. Check if already linked
        var existingLink = await _dbContext.Sites
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                && s.FloatProjectId == floatProject.ProjectId
                && !s.IsDeleted, ct);

        if (existingLink != null)
        {
            return new FloatMatchResult<Site>
            {
                IsMatched = true,
                MatchedEntity = existingLink,
                MatchedEntityId = existingLink.Id,
                MatchedEntityName = existingLink.SiteName,
                MatchMethod = "Existing",
                Confidence = 1.0m,
                RequiresReview = false
            };
        }

        // Get all active sites for matching (without Float links)
        var sites = await _dbContext.Sites
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == tenantId && !s.IsDeleted && s.IsActive && s.FloatProjectId == null)
            .ToListAsync(ct);

        // 2. Try exact name match
        var nameMatch = sites.FirstOrDefault(s =>
            s.SiteName.Equals(floatProject.Name, StringComparison.OrdinalIgnoreCase));

        if (nameMatch != null && floatProject.ProjectId.HasValue)
        {
            LinkSiteToFloatProject(nameMatch, floatProject.ProjectId.Value, "Auto-Name");

            return new FloatMatchResult<Site>
            {
                IsMatched = true,
                MatchedEntity = nameMatch,
                MatchedEntityId = nameMatch.Id,
                MatchedEntityName = nameMatch.SiteName,
                MatchMethod = "Auto-Name",
                Confidence = 1.0m,
                RequiresReview = false
            };
        }

        // 3. Try matching by project code to site code
        if (!string.IsNullOrWhiteSpace(floatProject.ProjectCode))
        {
            var codeMatch = sites.FirstOrDefault(s =>
                !string.IsNullOrWhiteSpace(s.SiteCode) &&
                s.SiteCode.Equals(floatProject.ProjectCode, StringComparison.OrdinalIgnoreCase));

            if (codeMatch != null && floatProject.ProjectId.HasValue)
            {
                LinkSiteToFloatProject(codeMatch, floatProject.ProjectId.Value, "Auto-Code");

                return new FloatMatchResult<Site>
                {
                    IsMatched = true,
                    MatchedEntity = codeMatch,
                    MatchedEntityId = codeMatch.Id,
                    MatchedEntityName = codeMatch.SiteName,
                    MatchMethod = "Auto-Code",
                    Confidence = 0.95m,
                    RequiresReview = false
                };
            }
        }

        // 4. Try fuzzy name match
        var fuzzyMatch = FindBestFuzzyMatchSite(floatProject.Name, sites);

        if (fuzzyMatch.Match != null && fuzzyMatch.Score >= AutoLinkThreshold && floatProject.ProjectId.HasValue)
        {
            LinkSiteToFloatProject(fuzzyMatch.Match, floatProject.ProjectId.Value, "Auto-Fuzzy");

            return new FloatMatchResult<Site>
            {
                IsMatched = true,
                MatchedEntity = fuzzyMatch.Match,
                MatchedEntityId = fuzzyMatch.Match.Id,
                MatchedEntityName = fuzzyMatch.Match.SiteName,
                MatchMethod = "Auto-Fuzzy",
                Confidence = fuzzyMatch.Score,
                RequiresReview = true
            };
        }

        // 5. No match
        var result = new FloatMatchResult<Site>
        {
            IsMatched = false,
            MatchMethod = "None",
            Confidence = 0,
            RequiresReview = true
        };

        if (fuzzyMatch.Match != null)
        {
            result.MatchedEntity = fuzzyMatch.Match;
            result.MatchedEntityId = fuzzyMatch.Match.Id;
            result.MatchedEntityName = fuzzyMatch.Match.SiteName;
            result.Confidence = fuzzyMatch.Score;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<FloatSyncResult> SyncAndMatchAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var result = new FloatSyncResult();

        if (!_floatApiClient.IsConfigured)
        {
            _logger.LogWarning("Float API is not configured. Skipping sync for tenant {TenantId}", tenantId);
            result.Errors.Add("Float API is not configured");
            return result;
        }

        try
        {
            // Fetch all data from Float
            var floatPeople = await _floatApiClient.GetPeopleAsync(ct);
            var floatProjects = await _floatApiClient.GetProjectsAsync(ct);

            _logger.LogInformation(
                "Float sync started for tenant {TenantId}: {PeopleCount} people, {ProjectCount} projects",
                tenantId, floatPeople.Count, floatProjects.Count);

            // Match people (only active ones)
            foreach (var person in floatPeople.Where(p => p.IsActive))
            {
                try
                {
                    var matchResult = await MatchPersonToEmployeeAsync(tenantId, person, ct);

                    if (matchResult.IsMatched)
                    {
                        result.PeopleMatched++;
                    }
                    else if (person.PeopleId.HasValue)
                    {
                        result.PeopleUnmatched++;
                        var isNew = await CreateOrUpdateUnmatchedItemAsync(
                            tenantId, "Person", person.PeopleId.Value,
                            person.Name, person.Email, matchResult, ct);
                        if (isNew) result.NewUnmatchedItems++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error matching Float person {PersonId} ({PersonName})",
                        person.PeopleId, person.Name);
                    result.Errors.Add($"Error matching person {person.Name}: {ex.Message}");
                }
            }

            // Match projects (only active ones)
            foreach (var project in floatProjects.Where(p => p.IsActive))
            {
                try
                {
                    var matchResult = await MatchProjectToSiteAsync(tenantId, project, ct);

                    if (matchResult.IsMatched)
                    {
                        result.ProjectsMatched++;
                    }
                    else if (project.ProjectId.HasValue)
                    {
                        result.ProjectsUnmatched++;
                        var isNew = await CreateOrUpdateUnmatchedItemAsync(
                            tenantId, "Project", project.ProjectId.Value,
                            project.Name, null, matchResult, ct);
                        if (isNew) result.NewUnmatchedItems++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error matching Float project {ProjectId} ({ProjectName})",
                        project.ProjectId, project.Name);
                    result.Errors.Add($"Error matching project {project.Name}: {ex.Message}");
                }
            }

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Float sync completed for tenant {TenantId}: People {Matched}/{Total}, Projects {ProjMatched}/{ProjTotal}, New unmatched: {NewUnmatched}",
                tenantId, result.PeopleMatched, result.TotalPeople,
                result.ProjectsMatched, result.TotalProjects, result.NewUnmatchedItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Float sync failed for tenant {TenantId}", tenantId);
            result.Errors.Add($"Sync failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task LinkPersonToEmployeeAsync(
        Guid tenantId,
        int floatPersonId,
        Guid employeeId,
        string resolvedBy,
        CancellationToken ct = default)
    {
        var employee = await _dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.TenantId == tenantId && !e.IsDeleted, ct);

        if (employee == null)
            throw new InvalidOperationException($"Employee {employeeId} not found");

        // Check if this Float person is already linked to another employee
        var existingLink = await _dbContext.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId
                && e.FloatPersonId == floatPersonId
                && !e.IsDeleted, ct);

        if (existingLink != null && existingLink.Id != employeeId)
        {
            throw new InvalidOperationException(
                $"Float person {floatPersonId} is already linked to employee {existingLink.FullName}");
        }

        LinkEmployeeToFloatPerson(employee, floatPersonId, "Manual");

        // Update unmatched item if it exists
        var unmatchedItem = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                && u.ItemType == "Person"
                && u.FloatId == floatPersonId
                && !u.IsDeleted, ct);

        if (unmatchedItem != null)
        {
            unmatchedItem.Status = "Linked";
            unmatchedItem.LinkedToId = employeeId;
            unmatchedItem.ResolvedAt = DateTime.UtcNow;
            unmatchedItem.ResolvedBy = resolvedBy;
            unmatchedItem.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Manually linked Float person {FloatPersonId} to employee {EmployeeId} ({EmployeeName}) by {ResolvedBy}",
            floatPersonId, employeeId, employee.FullName, resolvedBy);
    }

    /// <inheritdoc />
    public async Task LinkProjectToSiteAsync(
        Guid tenantId,
        int floatProjectId,
        Guid siteId,
        string resolvedBy,
        CancellationToken ct = default)
    {
        var site = await _dbContext.Sites
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == siteId && s.TenantId == tenantId && !s.IsDeleted, ct);

        if (site == null)
            throw new InvalidOperationException($"Site {siteId} not found");

        // Check if this Float project is already linked to another site
        var existingLink = await _dbContext.Sites
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                && s.FloatProjectId == floatProjectId
                && !s.IsDeleted, ct);

        if (existingLink != null && existingLink.Id != siteId)
        {
            throw new InvalidOperationException(
                $"Float project {floatProjectId} is already linked to site {existingLink.SiteName}");
        }

        LinkSiteToFloatProject(site, floatProjectId, "Manual");

        // Update unmatched item if it exists
        var unmatchedItem = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                && u.ItemType == "Project"
                && u.FloatId == floatProjectId
                && !u.IsDeleted, ct);

        if (unmatchedItem != null)
        {
            unmatchedItem.Status = "Linked";
            unmatchedItem.LinkedToId = siteId;
            unmatchedItem.ResolvedAt = DateTime.UtcNow;
            unmatchedItem.ResolvedBy = resolvedBy;
            unmatchedItem.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Manually linked Float project {FloatProjectId} to site {SiteId} ({SiteName}) by {ResolvedBy}",
            floatProjectId, siteId, site.SiteName, resolvedBy);
    }

    /// <inheritdoc />
    public async Task IgnoreUnmatchedItemAsync(
        Guid tenantId,
        Guid unmatchedItemId,
        string resolvedBy,
        CancellationToken ct = default)
    {
        var item = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == unmatchedItemId && u.TenantId == tenantId && !u.IsDeleted, ct);

        if (item == null)
            throw new InvalidOperationException($"Unmatched item {unmatchedItemId} not found");

        item.Status = "Ignored";
        item.ResolvedAt = DateTime.UtcNow;
        item.ResolvedBy = resolvedBy;
        item.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Ignored Float unmatched item {ItemId} ({ItemType}: {FloatName}) by {ResolvedBy}",
            unmatchedItemId, item.ItemType, item.FloatName, resolvedBy);
    }

    /// <inheritdoc />
    public async Task<List<FloatUnmatchedItem>> GetPendingUnmatchedItemsAsync(
        Guid tenantId,
        string? itemType = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && !u.IsDeleted && u.Status == "Pending");

        if (!string.IsNullOrWhiteSpace(itemType))
        {
            query = query.Where(u => u.ItemType == itemType);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    #region Private Helper Methods

    /// <summary>
    /// Links an employee to a Float person ID.
    /// </summary>
    private void LinkEmployeeToFloatPerson(Employee employee, int floatPersonId, string method)
    {
        employee.FloatPersonId = floatPersonId;
        employee.FloatLinkedAt = DateTime.UtcNow;
        employee.FloatLinkMethod = method;

        _logger.LogInformation(
            "Linked Employee {EmployeeId} ({EmployeeName}) to Float Person {FloatPersonId} via {Method}",
            employee.Id, employee.FullName, floatPersonId, method);
    }

    /// <summary>
    /// Links a site to a Float project ID.
    /// </summary>
    private void LinkSiteToFloatProject(Site site, int floatProjectId, string method)
    {
        site.FloatProjectId = floatProjectId;
        site.FloatLinkedAt = DateTime.UtcNow;
        site.FloatLinkMethod = method;

        _logger.LogInformation(
            "Linked Site {SiteId} ({SiteName}) to Float Project {FloatProjectId} via {Method}",
            site.Id, site.SiteName, floatProjectId, method);
    }

    /// <summary>
    /// Creates or updates an unmatched item record for admin review.
    /// </summary>
    /// <returns>True if a new item was created, false if existing item was updated.</returns>
    private async Task<bool> CreateOrUpdateUnmatchedItemAsync<T>(
        Guid tenantId,
        string itemType,
        int floatId,
        string floatName,
        string? floatEmail,
        FloatMatchResult<T> matchResult,
        CancellationToken ct) where T : class
    {
        var existing = await _dbContext.FloatUnmatchedItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                && u.ItemType == itemType
                && u.FloatId == floatId
                && !u.IsDeleted, ct);

        if (existing != null)
        {
            // Update if still pending (don't touch resolved items)
            if (existing.Status == "Pending")
            {
                existing.FloatName = floatName;
                existing.FloatEmail = floatEmail;
                existing.SuggestedMatchId = matchResult.MatchedEntityId;
                existing.SuggestedMatchName = matchResult.MatchedEntityName;
                existing.MatchConfidence = matchResult.Confidence;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            return false;
        }

        var unmatchedItem = new FloatUnmatchedItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ItemType = itemType,
            FloatId = floatId,
            FloatName = floatName,
            FloatEmail = floatEmail,
            SuggestedMatchId = matchResult.MatchedEntityId,
            SuggestedMatchName = matchResult.MatchedEntityName,
            MatchConfidence = matchResult.Confidence,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.FloatUnmatchedItems.AddAsync(unmatchedItem, ct);
        return true;
    }

    /// <summary>
    /// Finds the best fuzzy match for a Float person name among employees.
    /// </summary>
    private (Employee? Match, decimal Score) FindBestFuzzyMatch(string floatName, List<Employee> employees)
    {
        Employee? bestMatch = null;
        decimal bestScore = 0;

        var floatNameNormalized = NormalizeName(floatName);

        foreach (var employee in employees)
        {
            var employeeNameNormalized = NormalizeName(employee.FullName);
            var score = CalculateSimilarity(floatNameNormalized, employeeNameNormalized);

            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = employee;
            }
        }

        return (bestMatch, bestScore);
    }

    /// <summary>
    /// Finds the best fuzzy match for a Float project name among sites.
    /// </summary>
    private (Site? Match, decimal Score) FindBestFuzzyMatchSite(string floatName, List<Site> sites)
    {
        Site? bestMatch = null;
        decimal bestScore = 0;

        var floatNameNormalized = NormalizeName(floatName);

        foreach (var site in sites)
        {
            var siteNameNormalized = NormalizeName(site.SiteName);
            var score = CalculateSimilarity(floatNameNormalized, siteNameNormalized);

            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = site;
            }
        }

        return (bestMatch, bestScore);
    }

    /// <summary>
    /// Normalizes a name for comparison (lowercase, trim, collapse spaces).
    /// </summary>
    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Lowercase, trim, and collapse multiple spaces
        return string.Join(" ", name.ToLowerInvariant().Trim().Split(
            Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Calculates similarity between two strings using Levenshtein distance.
    /// Returns a score between 0.0 (completely different) and 1.0 (identical).
    /// </summary>
    private static decimal CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0;

        if (s1 == s2)
            return 1.0m;

        int distance = LevenshteinDistance(s1, s2);
        int maxLength = Math.Max(s1.Length, s2.Length);

        return 1.0m - ((decimal)distance / maxLength);
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        int[,] d = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            d[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
    }

    #endregion
}
