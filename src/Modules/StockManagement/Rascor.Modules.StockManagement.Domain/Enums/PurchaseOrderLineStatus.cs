namespace Rascor.Modules.StockManagement.Domain.Enums;

/// <summary>
/// Status of an individual purchase order line
/// </summary>
public enum PurchaseOrderLineStatus
{
    /// <summary>
    /// Line item has not been received yet
    /// </summary>
    Open = 0,

    /// <summary>
    /// Line item has been partially received
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Line item has been fully received
    /// </summary>
    Complete = 2,

    /// <summary>
    /// Line item has been cancelled
    /// </summary>
    Cancelled = 3
}
