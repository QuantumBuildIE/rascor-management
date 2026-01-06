namespace Rascor.Modules.StockManagement.Domain.Enums;

/// <summary>
/// Status of a stock order from a site
/// </summary>
public enum StockOrderStatus
{
    /// <summary>
    /// Draft order, not yet submitted
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Submitted and awaiting approval
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// Order has been approved, stock is reserved
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Warehouse is picking the order
    /// </summary>
    AwaitingPick = 3,

    /// <summary>
    /// Order is ready for site to collect
    /// </summary>
    ReadyForCollection = 4,

    /// <summary>
    /// Order has been collected and stock issued
    /// </summary>
    Collected = 5,

    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled = 6
}
