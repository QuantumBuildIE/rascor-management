using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Product category for organizing inventory items
/// </summary>
public class Category : TenantEntity
{
    /// <summary>
    /// Name of the category
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Display order for the category (lower numbers appear first)
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Whether the category is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    // public ICollection<Product> Products { get; set; } = new List<Product>();
}
