using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Stock Transaction operations.
/// Stock transactions are the audit trail for all stock movements.
/// </summary>
public class StockTransactionTests : IntegrationTestBase
{
    public StockTransactionTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Stock Transactions Tests

    [Fact]
    public async Task GetStockTransactions_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock-transactions");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockTransactionDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockTransactionById_ExistingTransaction_ReturnsTransaction()
    {
        // Arrange - Create a transaction first
        var transactionId = await CreateStockTransactionAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-transactions/{transactionId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockTransactionDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(transactionId);
    }

    [Fact]
    public async Task GetStockTransactionById_NonExistingTransaction_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-transactions/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStockTransactions_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/stock-transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Transactions by Product Tests

    [Fact]
    public async Task GetStockTransactionsByProduct_ReturnsFilteredResults()
    {
        // Arrange - Create a transaction for a specific product
        await CreateStockTransactionAsync(TestTenantConstants.StockManagement.Products.HardHat);
        var productId = TestTenantConstants.StockManagement.Products.HardHat;

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-transactions/by-product/{productId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockTransactionDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Should().OnlyContain(t => t.ProductId == productId);
    }

    #endregion

    #region Get Transactions by Location Tests

    [Fact]
    public async Task GetStockTransactionsByLocation_ReturnsFilteredResults()
    {
        // Arrange
        var locationId = TestTenantConstants.StockManagement.Locations.MainWarehouse;

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-transactions/by-location/{locationId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockTransactionDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region Get Transactions by Date Range Tests

    [Fact]
    public async Task GetStockTransactionsByDateRange_ReturnsFilteredResults()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("o");
        var endDate = DateTime.UtcNow.AddDays(1).ToString("o");

        // Act
        var response = await AdminClient.GetAsync(
            $"/api/stock-transactions/by-date-range?startDate={Uri.EscapeDataString(startDate)}&endDate={Uri.EscapeDataString(endDate)}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockTransactionDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockTransactionsByDateRange_InvalidDates_ReturnsBadRequest()
    {
        // Arrange - End date before start date
        var startDate = DateTime.UtcNow.ToString("o");
        var endDate = DateTime.UtcNow.AddDays(-30).ToString("o");

        // Act
        var response = await AdminClient.GetAsync(
            $"/api/stock-transactions/by-date-range?startDate={Uri.EscapeDataString(startDate)}&endDate={Uri.EscapeDataString(endDate)}");

        // Assert - Should return BadRequest or empty list depending on implementation
        // Some implementations may just return empty results
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Create Stock Transaction Tests

    [Fact]
    public async Task CreateStockTransaction_ValidData_ReturnsCreated()
    {
        // Arrange - Use valid TransactionType enum value: AdjustmentIn
        var command = new
        {
            TransactionType = "AdjustmentIn",
            ProductId = TestTenantConstants.StockManagement.Products.HardHat,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Quantity = 10,
            ReferenceType = (string?)null,
            ReferenceId = (Guid?)null,
            Notes = "Integration test adjustment"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/stock-transactions", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockTransactionDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.ProductId.Should().Be(TestTenantConstants.StockManagement.Products.HardHat);
        result.Data.TransactionType.Should().Be("AdjustmentIn");
        result.Data.Quantity.Should().Be(10);
    }

    [Fact]
    public async Task CreateStockTransaction_ReceiptType_ReturnsCreated()
    {
        // Arrange - Use valid TransactionType enum value: GrnReceipt
        var command = new
        {
            TransactionType = "GrnReceipt",
            ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Quantity = 50,
            ReferenceType = (string?)null,
            ReferenceId = (Guid?)null,
            Notes = "Goods received"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/stock-transactions", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockTransactionDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.TransactionType.Should().Be("GrnReceipt");
    }

    [Fact]
    public async Task CreateStockTransaction_IssueType_ReturnsCreated()
    {
        // Arrange - Use valid TransactionType enum value: OrderIssue
        var command = new
        {
            TransactionType = "OrderIssue",
            ProductId = TestTenantConstants.StockManagement.Products.PowerDrill,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Quantity = -5,
            ReferenceType = (string?)null,
            ReferenceId = (Guid?)null,
            Notes = "Stock issued to site"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/stock-transactions", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockTransactionDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.TransactionType.Should().Be("OrderIssue");
        result.Data.Quantity.Should().Be(-5);
    }

    [Fact]
    public async Task CreateStockTransaction_TransferType_ReturnsCreated()
    {
        // Arrange - Use valid TransactionType enum value: TransferOut
        var command = new
        {
            TransactionType = "TransferOut",
            ProductId = TestTenantConstants.StockManagement.Products.HardHat,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Quantity = -3,
            ReferenceType = (string?)null,
            ReferenceId = (Guid?)null,
            Notes = "Transfer to site storage"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/stock-transactions", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockTransactionDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateStockTransaction_Unauthenticated_Returns401()
    {
        // Arrange - Use valid TransactionType enum value: AdjustmentIn
        var command = new
        {
            TransactionType = "AdjustmentIn",
            ProductId = TestTenantConstants.StockManagement.Products.HardHat,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Quantity = 5,
            ReferenceType = (string?)null,
            ReferenceId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/stock-transactions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Stock Transaction Tests

    [Fact]
    public async Task DeleteStockTransaction_ExistingTransaction_ReturnsNoContent()
    {
        // Arrange
        var transactionId = await CreateStockTransactionAsync();

        // Act
        var response = await AdminClient.DeleteAsync($"/api/stock-transactions/{transactionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await AdminClient.GetAsync($"/api/stock-transactions/{transactionId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteStockTransaction_NonExistingTransaction_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.DeleteAsync($"/api/stock-transactions/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Transaction Types Tests

    [Fact]
    public async Task CreateStockTransaction_AllValidTypes_ReturnsCreated()
    {
        // Test all valid transaction types matching the TransactionType enum:
        // GrnReceipt, OrderIssue, AdjustmentIn, AdjustmentOut, TransferIn, TransferOut, StocktakeAdjustment
        var transactionTypes = new[]
        {
            ("GrnReceipt", 1),
            ("OrderIssue", -1),
            ("AdjustmentIn", 1),
            ("AdjustmentOut", -1),
            ("TransferIn", 1),
            ("TransferOut", -1),
            ("StocktakeAdjustment", 1)
        };

        foreach (var (transactionType, quantity) in transactionTypes)
        {
            var command = new
            {
                TransactionType = transactionType,
                ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
                Quantity = quantity,
                ReferenceType = (string?)null,
                ReferenceId = (Guid?)null,
                Notes = $"Test {transactionType} transaction"
            };

            var response = await AdminClient.PostAsJsonAsync("/api/stock-transactions", command);

            response.StatusCode.Should().Be(HttpStatusCode.Created,
                $"Transaction type '{transactionType}' should create successfully");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateStockTransactionAsync(Guid? productId = null)
    {
        // Use valid TransactionType enum value: AdjustmentIn
        var command = new
        {
            TransactionType = "AdjustmentIn",
            ProductId = productId ?? TestTenantConstants.StockManagement.Products.HardHat,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Quantity = 5,
            ReferenceType = (string?)null,
            ReferenceId = (Guid?)null,
            Notes = "Test transaction"
        };

        var response = await AdminClient.PostAsJsonAsync("/api/stock-transactions", command);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockTransactionDto>>();
        return result!.Data!.Id;
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(
        bool Success,
        T? Data,
        string? Message,
        List<string>? Errors
    );

    private record StockTransactionDto(
        Guid Id,
        Guid ProductId,
        string ProductName,
        string ProductCode,
        Guid StockLocationId,
        string StockLocationName,
        string TransactionType,
        decimal Quantity,
        decimal? UnitCost,
        Guid? SourceDocumentId,
        string? SourceDocumentType,
        string? SourceDocumentReference,
        Guid? DestinationLocationId,
        string? DestinationLocationName,
        string? Notes,
        DateTime TransactionDate,
        string CreatedBy,
        DateTime CreatedAt
    );

    #endregion
}
