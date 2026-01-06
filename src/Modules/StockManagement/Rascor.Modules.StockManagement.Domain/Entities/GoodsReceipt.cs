using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Goods Receipt Note (GRN) recording receipt of goods from supplier
/// </summary>
public class GoodsReceipt : TenantEntity
{
    /// <summary>
    /// Unique goods receipt number
    /// </summary>
    public string GrnNumber { get; set; } = string.Empty;

    /// <summary>
    /// Related purchase order (nullable - can receive goods without PO)
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }

    /// <summary>
    /// Supplier the goods were received from
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Supplier's delivery note reference number
    /// </summary>
    public string? DeliveryNoteRef { get; set; }

    /// <summary>
    /// Location where goods were received
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Date the goods were received
    /// </summary>
    public DateTime ReceiptDate { get; set; }

    /// <summary>
    /// Person who received the goods
    /// </summary>
    public string ReceivedBy { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the receipt
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public StockLocation Location { get; set; } = null!;
    public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}
