using Rascor.Core.Domain.Common;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Purchase order for ordering products from suppliers
/// </summary>
public class PurchaseOrder : TenantEntity
{
    /// <summary>
    /// Unique purchase order number
    /// </summary>
    public string PoNumber { get; set; } = string.Empty;

    /// <summary>
    /// Supplier this order is placed with
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Date the order was placed
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Expected delivery date
    /// </summary>
    public DateTime? ExpectedDate { get; set; }

    /// <summary>
    /// Current status of the purchase order
    /// </summary>
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    /// <summary>
    /// Total value of the order (calculated from line items)
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Additional notes about the order
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
