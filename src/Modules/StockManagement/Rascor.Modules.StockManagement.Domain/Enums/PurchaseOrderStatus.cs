namespace Rascor.Modules.StockManagement.Domain.Enums;

/// <summary>
/// Status of a purchase order
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// Draft order, not yet confirmed with supplier
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Order confirmed and sent to supplier
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Some items have been received but not all
    /// </summary>
    PartiallyReceived = 2,

    /// <summary>
    /// All items have been received
    /// </summary>
    FullyReceived = 3,

    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled = 4
}
