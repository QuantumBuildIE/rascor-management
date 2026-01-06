using Rascor.Core.Domain.Common;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Individual line item on a purchase order
/// </summary>
public class PurchaseOrderLine : TenantEntity
{
    /// <summary>
    /// Purchase order this line belongs to
    /// </summary>
    public Guid PurchaseOrderId { get; set; }

    /// <summary>
    /// Product being ordered
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int QuantityOrdered { get; set; }

    /// <summary>
    /// Quantity received so far
    /// </summary>
    public int QuantityReceived { get; set; } = 0;

    /// <summary>
    /// Price per unit
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Status of this line item
    /// </summary>
    public PurchaseOrderLineStatus LineStatus { get; set; } = PurchaseOrderLineStatus.Open;

    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
