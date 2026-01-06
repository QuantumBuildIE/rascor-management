using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Goods Receipt (GRN) operations.
/// Tests creating GRNs with and without linked purchase orders.
/// </summary>
public class GoodsReceiptTests : IntegrationTestBase
{
    public GoodsReceiptTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Goods Receipts Tests

    [Fact]
    public async Task GetGoodsReceipts_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/goods-receipts");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<GoodsReceiptListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetGoodsReceiptById_ExistingGRN_ReturnsGRNWithLines()
    {
        // Arrange - Create a GRN first
        var grnId = await CreateGoodsReceiptAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/goods-receipts/{grnId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(grnId);
        result.Data.Lines.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGoodsReceiptById_NonExistingGRN_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/goods-receipts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGoodsReceiptsBySupplier_ReturnsFilteredResults()
    {
        // Arrange
        var supplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1;

        // Act
        var response = await AdminClient.GetAsync($"/api/goods-receipts/by-supplier/{supplierId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<GoodsReceiptListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetGoodsReceipts_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/goods-receipts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Create Goods Receipt Tests

    [Fact]
    public async Task CreateGoodsReceipt_WithoutPO_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            PurchaseOrderId = (Guid?)null,
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test User",
            Notes = "Direct delivery without PO",
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    PurchaseOrderLineId = (Guid?)null,
                    QuantityReceived = 50,
                    Notes = (string?)null,
                    BayLocationId = (Guid?)null,
                    BatchNumber = "BATCH-001",
                    ExpiryDate = (DateTime?)null
                }
            }
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.Lines.Should().HaveCount(1);
        result.Data.PurchaseOrderId.Should().BeNull();
    }

    [Fact]
    public async Task CreateGoodsReceipt_WithBatchTracking_StoresBatchInfo()
    {
        // Arrange
        var batchNumber = $"BATCH-{DateTime.Now.Ticks}";
        var expiryDate = DateTime.UtcNow.AddYears(2);

        var command = new
        {
            PurchaseOrderId = (Guid?)null,
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test User",
            Notes = (string?)null,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
                    PurchaseOrderLineId = (Guid?)null,
                    QuantityReceived = 100,
                    Notes = (string?)null,
                    BatchNumber = batchNumber,
                    ExpiryDate = expiryDate
                }
            }
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Lines[0].BatchNumber.Should().Be(batchNumber);
        result.Data.Lines[0].ExpiryDate.Should().BeCloseTo(expiryDate, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateGoodsReceipt_WithBayLocation_StoresLocation()
    {
        // Arrange
        var command = new
        {
            PurchaseOrderId = (Guid?)null,
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test User",
            Notes = (string?)null,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    PurchaseOrderLineId = (Guid?)null,
                    QuantityReceived = 25,
                    Notes = (string?)null,
                    BayLocationId = TestTenantConstants.StockManagement.BayLocations.BayA1
                }
            }
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Lines[0].BayLocationId.Should().Be(TestTenantConstants.StockManagement.BayLocations.BayA1);
    }

    [Fact]
    public async Task CreateGoodsReceipt_WithRejectedQuantity_RecordsRejection()
    {
        // Arrange
        var command = new
        {
            PurchaseOrderId = (Guid?)null,
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test User",
            Notes = (string?)null,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
                    PurchaseOrderLineId = (Guid?)null,
                    QuantityReceived = 95,
                    Notes = (string?)null,
                    QuantityRejected = 5m,
                    RejectionReason = "Damaged"
                }
            }
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Lines[0].QuantityReceived.Should().Be(95);
        result.Data.Lines[0].QuantityRejected.Should().Be(5m);
        result.Data.Lines[0].RejectionReason.Should().Be("Damaged");
    }

    [Fact]
    public async Task CreateGoodsReceipt_Unauthenticated_Returns401()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            StockLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityReceived = 10m,
                    UnitCost = 15.00m
                }
            }
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/goods-receipts", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateGoodsReceipt_WithoutPermission_Returns403()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            StockLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityReceived = 10m,
                    UnitCost = 15.00m
                }
            }
        };

        // Act - Finance user doesn't have ReceiveGoods permission
        var response = await FinanceClient.PostAsJsonAsync("/api/goods-receipts", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Create GRN from Purchase Order Tests

    [Fact]
    public async Task CreateGoodsReceipt_LinkedToPO_ReturnsCreated()
    {
        // Arrange - First create and confirm a PO
        var poId = await CreateAndConfirmPurchaseOrderAsync();

        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            PurchaseOrderId = poId,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test User",
            Notes = "Receiving against PO",
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityReceived = 25m,
                    UnitCost = TestTenantConstants.StockManagement.Products.HardHatCostPrice
                }
            }
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.PurchaseOrderId.Should().Be(poId);
    }

    [Fact]
    public async Task GetGoodsReceiptsByPO_ReturnsFilteredResults()
    {
        // Arrange - Create a GRN linked to a PO
        var poId = await CreateAndConfirmPurchaseOrderAsync();
        await CreateGoodsReceiptForPOAsync(poId);

        // Act
        var response = await AdminClient.GetAsync($"/api/goods-receipts/by-po/{poId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<GoodsReceiptListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Should().NotBeEmpty();
    }

    #endregion

    #region Delete Goods Receipt Tests

    [Fact]
    public async Task DeleteGoodsReceipt_ExistingGRN_ReturnsNoContent()
    {
        // Arrange
        var grnId = await CreateGoodsReceiptAsync();

        // Act
        var response = await WarehouseClient.DeleteAsync($"/api/goods-receipts/{grnId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await AdminClient.GetAsync($"/api/goods-receipts/{grnId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGoodsReceipt_NonExistingGRN_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await WarehouseClient.DeleteAsync($"/api/goods-receipts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Multiple Lines Tests

    [Fact]
    public async Task CreateGoodsReceipt_MultipleLines_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test User",
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityReceived = 50m,
                    UnitCost = TestTenantConstants.StockManagement.Products.HardHatCostPrice
                },
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
                    QuantityReceived = 100m,
                    UnitCost = TestTenantConstants.StockManagement.Products.SafetyVestCostPrice
                },
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.PowerDrill,
                    QuantityReceived = 10m,
                    UnitCost = TestTenantConstants.StockManagement.Products.PowerDrillCostPrice
                }
            }
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Lines.Should().HaveCount(3);
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateGoodsReceiptAsync()
    {
        var command = new
        {
            PurchaseOrderId = (Guid?)null,
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test",
            Notes = (string?)null,
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    PurchaseOrderLineId = (Guid?)null,
                    QuantityReceived = 20,
                    Notes = (string?)null
                }
            }
        };

        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<GoodsReceiptDto>>();
        return result!.Data!.Id;
    }

    private async Task<Guid> CreateAndConfirmPurchaseOrderAsync()
    {
        // Create PO
        var createCommand = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            DeliveryLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityOrdered = 25m,
                    UnitPrice = TestTenantConstants.StockManagement.Products.HardHatCostPrice
                }
            }
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/purchase-orders", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<ResultWrapper<PurchaseOrderDto>>();
        var poId = createResult!.Data!.Id;

        // Confirm PO
        await AdminClient.PostAsync($"/api/purchase-orders/{poId}/confirm", null);

        return poId;
    }

    private async Task CreateGoodsReceiptForPOAsync(Guid poId)
    {
        var command = new
        {
            SupplierId = TestTenantConstants.StockManagement.Suppliers.Supplier1,
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            PurchaseOrderId = poId,
            DeliveryNoteRef = $"DN-{Guid.NewGuid():N}"[..20],
            ReceiptDate = DateTime.UtcNow,
            ReceivedBy = "Integration Test",
            Lines = new[]
            {
                new
                {
                    ProductId = TestTenantConstants.StockManagement.Products.HardHat,
                    QuantityReceived = 25m,
                    UnitCost = TestTenantConstants.StockManagement.Products.HardHatCostPrice
                }
            }
        };

        var response = await WarehouseClient.PostAsJsonAsync("/api/goods-receipts", command);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(
        bool Success,
        T? Data,
        string? Message,
        List<string>? Errors
    );

    private record GoodsReceiptListDto(
        Guid Id,
        string ReceiptNumber,
        Guid SupplierId,
        string SupplierName,
        Guid? PurchaseOrderId,
        string? PurchaseOrderNumber,
        DateTime ReceiptDate,
        string ReceivedBy,
        int LineCount,
        DateTime CreatedAt
    );

    private record GoodsReceiptDto(
        Guid Id,
        string ReceiptNumber,
        Guid SupplierId,
        string SupplierName,
        Guid StockLocationId,
        string StockLocationName,
        Guid? PurchaseOrderId,
        string? PurchaseOrderNumber,
        string? DeliveryNoteReference,
        DateTime ReceiptDate,
        string ReceivedBy,
        string? Notes,
        List<GoodsReceiptLineDto> Lines,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    private record GoodsReceiptLineDto(
        Guid Id,
        Guid GoodsReceiptId,
        Guid ProductId,
        string ProductName,
        string ProductCode,
        decimal QuantityReceived,
        decimal? QuantityRejected,
        string? RejectionReason,
        decimal UnitCost,
        decimal LineTotal,
        Guid? BayLocationId,
        string? BayCode,
        string? BatchNumber,
        DateTime? ExpiryDate
    );

    private record PurchaseOrderDto(
        Guid Id,
        string OrderNumber,
        Guid SupplierId,
        string SupplierName,
        string Status
    );

    #endregion
}
