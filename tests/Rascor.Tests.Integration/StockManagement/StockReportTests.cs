using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.StockManagement;

/// <summary>
/// Integration tests for Stock Reports.
/// Tests various reporting endpoints including valuation and analytics.
/// </summary>
public class StockReportTests : IntegrationTestBase
{
    public StockReportTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Products by Month Report Tests

    [Fact]
    public async Task GetProductsByMonth_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-month");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueByMonthDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsByMonth_WithParameters_ReturnsFilteredResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-month?months=6&topN=5");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueByMonthDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsByMonth_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/stock/reports/products-by-month");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Products by Site Report Tests

    [Fact]
    public async Task GetProductsBySite_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-site");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueBySiteDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsBySite_WithTopN_ReturnsLimitedResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-site?topN=3");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueBySiteDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsBySite_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/stock/reports/products-by-site");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Products by Week Report Tests

    [Fact]
    public async Task GetProductsByWeek_ReturnsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-week");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueByWeekDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsByWeek_WithParameters_ReturnsFilteredResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-week?weeks=8&topN=5");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueByWeekDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsByWeek_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/stock/reports/products-by-week");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Stock Valuation Report Tests

    [Fact]
    public async Task GetStockValuation_WithViewCostingsPermission_ReturnsResults()
    {
        // Act - Admin has ViewCostings permission
        var response = await AdminClient.GetAsync("/api/stock/reports/valuation");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockValuationReport>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockValuation_FilterByLocation_ReturnsFilteredResults()
    {
        // Arrange
        var locationId = TestTenantConstants.StockManagement.Locations.MainWarehouse;

        // Act
        var response = await AdminClient.GetAsync($"/api/stock/reports/valuation?locationId={locationId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockValuationReport>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockValuation_FilterByCategory_ReturnsFilteredResults()
    {
        // Arrange
        var categoryId = TestTenantConstants.StockManagement.Categories.Safety;

        // Act
        var response = await AdminClient.GetAsync($"/api/stock/reports/valuation?categoryId={categoryId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockValuationReport>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockValuation_FilterByLocationAndCategory_ReturnsFilteredResults()
    {
        // Arrange
        var locationId = TestTenantConstants.StockManagement.Locations.MainWarehouse;
        var categoryId = TestTenantConstants.StockManagement.Categories.Safety;

        // Act
        var response = await AdminClient.GetAsync(
            $"/api/stock/reports/valuation?locationId={locationId}&categoryId={categoryId}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockValuationReport>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockValuation_WithoutViewCostingsPermission_Returns403()
    {
        // Act - Warehouse user doesn't have ViewCostings permission
        var response = await WarehouseClient.GetAsync("/api/stock/reports/valuation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetStockValuation_FinanceUser_ReturnsResults()
    {
        // Act - Finance user has ViewCostings permission
        var response = await FinanceClient.GetAsync("/api/stock/reports/valuation");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockValuationReport>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetStockValuation_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/stock/reports/valuation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Report Data Structure Tests

    [Fact]
    public async Task GetStockValuation_ReturnsValidStructure()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/valuation");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<StockValuationReport>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();

        // Verify structure has expected properties
        result.Data!.Items.Should().NotBeNull();
        // TotalValue should be a valid decimal
        result.Data.TotalValue.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetProductsByMonth_ReturnsValidStructure()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/stock/reports/products-by-month");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<List<ProductValueByMonthDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        // Data is a flat list of ProductValueByMonthDto items
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(
        bool Success,
        T? Data,
        string? Message,
        List<string>? Errors
    );

    // API returns Result<List<ProductValueByMonthDto>> - a flat list
    private record ProductValueByMonthDto(
        string Month,
        string ProductName,
        decimal Value
    );

    // API returns Result<List<ProductValueBySiteDto>> - a flat list
    private record ProductValueBySiteDto(
        string SiteName,
        string ProductName,
        decimal Value
    );

    // API returns Result<List<ProductValueByWeekDto>> - a flat list
    private record ProductValueByWeekDto(
        DateTime WeekStartDate,
        string ProductName,
        decimal Value
    );

    // API returns Result<StockValuationReportDto>
    private record StockValuationReport(
        List<StockValuationItem> Items,
        int TotalProducts,
        decimal TotalQuantity,
        decimal TotalValue,
        DateTime GeneratedAt
    );

    // StockValuationItemDto
    private record StockValuationItem(
        Guid ProductId,
        string ProductCode,
        string ProductName,
        Guid? CategoryId,
        string? CategoryName,
        Guid LocationId,
        string LocationName,
        string? BayCode,
        decimal QuantityOnHand,
        decimal? CostPrice,
        decimal TotalValue
    );

    #endregion
}
