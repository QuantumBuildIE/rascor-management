using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Individual line item on a goods receipt note
/// </summary>
public class GoodsReceiptLine : TenantEntity
{
    /// <summary>
    /// Goods receipt this line belongs to
    /// </summary>
    public Guid GoodsReceiptId { get; set; }

    /// <summary>
    /// Related purchase order line (nullable - can receive without PO)
    /// </summary>
    public Guid? PurchaseOrderLineId { get; set; }

    /// <summary>
    /// Product being received
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity received
    /// </summary>
    public int QuantityReceived { get; set; }

    /// <summary>
    /// Additional notes about this line
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Quantity rejected on receipt
    /// </summary>
    public decimal QuantityRejected { get; set; } = 0;

    /// <summary>
    /// Reason for rejection: "Damaged", "Wrong Item", "Expired", "Quality Issue", "Other"
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Batch/lot number for traceability
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Expiry date for perishable items
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Bay location where goods are being stored (optional)
    /// </summary>
    public Guid? BayLocationId { get; set; }

    // Navigation properties
    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public PurchaseOrderLine? PurchaseOrderLine { get; set; }
    public Product Product { get; set; } = null!;
    public BayLocation? BayLocation { get; set; }
}
