namespace Rascor.Modules.StockManagement.Domain.Enums;

/// <summary>
/// Type of stock transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Stock received via Goods Receipt Note (GRN)
    /// </summary>
    GrnReceipt = 0,

    /// <summary>
    /// Stock issued for a stock order
    /// </summary>
    OrderIssue = 1,

    /// <summary>
    /// Manual adjustment increasing stock
    /// </summary>
    AdjustmentIn = 2,

    /// <summary>
    /// Manual adjustment decreasing stock (e.g., damage, loss)
    /// </summary>
    AdjustmentOut = 3,

    /// <summary>
    /// Stock transferred into this location
    /// </summary>
    TransferIn = 4,

    /// <summary>
    /// Stock transferred out of this location
    /// </summary>
    TransferOut = 5,

    /// <summary>
    /// Adjustment from stocktake/inventory count
    /// </summary>
    StocktakeAdjustment = 6
}
