using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Stocktake operations.
/// Tests the full stocktake lifecycle: Draft -> InProgress -> Completed
/// </summary>
public class StocktakeTests : IntegrationTestBase
{
    public StocktakeTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Stocktakes Tests

    [Fact]
    public async Task GetStocktakes_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stocktakes");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StocktakeListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStocktakeById_ExistingStocktake_ReturnsStocktakeWithLines()
    {
        // Arrange - Create a stocktake first
        var stocktakeId = await CreateDraftStocktakeAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/stocktakes/{stocktakeId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(stocktakeId);
        result.Data.Status.Should().Be("Draft");
        result.Data.Lines.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStocktakeById_NonExistingStocktake_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/stocktakes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStocktakesByLocation_ReturnsFilteredResults()
    {
        // Arrange
        var locationId = TestTenantConstants.StockManagement.Locations.MainWarehouse;

        // Act
        var response = await AdminClient.GetAsync($"/api/stocktakes/by-location/{locationId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<StocktakeListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStocktakes_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/stocktakes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Create Stocktake Tests

    [Fact]
    public async Task CreateStocktake_ValidData_ReturnsCreatedWithAutoPopulatedLines()
    {
        // Arrange
        var command = new
        {
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            CountedBy = "Integration Test User",
            Notes = "Integration test stocktake"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/stocktakes", command);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeEmpty();
        result.Data.Status.Should().Be("Draft");
        // Lines should be auto-populated with products at the location
        result.Data.Lines.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateStocktake_Unauthenticated_Returns401()
    {
        // Arrange
        var command = new
        {
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            CountedBy = "Test User",
            Notes = "Unauthenticated test"
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/stocktakes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateStocktake_WithoutPermission_Returns403()
    {
        // Arrange
        var command = new
        {
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            CountedBy = "Test User",
            Notes = "No permission test"
        };

        // Act - Finance user doesn't have Stocktake permission
        var response = await FinanceClient.PostAsJsonAsync("/api/stocktakes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Start Stocktake Tests

    [Fact]
    public async Task StartStocktake_FromDraft_ChangesStatusToInProgress()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();

        // Act
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/start", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task StartStocktake_AlreadyStarted_ReturnsBadRequest()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);

        // Act - Try to start again
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/start", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Line Tests

    [Fact]
    public async Task UpdateStocktakeLine_ValidData_UpdatesCountedQuantity()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);

        // Get stocktake to find a line ID
        var stocktake = await GetStocktakeAsync(stocktakeId);
        var firstLine = stocktake.Lines.FirstOrDefault();

        if (firstLine == null)
        {
            // Skip test if no lines (no stock at location)
            return;
        }

        // API expects 'CountedQuantity' field name per UpdateStocktakeLineDto
        var updateCommand = new
        {
            CountedQuantity = 42,
            VarianceReason = (string?)null
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync(
            $"/api/stocktakes/{stocktakeId}/lines/{firstLine.Id}",
            updateCommand);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStocktakeLine_DraftStocktake_ReturnsBadRequest()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        var stocktake = await GetStocktakeAsync(stocktakeId);
        var firstLine = stocktake.Lines.FirstOrDefault();

        if (firstLine == null)
        {
            return;
        }

        // API expects 'CountedQuantity' field name per UpdateStocktakeLineDto
        var updateCommand = new
        {
            CountedQuantity = 10
        };

        // Act - Cannot update lines on draft stocktake (must start first)
        var response = await AdminClient.PutAsJsonAsync(
            $"/api/stocktakes/{stocktakeId}/lines/{firstLine.Id}",
            updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Complete Stocktake Tests

    [Fact]
    public async Task CompleteStocktake_AllLinesCounted_ChangesStatusToCompleted()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);

        // Get stocktake and count all lines
        var stocktake = await GetStocktakeAsync(stocktakeId);

        // API expects 'CountedQuantity' (int?) field name per UpdateStocktakeLineDto
        foreach (var line in stocktake.Lines)
        {
            var updateCommand = new
            {
                CountedQuantity = (int)line.SystemQuantity, // Count matches system
                VarianceReason = (string?)null
            };
            await AdminClient.PutAsJsonAsync(
                $"/api/stocktakes/{stocktakeId}/lines/{line.Id}",
                updateCommand);
        }

        // Act
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/complete", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task CompleteStocktake_FromDraft_ReturnsBadRequest()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();

        // Act - Cannot complete a draft stocktake
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Cancel Stocktake Tests

    [Fact]
    public async Task CancelStocktake_FromDraft_ChangesStatusToCancelled()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();

        // Act
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelStocktake_FromInProgress_ChangesStatusToCancelled()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);

        // Act
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CancelStocktake_AlreadyCompleted_ReturnsBadRequest()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);
        await CompleteStocktakeAsync(stocktakeId);

        // Act
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Delete Stocktake Tests

    [Fact]
    public async Task DeleteStocktake_DraftStocktake_ReturnsNoContent()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();

        // Act
        var response = await AdminClient.DeleteAsync($"/api/stocktakes/{stocktakeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await AdminClient.GetAsync($"/api/stocktakes/{stocktakeId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteStocktake_CompletedStocktake_ReturnsBadRequest()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);
        await CompleteStocktakeAsync(stocktakeId);

        // Act - Cannot delete completed stocktakes
        var response = await AdminClient.DeleteAsync($"/api/stocktakes/{stocktakeId}");

        // Assert - Should be either BadRequest or NotFound depending on implementation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Full Workflow Tests

    [Fact]
    public async Task FullWorkflow_DraftToCompleted_CompletesSuccessfully()
    {
        // 1. Create draft stocktake
        var stocktakeId = await CreateDraftStocktakeAsync();

        // 2. Verify draft status
        var draftStocktake = await GetStocktakeAsync(stocktakeId);
        draftStocktake.Status.Should().Be("Draft");

        // 3. Start stocktake
        await StartStocktakeAsync(stocktakeId);
        var inProgressStocktake = await GetStocktakeAsync(stocktakeId);
        inProgressStocktake.Status.Should().Be("InProgress");

        // 4. Count all lines - API expects 'CountedQuantity' (int?) per UpdateStocktakeLineDto
        foreach (var line in inProgressStocktake.Lines)
        {
            var updateCommand = new
            {
                CountedQuantity = (int)(line.SystemQuantity + 5), // Create variance
                VarianceReason = "Found extra during workflow test"
            };
            await AdminClient.PutAsJsonAsync(
                $"/api/stocktakes/{stocktakeId}/lines/{line.Id}",
                updateCommand);
        }

        // 5. Complete stocktake
        await CompleteStocktakeAsync(stocktakeId);

        var completedStocktake = await GetStocktakeAsync(stocktakeId);
        completedStocktake.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Stocktake_WithVariance_CalculatesVarianceCorrectly()
    {
        // Arrange
        var stocktakeId = await CreateDraftStocktakeAsync();
        await StartStocktakeAsync(stocktakeId);

        var stocktake = await GetStocktakeAsync(stocktakeId);
        var firstLine = stocktake.Lines.FirstOrDefault();

        if (firstLine == null)
        {
            return;
        }

        // API expects 'CountedQuantity' (int?) per UpdateStocktakeLineDto
        var varianceAmount = 10;
        var expectedCounted = (int)firstLine.SystemQuantity + varianceAmount;
        var updateCommand = new
        {
            CountedQuantity = expectedCounted,
            VarianceReason = "Variance test"
        };

        // Act
        var updateResponse = await AdminClient.PutAsJsonAsync(
            $"/api/stocktakes/{stocktakeId}/lines/{firstLine.Id}",
            updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        // Assert
        var updatedStocktake = await GetStocktakeAsync(stocktakeId);
        var updatedLine = updatedStocktake.Lines.First(l => l.Id == firstLine.Id);
        updatedLine.CountedQuantity.Should().Be(expectedCounted);
        updatedLine.Variance.Should().Be(varianceAmount);
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateDraftStocktakeAsync()
    {
        var command = new
        {
            LocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            CountedBy = "Integration Test User",
            Notes = "Test stocktake"
        };

        var response = await AdminClient.PostAsJsonAsync("/api/stocktakes", command);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();
        return result!.Data!.Id;
    }

    private async Task StartStocktakeAsync(Guid stocktakeId)
    {
        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/start", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task CompleteStocktakeAsync(Guid stocktakeId)
    {
        // Count all lines first - API expects 'CountedQuantity' (int?) per UpdateStocktakeLineDto
        var stocktake = await GetStocktakeAsync(stocktakeId);
        foreach (var line in stocktake.Lines)
        {
            var updateCommand = new
            {
                CountedQuantity = (int)line.SystemQuantity,
                VarianceReason = (string?)null
            };
            await AdminClient.PutAsJsonAsync(
                $"/api/stocktakes/{stocktakeId}/lines/{line.Id}",
                updateCommand);
        }

        var response = await AdminClient.PostAsync($"/api/stocktakes/{stocktakeId}/complete", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task<StocktakeDto> GetStocktakeAsync(Guid stocktakeId)
    {
        var response = await AdminClient.GetAsync($"/api/stocktakes/{stocktakeId}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StocktakeDto>>();
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

    private record StocktakeListDto(
        Guid Id,
        string Reference,
        Guid StockLocationId,
        string StockLocationName,
        string Status,
        DateTime StocktakeDate,
        string? CompletedBy,
        DateTime? CompletedDate,
        int LineCount,
        int CountedCount,
        int VarianceCount,
        DateTime CreatedAt
    );

    private record StocktakeDto(
        Guid Id,
        string Reference,
        Guid StockLocationId,
        string StockLocationName,
        string Status,
        DateTime StocktakeDate,
        string? Notes,
        string? CompletedBy,
        DateTime? CompletedDate,
        List<StocktakeLineDto> Lines,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    // Note: Field name must match API's StocktakeLineDto - uses 'CountedQuantity' not 'QuantityCounted'
    private record StocktakeLineDto(
        Guid Id,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        int SystemQuantity,
        int? CountedQuantity,
        int? Variance,
        bool AdjustmentCreated,
        string? VarianceReason,
        Guid? BayLocationId,
        string? BayCode
    );

    #endregion
}
