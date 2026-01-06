namespace Rascor.Modules.StockManagement.Domain.Enums;

/// <summary>
/// Status of a stocktake/inventory count
/// </summary>
public enum StocktakeStatus
{
    /// <summary>
    /// Stocktake has been created but not started
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Stocktake is currently in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Stocktake has been completed and adjustments created
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Stocktake has been cancelled
    /// </summary>
    Cancelled = 3
}
