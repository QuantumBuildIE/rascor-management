using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

public class ProductKitItem : TenantEntity
{
    public Guid ProductKitId { get; set; }
    public Guid ProductId { get; set; }

    public decimal DefaultQuantity { get; set; } = 1;  // Default qty when kit is added
    public int SortOrder { get; set; }
    public string? Notes { get; set; }  // e.g., "Optional item" or "Use 2x for large areas"

    // Navigation
    public ProductKit ProductKit { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
