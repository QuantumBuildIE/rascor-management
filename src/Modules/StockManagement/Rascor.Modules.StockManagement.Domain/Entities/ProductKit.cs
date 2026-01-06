using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

public class ProductKit : TenantEntity
{
    public string KitCode { get; set; } = string.Empty;  // e.g., "KIT-WRAP-001"
    public string KitName { get; set; } = string.Empty;  // e.g., "Waterproofing Kit"
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }  // Optional category grouping

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Calculated totals (based on default quantities and current prices)
    public decimal TotalCost { get; set; }  // Sum of item costs
    public decimal TotalPrice { get; set; }  // Sum of item prices (if using SellPrice)

    // Navigation
    public Category? Category { get; set; }
    public ICollection<ProductKitItem> Items { get; set; } = new List<ProductKitItem>();
}
