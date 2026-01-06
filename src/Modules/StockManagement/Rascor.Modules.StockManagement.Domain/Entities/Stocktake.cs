using Rascor.Core.Domain.Common;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Stocktake/inventory count session
/// </summary>
public class Stocktake : TenantEntity
{
    /// <summary>
    /// Unique stocktake number
    /// </summary>
    public string StocktakeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Location where the stocktake is being performed
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Date of the count
    /// </summary>
    public DateTime CountDate { get; set; }

    /// <summary>
    /// Current status of the stocktake
    /// </summary>
    public StocktakeStatus Status { get; set; } = StocktakeStatus.Draft;

    /// <summary>
    /// Person performing the count
    /// </summary>
    public string CountedBy { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the stocktake
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public StockLocation Location { get; set; } = null!;
    public ICollection<StocktakeLine> Lines { get; set; } = new List<StocktakeLine>();
}
