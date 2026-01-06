using Rascor.Core.Domain.Common;

namespace Rascor.Modules.StockManagement.Domain.Entities;

/// <summary>
/// Supplier providing products to the organization
/// </summary>
public class Supplier : TenantEntity
{
    /// <summary>
    /// Unique code identifying the supplier within the tenant
    /// </summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the supplier
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Primary contact person at the supplier
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Email address for the supplier
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number for the supplier
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Physical address of the supplier
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Payment terms agreed with the supplier (e.g., "Net 30", "COD")
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Whether the supplier is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    // public ICollection<Product> Products { get; set; } = new List<Product>();
}
