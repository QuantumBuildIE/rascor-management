using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Product CRUD operations.
/// </summary>
public class ProductTests : IntegrationTestBase
{
    public ProductTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Products Tests

    [Fact]
    public async Task GetProducts_ReturnsPagedResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/products?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PaginatedResult<ProductListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetProducts_WithSearchTerm_ReturnsFilteredResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/products?search=Hard Hat");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PaginatedResult<ProductListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Items.Should().Contain(p => p.ProductName.Contains("Hard Hat"));
    }

    [Fact]
    public async Task GetAllProducts_ReturnsNonPaginatedList()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/products/all");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductById_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = TestTenantConstants.StockManagement.Products.HardHat;

        // Act
        var response = await AdminClient.GetAsync($"/api/products/{productId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(productId);
        result.Data.ProductName.Should().Be(TestTenantConstants.StockManagement.Products.HardHatName);
        result.Data.ProductCode.Should().Be(TestTenantConstants.StockManagement.Products.HardHatSku);
    }

    [Fact]
    public async Task GetProductById_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProducts_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Create Product Tests

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsCreated()
    {
        // Arrange
        var uniqueSku = $"TEST-{Guid.NewGuid():N}"[..20];
        var command = new
        {
            ProductName = $"Test Product {Guid.NewGuid():N}"[..40],
            ProductCode = uniqueSku,
            CategoryId = TestTenantConstants.StockManagement.Categories.Safety,
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            UnitType = "Each",
            CostPrice = 10.00m,
            SellPrice = 15.00m,
            BaseRate = 10.00m,
            ReorderLevel = 5,
            ReorderQuantity = 20,
            ProductType = "Main Product",
            IsActive = true
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/products", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.ProductName.Should().Be(command.ProductName);
        result.Data.ProductCode.Should().Be(command.ProductCode);
        result.Data.CostPrice.Should().Be(command.CostPrice);
        result.Data.SellPrice.Should().Be(command.SellPrice);
    }

    [Fact]
    public async Task CreateProduct_MinimalData_ReturnsCreated()
    {
        // Arrange - Only required fields
        var uniqueSku = $"MIN-{Guid.NewGuid():N}"[..20];
        var command = new
        {
            ProductName = $"Minimal Product {Guid.NewGuid():N}"[..40],
            ProductCode = uniqueSku,
            CategoryId = TestTenantConstants.StockManagement.Categories.Tools,
            UnitType = "Each"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateProduct_Unauthenticated_Returns401()
    {
        // Arrange
        var command = new
        {
            ProductName = "Unauthenticated Test",
            ProductCode = "UNAUTH-001",
            CategoryId = TestTenantConstants.StockManagement.Categories.Safety,
            UnitType = "Each"
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithoutPermission_Returns403()
    {
        // Arrange
        var command = new
        {
            ProductName = "No Permission Test",
            ProductCode = "NOPERM-001",
            CategoryId = TestTenantConstants.StockManagement.Categories.Safety,
            UnitType = "Each"
        };

        // Act - Operator user doesn't have ManageProducts permission
        var response = await OperatorClient.PostAsJsonAsync("/api/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Update Product Tests

    [Fact]
    public async Task UpdateProduct_ValidData_ReturnsOk()
    {
        // Arrange - First create a product
        var uniqueSku = $"UPD-{Guid.NewGuid():N}".Substring(0, 20);
        var createCommand = new
        {
            ProductName = $"Product to Update {Guid.NewGuid():N}".Substring(0, 50),
            ProductCode = uniqueSku,
            CategoryId = TestTenantConstants.StockManagement.Categories.Safety,
            UnitType = "Each",
            CostPrice = 10.00m,
            SellPrice = 15.00m
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/products", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdResult = await createResponse.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();
        var productId = createdResult!.Data!.Id;

        // Update the product
        var updateCommand = new
        {
            ProductName = "Updated Product Name",
            ProductCode = uniqueSku,
            CategoryId = TestTenantConstants.StockManagement.Categories.Tools,
            UnitType = "Pack",
            CostPrice = 12.00m,
            SellPrice = 18.00m
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/products/{productId}", updateCommand);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.ProductName.Should().Be("Updated Product Name");
        result.Data.CategoryId.Should().Be(TestTenantConstants.StockManagement.Categories.Tools);
        result.Data.CostPrice.Should().Be(12.00m);
        result.Data.SellPrice.Should().Be(18.00m);
    }

    [Fact]
    public async Task UpdateProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new
        {
            ProductName = "Non-existent Product",
            ProductCode = "NONEXIST-001",
            CategoryId = TestTenantConstants.StockManagement.Categories.Safety,
            UnitType = "Each"
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/products/{nonExistentId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete Product Tests

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsNoContent()
    {
        // Arrange - Create a product to delete
        var uniqueSku = $"DEL-{Guid.NewGuid():N}".Substring(0, 20);
        var createCommand = new
        {
            ProductName = $"Product to Delete {Guid.NewGuid():N}".Substring(0, 50),
            ProductCode = uniqueSku,
            CategoryId = TestTenantConstants.StockManagement.Categories.Safety,
            UnitType = "Each"
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/products", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdResult = await createResponse.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();
        var productId = createdResult!.Data!.Id;

        // Act
        var response = await AdminClient.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted (soft delete)
        var getResponse = await AdminClient.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.DeleteAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithoutPermission_Returns403()
    {
        // Arrange
        var productId = TestTenantConstants.StockManagement.Products.HardHat;

        // Act - Operator doesn't have ManageProducts permission
        var response = await OperatorClient.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Product Pricing Tests

    [Fact]
    public async Task GetProduct_IncludesCostAndSellPrice()
    {
        // Arrange
        var productId = TestTenantConstants.StockManagement.Products.HardHat;

        // Act
        var response = await AdminClient.GetAsync($"/api/products/{productId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.CostPrice.Should().Be(TestTenantConstants.StockManagement.Products.HardHatCostPrice);
        result.Data.SellPrice.Should().Be(TestTenantConstants.StockManagement.Products.HardHatSellPrice);
    }

    [Fact]
    public async Task CreateProduct_WithPricing_StoresPricesCorrectly()
    {
        // Arrange
        var uniqueSku = $"PRICE-{Guid.NewGuid():N}".Substring(0, 20);
        var command = new
        {
            ProductName = $"Pricing Test Product {Guid.NewGuid():N}".Substring(0, 50),
            ProductCode = uniqueSku,
            CategoryId = TestTenantConstants.StockManagement.Categories.Materials,
            UnitType = "Each",
            CostPrice = 45.99m,
            SellPrice = 67.50m,
            BaseRate = 45.99m
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/products", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ProductDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.Data!.CostPrice.Should().Be(45.99m);
        result.Data.SellPrice.Should().Be(67.50m);
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(
        bool Success,
        T? Data,
        string? Message,
        List<string>? Errors
    );

    private record PaginatedResult<T>(
        List<T> Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages
    );

    private record ProductListDto(
        Guid Id,
        string ProductCode,
        string ProductName,
        Guid CategoryId,
        string CategoryName,
        Guid? SupplierId,
        string? SupplierName,
        string UnitType,
        decimal? CostPrice,
        decimal? SellPrice,
        int? ReorderLevel,
        string? ProductType,
        bool IsActive,
        DateTime CreatedAt
    );

    private record ProductDto(
        Guid Id,
        string ProductCode,
        string ProductName,
        string? Description,
        Guid CategoryId,
        string CategoryName,
        Guid? SupplierId,
        string? SupplierName,
        string UnitType,
        decimal? BaseRate,
        decimal? CostPrice,
        decimal? SellPrice,
        int? ReorderLevel,
        int? ReorderQuantity,
        string? ProductType,
        string? ImageUrl,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    #endregion
}
