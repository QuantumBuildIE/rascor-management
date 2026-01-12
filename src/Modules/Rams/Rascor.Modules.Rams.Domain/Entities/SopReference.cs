using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Standard Operating Procedures library.
/// </summary>
public class SopReference : TenantEntity
{
    public string SopId { get; set; } = string.Empty;  // e.g., "SOP-09"
    public string Topic { get; set; } = string.Empty;  // e.g., "Cutting Operations"
    public string? Description { get; set; }

    // Keywords for AI/search matching
    public string? TaskKeywords { get; set; }  // "cutting,grinding,saw,angle grinder"

    // RASCOR policy excerpt
    public string? PolicySnippet { get; set; }

    // Full procedure details
    public string? ProcedureDetails { get; set; }

    // Applicable legislation
    public string? ApplicableLegislation { get; set; }

    // Link to full SOP document (e.g., Google Drive)
    public string? DocumentUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
