using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Current stock level for a product at a specific location
/// </summary>
public class StockLevel : TenantEntity
{
    /// <summary>
    /// Product for this stock level
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Location where the stock is held
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Physical quantity of stock on hand
    /// </summary>
    public int QuantityOnHand { get; set; } = 0;

    /// <summary>
    /// Quantity reserved for pending orders
    /// </summary>
    public int QuantityReserved { get; set; } = 0;

    /// <summary>
    /// Quantity on order from suppliers (pending receipt)
    /// </summary>
    public int QuantityOnOrder { get; set; } = 0;

    /// <summary>
    /// Physical bin/shelf location within the location (optional - legacy field)
    /// </summary>
    public string? BinLocation { get; set; }

    /// <summary>
    /// Bay location within the stock location (optional)
    /// </summary>
    public Guid? BayLocationId { get; set; }

    /// <summary>
    /// Date of last stock movement
    /// </summary>
    public DateTime? LastMovementDate { get; set; }

    /// <summary>
    /// Date of last physical count/stocktake
    /// </summary>
    public DateTime? LastCountDate { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public StockLocation Location { get; set; } = null!;
    public BayLocation? BayLocation { get; set; }
}
