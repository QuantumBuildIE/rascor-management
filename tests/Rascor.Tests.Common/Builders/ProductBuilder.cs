using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating Product entities in tests.
/// </summary>
public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tenantId = TestTenant.TestTenantConstants.TenantId;
    private Guid _categoryId;
    private Guid? _supplierId = null;
    private string _productCode = $"PROD-{Guid.NewGuid().ToString()[..8]}";
    private string _productName = "Test Product";
    private string _unitType = "Each";
    private decimal _baseRate = 10.00m;
    private decimal? _costPrice = null;
    private decimal? _sellPrice = null;
    private int _reorderLevel = 10;
    private int _reorderQuantity = 20;
    private int _leadTimeDays = 7;
    private string? _productType = "Main Product";
    private bool _isActive = true;

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public ProductBuilder WithCategory(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ProductBuilder WithSupplier(Guid supplierId)
    {
        _supplierId = supplierId;
        return this;
    }

    public ProductBuilder WithProductCode(string productCode)
    {
        _productCode = productCode;
        return this;
    }

    public ProductBuilder WithProductName(string productName)
    {
        _productName = productName;
        return this;
    }

    public ProductBuilder WithUnitType(string unitType)
    {
        _unitType = unitType;
        return this;
    }

    public ProductBuilder WithBaseRate(decimal baseRate)
    {
        _baseRate = baseRate;
        return this;
    }

    public ProductBuilder WithPricing(decimal costPrice, decimal sellPrice)
    {
        _costPrice = costPrice;
        _sellPrice = sellPrice;
        _baseRate = costPrice;
        return this;
    }

    public ProductBuilder WithReorderSettings(int level, int quantity)
    {
        _reorderLevel = level;
        _reorderQuantity = quantity;
        return this;
    }

    public ProductBuilder WithLeadTimeDays(int days)
    {
        _leadTimeDays = days;
        return this;
    }

    public ProductBuilder WithProductType(string productType)
    {
        _productType = productType;
        return this;
    }

    public ProductBuilder AsTool()
    {
        _productType = "Tool";
        return this;
    }

    public ProductBuilder AsConsumable()
    {
        _productType = "Consumable";
        return this;
    }

    public ProductBuilder AsAncillary()
    {
        _productType = "Ancillary Product";
        return this;
    }

    public ProductBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    public Product Build()
    {
        return new Product
        {
            Id = _id,
            TenantId = _tenantId,
            CategoryId = _categoryId,
            SupplierId = _supplierId,
            ProductCode = _productCode,
            ProductName = _productName,
            UnitType = _unitType,
            BaseRate = _baseRate,
            CostPrice = _costPrice,
            SellPrice = _sellPrice,
            ReorderLevel = _reorderLevel,
            ReorderQuantity = _reorderQuantity,
            LeadTimeDays = _leadTimeDays,
            ProductType = _productType,
            IsActive = _isActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-builder"
        };
    }

    /// <summary>
    /// Creates a simple product with basic pricing.
    /// </summary>
    public static Product CreateSimple(Guid categoryId, string productName, decimal costPrice, decimal sellPrice, Guid? id = null)
    {
        return new ProductBuilder()
            .WithId(id ?? Guid.NewGuid())
            .WithCategory(categoryId)
            .WithProductName(productName)
            .WithPricing(costPrice, sellPrice)
            .Build();
    }

    /// <summary>
    /// Creates a PPE product.
    /// </summary>
    public static Product CreatePPE(Guid categoryId, string productName, decimal costPrice, decimal sellPrice, Guid? id = null)
    {
        return new ProductBuilder()
            .WithId(id ?? Guid.NewGuid())
            .WithCategory(categoryId)
            .WithProductName(productName)
            .WithPricing(costPrice, sellPrice)
            .WithProductType("Main Product")
            .Build();
    }

    /// <summary>
    /// Creates a tool product.
    /// </summary>
    public static Product CreateTool(Guid categoryId, string productName, decimal costPrice, decimal sellPrice, Guid? id = null)
    {
        return new ProductBuilder()
            .WithId(id ?? Guid.NewGuid())
            .WithCategory(categoryId)
            .WithProductName(productName)
            .WithPricing(costPrice, sellPrice)
            .AsTool()
            .Build();
    }

    /// <summary>
    /// Creates a low stock product (below reorder level).
    /// </summary>
    public static Product CreateLowStock(Guid categoryId, string productName, int reorderLevel = 50, Guid? id = null)
    {
        return new ProductBuilder()
            .WithId(id ?? Guid.NewGuid())
            .WithCategory(categoryId)
            .WithProductName(productName)
            .WithPricing(10.00m, 20.00m)
            .WithReorderSettings(reorderLevel, reorderLevel * 2)
            .Build();
    }
}
