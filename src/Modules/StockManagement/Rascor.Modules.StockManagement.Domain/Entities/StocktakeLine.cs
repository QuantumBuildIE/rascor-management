using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Individual line item in a stocktake count
/// </summary>
public class StocktakeLine : TenantEntity
{
    /// <summary>
    /// Stocktake this line belongs to
    /// </summary>
    public Guid StocktakeId { get; set; }

    /// <summary>
    /// Product being counted
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// System quantity at the start of the count (snapshot)
    /// </summary>
    public int SystemQuantity { get; set; }

    /// <summary>
    /// Physical quantity counted (nullable until counted)
    /// </summary>
    public int? CountedQuantity { get; set; }

    /// <summary>
    /// Whether a stock adjustment has been created for this variance
    /// </summary>
    public bool AdjustmentCreated { get; set; } = false;

    /// <summary>
    /// Reason for variance: "Damaged", "Missing", "Found", "Data Entry Error", "Theft", "Other"
    /// </summary>
    public string? VarianceReason { get; set; }

    /// <summary>
    /// Bay location ID at the time of count (snapshot, no FK constraint)
    /// </summary>
    public Guid? BayLocationId { get; set; }

    /// <summary>
    /// Bay code at the time of count (denormalized for display/sorting)
    /// </summary>
    public string? BayCode { get; set; }

    // Navigation properties
    public Stocktake Stocktake { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
