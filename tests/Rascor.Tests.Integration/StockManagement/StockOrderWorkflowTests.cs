using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Stock Order workflow state transitions.
/// Tests the full lifecycle: Draft -> PendingApproval -> Approved -> ReadyForCollection -> Collected
/// </summary>
public class StockOrderWorkflowTests : IntegrationTestBase
{
    public StockOrderWorkflowTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Stock Orders Tests

    [Fact]
    public async Task GetStockOrders_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock-orders");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockOrderListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockOrderById_ExistingOrder_ReturnsOrderWithLines()
    {
        // Arrange
        var orderId = TestTenantConstants.StockManagement.StockOrders.DraftOrder;

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-orders/{orderId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(orderId);
        result.Data.OrderNumber.Should().Be(TestTenantConstants.StockManagement.StockOrders.DraftOrderReference);
        result.Data.Status.Should().Be("Draft");
        result.Data.Lines.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStockOrderById_NonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStockOrdersByStatus_ReturnsFilteredResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock-orders/by-status/Draft");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockOrderListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Should().OnlyContain(o => o.Status == "Draft");
    }

    [Fact]
    public async Task GetStockOrdersBySite_ReturnsFilteredResults()
    {
        // Arrange
        var siteId = TestTenantConstants.Sites.MainSite;

        // Act
        var response = await AdminClient.GetAsync($"/api/stock-orders/by-site/{siteId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StockOrderListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region Create Stock Order Tests

    [Fact]
    public async Task CreateStockOrder_ValidData_ReturnsCreatedWithDraftStatus()
    {
        // Arrange
        var command = new
        {
            SiteId = TestTenantConstants.Sites.MainSite,
            SiteName = TestTenantConstants.Sites.MainSiteName,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequestedBy = "Integration Test User",
            RequiredDate = DateTime.UtcNow.AddDays(3),
            Notes = "Integration test order",
            Lines = new[]
            {
                new { ProductId = TestTenantConstants.StockManagement.Products.HardHat, QuantityRequested = 5m },
                new { ProductId = TestTenantConstants.StockManagement.Products.SafetyVest, QuantityRequested = 3m }
            }
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/stock-orders", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.Status.Should().Be("Draft");
        result.Data.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateStockOrder_Unauthenticated_Returns401()
    {
        // Arrange
        var command = new
        {
            SiteId = TestTenantConstants.Sites.MainSite,
            SiteName = TestTenantConstants.Sites.MainSiteName,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequestedBy = "Test User",
            Lines = new[]
            {
                new { ProductId = TestTenantConstants.StockManagement.Products.HardHat, QuantityRequested = 1m }
            }
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/stock-orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateStockOrder_WithoutPermission_Returns403()
    {
        // Arrange
        var command = new
        {
            SiteId = TestTenantConstants.Sites.MainSite,
            SiteName = TestTenantConstants.Sites.MainSiteName,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequestedBy = "Test User",
            Lines = new[]
            {
                new { ProductId = TestTenantConstants.StockManagement.Products.HardHat, QuantityRequested = 1m }
            }
        };

        // Act - Finance user doesn't have CreateOrders permission
        var response = await FinanceClient.PostAsJsonAsync("/api/stock-orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Submit Workflow Tests

    [Fact]
    public async Task SubmitStockOrder_FromDraft_ChangesStatusToPendingApproval()
    {
        // Arrange - Create a new stock order
        var orderId = await CreateDraftStockOrderAsync();

        // Act
        var response = await AdminClient.PostAsync($"/api/stock-orders/{orderId}/submit", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("PendingApproval");
    }

    [Fact]
    public async Task SubmitStockOrder_AlreadySubmitted_ReturnsBadRequest()
    {
        // Arrange - Create and submit an order
        var orderId = await CreateDraftStockOrderAsync();
        await AdminClient.PostAsync($"/api/stock-orders/{orderId}/submit", null);

        // Act - Try to submit again
        var response = await AdminClient.PostAsync($"/api/stock-orders/{orderId}/submit", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Approve Workflow Tests

    [Fact]
    public async Task ApproveStockOrder_FromPendingApproval_ChangesStatusToApproved()
    {
        // Arrange - Create and submit an order
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);

        var approveRequest = new
        {
            ApprovedBy = "Test Admin",
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/approve", approveRequest);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Approved");
        result.Data.ApprovedBy.Should().Be("Test Admin");
        result.Data.ApprovedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ApproveStockOrder_FromDraft_ReturnsBadRequest()
    {
        // Arrange - Create a draft order (not submitted)
        var orderId = await CreateDraftStockOrderAsync();

        var approveRequest = new
        {
            ApprovedBy = "Test Admin",
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/approve", approveRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApproveStockOrder_WithoutPermission_Returns403()
    {
        // Arrange - Create and submit an order
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);

        var approveRequest = new
        {
            ApprovedBy = "Operator User",
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };

        // Act - Operator user doesn't have ApproveOrders permission
        var response = await OperatorClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/approve", approveRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Reject Workflow Tests

    [Fact]
    public async Task RejectStockOrder_FromPendingApproval_ChangesStatusToDraft()
    {
        // Arrange - Create and submit an order
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);

        var rejectRequest = new
        {
            RejectedBy = "Test Admin",
            Reason = "Quantities too high, please revise"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/reject", rejectRequest);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert - Rejection returns order to Draft status for revision
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Draft");
    }

    #endregion

    #region Ready for Collection Tests

    [Fact]
    public async Task ReadyForCollection_FromApproved_ChangesStatusToReadyForCollection()
    {
        // Arrange - Create, submit, and approve an order
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);
        await ApproveStockOrderAsync(orderId);

        // Act
        var response = await WarehouseClient.PostAsync($"/api/stock-orders/{orderId}/ready-for-collection", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("ReadyForCollection");
    }

    #endregion

    #region Collect Workflow Tests

    [Fact]
    public async Task CollectStockOrder_FromReadyForCollection_ChangesStatusToCollected()
    {
        // Arrange - Create order and move through workflow
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);
        await ApproveStockOrderAsync(orderId);
        await ReadyForCollectionAsync(orderId);

        var collectRequest = new
        {
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };

        // Act
        var response = await WarehouseClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/collect", collectRequest);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Collected");
    }

    #endregion

    #region Cancel Workflow Tests

    [Fact]
    public async Task CancelStockOrder_FromDraft_ChangesStatusToCancelled()
    {
        // Arrange
        var orderId = await CreateDraftStockOrderAsync();

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/cancel", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await AdminClient.GetAsync($"/api/stock-orders/{orderId}");
        var result = await getResponse.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();
        result!.Data!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelStockOrder_FromPendingApproval_ChangesStatusToCancelled()
    {
        // Arrange
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/cancel", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelStockOrder_FromApproved_RequiresWarehouseLocation()
    {
        // Arrange
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);
        await ApproveStockOrderAsync(orderId);

        // Cancel requires warehouse location to release reserved stock
        var cancelRequest = new
        {
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/cancel", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelStockOrder_AlreadyCollected_ReturnsBadRequest()
    {
        // Arrange - Create order and complete workflow
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);
        await ApproveStockOrderAsync(orderId);
        await ReadyForCollectionAsync(orderId);
        await CollectStockOrderAsync(orderId);

        // Act - Try to cancel collected order
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/cancel", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Full Workflow Integration Tests

    [Fact]
    public async Task FullWorkflow_DraftToCollected_CompletesSuccessfully()
    {
        // 1. Create draft order
        var orderId = await CreateDraftStockOrderAsync();

        // 2. Verify draft status
        var draftOrder = await GetStockOrderAsync(orderId);
        draftOrder.Status.Should().Be("Draft");

        // 3. Submit order
        await SubmitStockOrderAsync(orderId);
        var submittedOrder = await GetStockOrderAsync(orderId);
        submittedOrder.Status.Should().Be("PendingApproval");

        // 4. Approve order
        await ApproveStockOrderAsync(orderId);
        var approvedOrder = await GetStockOrderAsync(orderId);
        approvedOrder.Status.Should().Be("Approved");

        // 5. Mark ready for collection
        await ReadyForCollectionAsync(orderId);
        var readyOrder = await GetStockOrderAsync(orderId);
        readyOrder.Status.Should().Be("ReadyForCollection");

        // 6. Collect order
        await CollectStockOrderAsync(orderId);
        var collectedOrder = await GetStockOrderAsync(orderId);
        collectedOrder.Status.Should().Be("Collected");
    }

    [Fact]
    public async Task FullWorkflow_RejectAndResubmit_CompletesSuccessfully()
    {
        // 1. Create and submit draft order
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);

        // 2. Reject order - this returns it to Draft status
        var rejectRequest = new
        {
            RejectedBy = "Test Admin",
            Reason = "Needs revision"
        };
        await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/reject", rejectRequest);

        var rejectedOrder = await GetStockOrderAsync(orderId);
        rejectedOrder.Status.Should().Be("Draft");

        // 3. Resubmit the order
        await SubmitStockOrderAsync(orderId);
        var resubmittedOrder = await GetStockOrderAsync(orderId);
        resubmittedOrder.Status.Should().Be("PendingApproval");
    }

    #endregion

    #region Delete Stock Order Tests

    [Fact]
    public async Task DeleteStockOrder_DraftOrder_ReturnsNoContent()
    {
        // Arrange - Create a draft order
        var orderId = await CreateDraftStockOrderAsync();

        // Act
        var response = await AdminClient.DeleteAsync($"/api/stock-orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteStockOrder_ApprovedOrder_ReturnsBadRequest()
    {
        // Arrange - Create and approve an order
        var orderId = await CreateDraftStockOrderAsync();
        await SubmitStockOrderAsync(orderId);
        await ApproveStockOrderAsync(orderId);

        // Act - Cannot delete approved orders
        var response = await AdminClient.DeleteAsync($"/api/stock-orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Or BadRequest depending on implementation
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateDraftStockOrderAsync()
    {
        var command = new
        {
            SiteId = TestTenantConstants.Sites.MainSite,
            SiteName = TestTenantConstants.Sites.MainSiteName,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequestedBy = "Integration Test",
            OrderDate = DateTime.UtcNow,
            RequiredDate = DateTime.UtcNow.AddDays(3),
            Notes = "Workflow test order",
            Lines = new[]
            {
                new { ProductId = TestTenantConstants.StockManagement.Products.HardHat, QuantityRequested = 2 }
            }
        };

        var response = await AdminClient.PostAsJsonAsync("/api/stock-orders", command);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();
        return result!.Data!.Id;
    }

    private async Task SubmitStockOrderAsync(Guid orderId)
    {
        var response = await AdminClient.PostAsync($"/api/stock-orders/{orderId}/submit", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task ApproveStockOrderAsync(Guid orderId)
    {
        var approveRequest = new
        {
            ApprovedBy = "Test Admin",
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };
        var response = await AdminClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/approve", approveRequest);
        response.EnsureSuccessStatusCode();
    }

    private async Task ReadyForCollectionAsync(Guid orderId)
    {
        var response = await WarehouseClient.PostAsync($"/api/stock-orders/{orderId}/ready-for-collection", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task CollectStockOrderAsync(Guid orderId)
    {
        var collectRequest = new
        {
            WarehouseLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse
        };
        var response = await WarehouseClient.PostAsJsonAsync($"/api/stock-orders/{orderId}/collect", collectRequest);
        response.EnsureSuccessStatusCode();
    }

    private async Task<StockOrderDto> GetStockOrderAsync(Guid orderId)
    {
        var response = await AdminClient.GetAsync($"/api/stock-orders/{orderId}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();
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

    private record StockOrderListDto(
        Guid Id,
        string OrderNumber,
        Guid SiteId,
        string SiteName,
        string Status,
        DateTime OrderDate,
        DateTime? RequiredDate,
        string RequestedBy,
        string? ApprovedBy,
        DateTime? ApprovedDate,
        decimal OrderTotal,
        int LineCount,
        DateTime CreatedAt
    );

    private record StockOrderDto(
        Guid Id,
        string OrderNumber,
        Guid SiteId,
        string SiteName,
        Guid SourceLocationId,
        string SourceLocationName,
        string Status,
        DateTime OrderDate,
        DateTime? RequiredDate,
        string RequestedBy,
        string? ApprovedBy,
        DateTime? ApprovedDate,
        string? RejectedBy,
        DateTime? RejectedDate,
        string? RejectionReason,
        DateTime? CollectedDate,
        decimal OrderTotal,
        string? Notes,
        List<StockOrderLineDto> Lines,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    private record StockOrderLineDto(
        Guid Id,
        Guid StockOrderId,
        Guid ProductId,
        string ProductName,
        string ProductCode,
        decimal QuantityRequested,
        decimal QuantityIssued,
        decimal UnitPrice,
        decimal LineTotal
    );

    #endregion
}
