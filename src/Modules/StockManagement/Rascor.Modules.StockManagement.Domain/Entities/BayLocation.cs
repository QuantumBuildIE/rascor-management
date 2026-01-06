using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Sub-location within a stock location (e.g., aisle, shelf, bin)
/// </summary>
public class BayLocation : TenantEntity
{
    /// <summary>
    /// Unique code identifying the bay (e.g., "A-1-3")
    /// </summary>
    public string BayCode { get; set; } = string.Empty;

    /// <summary>
    /// Optional descriptive name (e.g., "Aisle A, Shelf 1, Bin 3")
    /// </summary>
    public string? BayName { get; set; }

    /// <summary>
    /// Parent stock location this bay belongs to
    /// </summary>
    public Guid StockLocationId { get; set; }

    /// <summary>
    /// Optional capacity limit for this bay
    /// </summary>
    public int? Capacity { get; set; }

    /// <summary>
    /// Whether the bay is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Additional notes about this bay location
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public StockLocation StockLocation { get; set; } = null!;
}
