using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Individual line item on a stock order
/// </summary>
public class StockOrderLine : TenantEntity
{
    /// <summary>
    /// Stock order this line belongs to
    /// </summary>
    public Guid StockOrderId { get; set; }

    /// <summary>
    /// Product being requested
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity requested by the site
    /// </summary>
    public int QuantityRequested { get; set; }

    /// <summary>
    /// Quantity actually issued
    /// </summary>
    public int QuantityIssued { get; set; } = 0;

    /// <summary>
    /// Price per unit (from Product.BaseRate at time of order)
    /// </summary>
    public decimal UnitPrice { get; set; }

    // Navigation properties
    public StockOrder StockOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
