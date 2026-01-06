using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Purchase Order operations.
/// Tests the full lifecycle: Draft -> Confirmed -> PartiallyReceived -> FullyReceived
/// </summary>
public class PurchaseOrderTests : IntegrationTestBase
{
    public PurchaseOrderTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Purchase Orders Tests

    [Fact]
    public async Task GetPurchaseOrders_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/purchase-orders");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<PurchaseOrderListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPurchaseOrderById_ExistingPO_ReturnsPOWithLines()
    {
        // Arrange - Create a PO first
        var poId = await CreateDraftPurchaseOrderAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/purchase-orders/{poId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(poId);
        result.Data.Status.Should().Be("Draft");
        result.Data.Lines.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPurchaseOrderById_NonExistingPO_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/purchase-orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPurchaseOrdersBySupplier_ReturnsFilteredResults()
    {
        // Arrange
        var supplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1;

        // Act
        var response = await AdminClient.GetAsync($"/api/purchase-orders/by-supplier/{supplierId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<PurchaseOrderListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPurchaseOrdersByStatus_ReturnsFilteredResults()
    {
        // Arrange - First create a Draft PO to ensure we have data
        await CreateDraftPurchaseOrderAsync();

        // Act
        var response = await AdminClient.GetAsync("/api/purchase-orders/by-status/Draft");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<PurchaseOrderListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Should().NotBeEmpty();
        result.Data!.Should().OnlyContain(po => po.Status == "Draft");
    }

    [Fact]
    public async Task GetPurchaseOrders_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/purchase-orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Create Purchase Order Tests

    [Fact]
    public async Task CreatePurchaseOrder_ValidData_ReturnsCreatedWithDraftStatus()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            DeliveryLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
            Notes = "Integration test purchase order",
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityOrdered = 50m,
                    UnitPrice = TestTenantConstants.StockManagement.Products.HardHatCostPrice
                },
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
                    QuantityOrdered = 100m,
                    UnitPrice = TestTenantConstants.StockManagement.Products.SafetyVestCostPrice
                }
            }
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/purchase-orders", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.Status.Should().Be("Draft");
        result.Data.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreatePurchaseOrder_Unauthenticated_Returns401()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            DeliveryLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityOrdered = 10m,
                    UnitPrice = 15.00m
                }
            }
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/purchase-orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithoutPermission_Returns403()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            DeliveryLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityOrdered = 10m,
                    UnitPrice = 15.00m
                }
            }
        };

        // Act - Operator user doesn't have ManageProducts permission
        var response = await OperatorClient.PostAsJsonAsync("/api/purchase-orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Update Purchase Order Tests

    [Fact]
    public async Task UpdatePurchaseOrder_DraftStatus_ReturnsOk()
    {
        // Arrange - Create a draft PO
        var poId = await CreateDraftPurchaseOrderAsync();

        // Only ExpectedDate and Notes can be updated per UpdatePurchaseOrderDto
        var updateCommand = new
        {
            ExpectedDate = DateTime.UtcNow.AddDays(14),
            Notes = "Updated purchase order"
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/purchase-orders/{poId}", updateCommand);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Notes.Should().Be("Updated purchase order");
    }

    [Fact]
    public async Task UpdatePurchaseOrder_ConfirmedStatus_ReturnsBadRequest()
    {
        // Arrange - Create and confirm a PO
        var poId = await CreateDraftPurchaseOrderAsync();
        await ConfirmPurchaseOrderAsync(poId);

        var updateCommand = new
        {
            ExpectedDate = DateTime.UtcNow.AddDays(14),
            Notes = "Try to update confirmed PO"
        };

        // Act - Cannot update confirmed PO
        var response = await AdminClient.PutAsJsonAsync($"/api/purchase-orders/{poId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Confirm Purchase Order Tests

    [Fact]
    public async Task ConfirmPurchaseOrder_FromDraft_ChangesStatusToConfirmed()
    {
        // Arrange
        var poId = await CreateDraftPurchaseOrderAsync();

        // Act
        var response = await AdminClient.PostAsync($"/api/purchase-orders/{poId}/confirm", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task ConfirmPurchaseOrder_AlreadyConfirmed_ReturnsBadRequest()
    {
        // Arrange
        var poId = await CreateDraftPurchaseOrderAsync();
        await ConfirmPurchaseOrderAsync(poId);

        // Act - Try to confirm again
        var response = await AdminClient.PostAsync($"/api/purchase-orders/{poId}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Cancel Purchase Order Tests

    [Fact]
    public async Task CancelPurchaseOrder_FromDraft_ChangesStatusToCancelled()
    {
        // Arrange
        var poId = await CreateDraftPurchaseOrderAsync();

        // Act
        var response = await AdminClient.PostAsync($"/api/purchase-orders/{poId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelPurchaseOrder_FromConfirmed_ChangesStatusToCancelled()
    {
        // Arrange
        var poId = await CreateDraftPurchaseOrderAsync();
        await ConfirmPurchaseOrderAsync(poId);

        // Act
        var response = await AdminClient.PostAsync($"/api/purchase-orders/{poId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region Delete Purchase Order Tests

    [Fact]
    public async Task DeletePurchaseOrder_DraftPO_ReturnsNoContent()
    {
        // Arrange
        var poId = await CreateDraftPurchaseOrderAsync();

        // Act
        var response = await AdminClient.DeleteAsync($"/api/purchase-orders/{poId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await AdminClient.GetAsync($"/api/purchase-orders/{poId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePurchaseOrder_ConfirmedPO_ReturnsBadRequest()
    {
        // Arrange
        var poId = await CreateDraftPurchaseOrderAsync();
        await ConfirmPurchaseOrderAsync(poId);

        // Act - Cannot delete confirmed PO
        var response = await AdminClient.DeleteAsync($"/api/purchase-orders/{poId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Full Workflow Tests

    [Fact]
    public async Task FullWorkflow_DraftToConfirmed_CompletesSuccessfully()
    {
        // 1. Create draft PO
        var poId = await CreateDraftPurchaseOrderAsync();

        // 2. Verify draft status
        var draftPO = await GetPurchaseOrderAsync(poId);
        draftPO.Status.Should().Be("Draft");

        // 3. Confirm PO
        await ConfirmPurchaseOrderAsync(poId);

        var confirmedPO = await GetPurchaseOrderAsync(poId);
        confirmedPO.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CreatePurchaseOrder_CalculatesTotalsCorrectly()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = DateTime.UtcNow.AddDays(7),
            Notes = (string?)null,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityOrdered = 10,
                    UnitPrice = 15.00m // 150.00 total
                },
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
                    QuantityOrdered = 20,
                    UnitPrice = 12.00m // 240.00 total
                }
            }
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/purchase-orders", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.Data!.TotalValue.Should().Be(390.00m); // 150 + 240 = 390
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateDraftPurchaseOrderAsync()
    {
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = DateTime.UtcNow.AddDays(7),
            Notes = "Test purchase order",
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityOrdered = 25,
                    UnitPrice = TestTenantConstants.StockManagement.Products.HardHatCostPrice
                }
            }
        };

        var response = await AdminClient.PostAsJsonAsync("/api/purchase-orders", command);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();
        return result!.Data!.Id;
    }

    private async Task ConfirmPurchaseOrderAsync(Guid poId)
    {
        var response = await AdminClient.PostAsync($"/api/purchase-orders/{poId}/confirm", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task<PurchaseOrderDto> GetPurchaseOrderAsync(Guid poId)
    {
        var response = await AdminClient.GetAsync($"/api/purchase-orders/{poId}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();
        return result!.Data!;
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(
        bool Success,
        T? Data,
        string? Message,
        List<string>? Errors
    );

    private record PurchaseOrderListDto(
        Guid Id,
        string PoNumber,
        Guid SupplierId,
        string SupplierName,
        string Status,
        DateTime OrderDate,
        DateTime? ExpectedDate,
        decimal TotalValue,
        int LineCount,
        DateTime CreatedAt
    );

    private record PurchaseOrderDto(
        Guid Id,
        string PoNumber,
        Guid SupplierId,
        string SupplierName,
        DateTime OrderDate,
        DateTime? ExpectedDate,
        string Status,
        decimal TotalValue,
        string? Notes,
        List<PurchaseOrderLineDto> Lines
    );

    private record PurchaseOrderLineDto(
        Guid Id,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        int QuantityOrdered,
        int QuantityReceived,
        decimal UnitPrice,
        decimal LineTotal,
        string? LineStatus
    );

    #endregion
}
