using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Product/inventory item in the stock management system
/// </summary>
public class Product : TenantEntity
{
    /// <summary>
    /// Unique code identifying the product within the tenant
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Name/description of the product
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Category this product belongs to
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Supplier of this product (nullable - can be unassigned)
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Unit of measure (e.g., "Each", "Box", "Pallet", "Meter")
    /// </summary>
    public string UnitType { get; set; } = "Each";

    /// <summary>
    /// Cost price per unit
    /// </summary>
    public decimal BaseRate { get; set; }

    /// <summary>
    /// Stock level at which to reorder
    /// </summary>
    public int ReorderLevel { get; set; } = 0;

    /// <summary>
    /// Quantity to order when stock falls below reorder level
    /// </summary>
    public int ReorderQuantity { get; set; } = 0;

    /// <summary>
    /// Lead time in days for receiving product after ordering
    /// </summary>
    public int LeadTimeDays { get; set; } = 0;

    /// <summary>
    /// Whether the product is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// QR code data for product labels (optional)
    /// </summary>
    public string? QrCodeData { get; set; }

    /// <summary>
    /// Cost price per unit (purchase cost)
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Selling price per unit
    /// </summary>
    public decimal? SellPrice { get; set; }

    /// <summary>
    /// Type of product: "Main Product", "Ancillary Product", "Tool", "Consumable"
    /// </summary>
    public string? ProductType { get; set; }

    /// <summary>
    /// Stored filename of the product image
    /// </summary>
    public string? ImageFileName { get; set; }

    /// <summary>
    /// URL path to the product image
    /// </summary>
    public string? ImageUrl { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<ProductKitItem> KitItems { get; set; } = new List<ProductKitItem>();
    // public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}
