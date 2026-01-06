using Rascor.Core.Domain.Common;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Physical location where stock is held
/// </summary>
public class StockLocation : TenantEntity
{
    /// <summary>
    /// Unique code identifying the location within the tenant
    /// </summary>
    public string LocationCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the location
    /// </summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// Type of location (Warehouse, Site, Van, etc.)
    /// </summary>
    public LocationType LocationType { get; set; }

    /// <summary>
    /// Physical address of the location
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Whether the location is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    // public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    // public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
