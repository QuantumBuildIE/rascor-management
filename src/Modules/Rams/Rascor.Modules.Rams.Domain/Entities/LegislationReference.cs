using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Library of applicable legislation and regulations.
/// </summary>
public class LegislationReference : TenantEntity
{
    public string Code { get; set; } = string.Empty;  // e.g., "HASAWA-1974"
    public string Name { get; set; } = string.Empty;  // e.g., "Health and Safety at Work etc. Act 1974"
    public string? ShortName { get; set; }            // e.g., "HASAWA"
    public string? Description { get; set; }

    // Jurisdiction/Region
    public string? Jurisdiction { get; set; }  // "UK", "Ireland", "EU"

    // Keywords for AI/search matching
    public string? Keywords { get; set; }  // "general,duties,employer,employee"

    // Link to full legislation document
    public string? DocumentUrl { get; set; }

    // Which hazard categories this legislation applies to (null = all)
    public string? ApplicableCategories { get; set; }  // JSON array or comma-separated

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
