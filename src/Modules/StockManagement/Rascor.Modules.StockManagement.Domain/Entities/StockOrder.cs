using Rascor.Core.Domain.Common;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Stock order from a site requesting materials
/// </summary>
public class StockOrder : TenantEntity
{
    /// <summary>
    /// Unique stock order number
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Site ID (may reference external system)
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Cached site name for performance
    /// </summary>
    public string SiteName { get; set; } = string.Empty;

    /// <summary>
    /// Date the order was created
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Date the materials are required
    /// </summary>
    public DateTime? RequiredDate { get; set; }

    /// <summary>
    /// Current status of the order
    /// </summary>
    public StockOrderStatus Status { get; set; } = StockOrderStatus.Draft;

    /// <summary>
    /// Total value of the order (calculated from line items)
    /// </summary>
    public decimal OrderTotal { get; set; }

    /// <summary>
    /// Person who requested the stock
    /// </summary>
    public string RequestedBy { get; set; } = string.Empty;

    /// <summary>
    /// Person who approved the order
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Date the order was approved
    /// </summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>
    /// Date the order was collected
    /// </summary>
    public DateTime? CollectedDate { get; set; }

    /// <summary>
    /// Additional notes about the order
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Source location ID where stock will be picked from
    /// </summary>
    public Guid SourceLocationId { get; set; }

    /// <summary>
    /// Source proposal ID if this order was created from a proposal conversion
    /// </summary>
    public Guid? SourceProposalId { get; set; }

    /// <summary>
    /// Source proposal number for display purposes
    /// </summary>
    public string? SourceProposalNumber { get; set; }

    // Navigation properties
    public StockLocation SourceLocation { get; set; } = null!;
    public ICollection<StockOrderLine> Lines { get; set; } = new List<StockOrderLine>();
}
