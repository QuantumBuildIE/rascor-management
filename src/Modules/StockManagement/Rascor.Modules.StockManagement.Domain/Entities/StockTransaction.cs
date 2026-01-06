using Rascor.Core.Domain.Common;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Record of stock movement (in/out)
/// </summary>
public class StockTransaction : TenantEntity
{
    /// <summary>
    /// Unique transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date and time of the transaction
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Type of stock transaction
    /// </summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Product being moved
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Location where the transaction occurred
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Quantity moved (positive for stock in, negative for stock out)
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Type of related document (e.g., "GRN", "StockOrder", "Stocktake")
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// ID of related document
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// Additional notes about the transaction
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public StockLocation Location { get; set; } = null!;
}
