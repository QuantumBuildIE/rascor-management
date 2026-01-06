namespace Rascor.Modules.StockManagement.Domain.Enums;

/// <summary>
/// Type of stock location
/// </summary>
public enum LocationType
{
    /// <summary>
    /// Main warehouse or distribution center
    /// </summary>
    Warehouse = 0,

    /// <summary>
    /// Stock held at a construction site
    /// </summary>
    SiteStore = 1,

    /// <summary>
    /// Stock carried in a vehicle
    /// </summary>
    VanStock = 2,

    /// <summary>
    /// Stock in transit between locations
    /// </summary>
    Transit = 3
}
