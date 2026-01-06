using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Proposals;

/// <summary>
/// Integration tests for converting won proposals to stock orders.
/// </summary>
public class ConvertToStockOrderTests : IntegrationTestBase
{
    public ConvertToStockOrderTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Can Convert Tests

    [Fact]
    public async Task CanConvert_WonProposal_ReturnsTrue()
    {
        // Arrange - Create a won proposal
        var proposalId = await CreateWonProposalAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/can-convert");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanConvert_DraftProposal_ReturnsFalse()
    {
        // Arrange - Create a draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/can-convert");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanConvert_ApprovedProposal_ReturnsFalse()
    {
        // Arrange - Create an approved (not won) proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/can-convert");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanConvert_LostProposal_ReturnsFalse()
    {
        // Arrange - Create a lost proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);
        await LoseProposalAsync(proposalId, "Test");

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/can-convert");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeFalse();
    }

    #endregion

    #region Preview Conversion Tests

    [Fact]
    public async Task PreviewConversion_WonProposal_ReturnsPreview()
    {
        // Arrange - Create a won proposal with items
        var proposalId = await CreateWonProposalAsync();

        var previewDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequiredDate = DateTime.UtcNow.AddDays(7),
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/preview-conversion", previewDto);
        var result = await response.Content.ReadFromJsonAsync<ConversionPreviewDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PreviewConversion_AllItemsMode_IncludesAllLineItems()
    {
        // Arrange - Create a won proposal with multiple items
        var proposalId = await CreateWonProposalWithMultipleItemsAsync();

        var previewDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/preview-conversion", previewDto);
        var result = await response.Content.ReadFromJsonAsync<ConversionPreviewDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Items.Count.Should().BeGreaterOrEqualTo(2); // We add 2 items
    }

    #endregion

    #region Convert to Stock Orders Tests

    [Fact]
    public async Task ConvertToStockOrders_WonProposal_CreatesStockOrder()
    {
        // Arrange - Create a won proposal
        var proposalId = await CreateWonProposalAsync();

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequiredDate = DateTime.UtcNow.AddDays(7),
            Notes = "Created from integration test",
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);
        var result = await response.Content.ReadFromJsonAsync<ConversionResultDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.CreatedOrders.Should().NotBeEmpty();
        result.CreatedOrders.First().ItemCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertToStockOrders_NonWonProposal_ReturnsBadRequest()
    {
        // Arrange - Use a draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConvertToStockOrders_CreatedOrderHasCorrectItems()
    {
        // Arrange - Create a won proposal with known items
        var proposalId = await CreateWonProposalAsync();

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequiredDate = DateTime.UtcNow.AddDays(7),
            Notes = "Test order",
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);
        var result = await response.Content.ReadFromJsonAsync<ConversionResultDto>();

        // Verify created stock order
        var orderId = result!.CreatedOrders.First().StockOrderId;
        var orderResponse = await AdminClient.GetAsync($"/api/stock-orders/{orderId}");
        var orderResult = await orderResponse.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        // Assert
        orderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        orderResult.Should().NotBeNull();
        orderResult!.Success.Should().BeTrue();
        var order = orderResult.Data!;
        order.Status.Should().Be("Draft");
        order.SiteId.Should().Be(TestTenantConstants.Sites.MainSite);
        order.SourceLocationId.Should().Be(TestTenantConstants.StockManagement.Locations.MainWarehouse);
        order.Lines.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ConvertToStockOrders_WithoutPermission_Returns403()
    {
        // Arrange - Create a won proposal
        var proposalId = await CreateWonProposalAsync();

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Mode = "AllItems"
        };

        // Act - Operator doesn't have Edit permission
        var response = await OperatorClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConvertToStockOrders_InvalidSite_ReturnsBadRequest()
    {
        // Arrange - Create a won proposal
        var proposalId = await CreateWonProposalAsync();

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = Guid.NewGuid(), // Non-existent site
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);

        // Assert - Should fail due to invalid site
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConvertToStockOrders_InvalidSourceLocation_ReturnsBadRequest()
    {
        // Arrange - Create a won proposal
        var proposalId = await CreateWonProposalAsync();

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = Guid.NewGuid(), // Non-existent location
            Mode = "AllItems"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);

        // Assert - Should fail due to invalid source location
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Selective Conversion Tests

    [Fact]
    public async Task ConvertToStockOrders_SelectedSectionsMode_ConvertsOnlySelectedSections()
    {
        // Arrange - Create a won proposal with multiple sections
        var proposalId = await CreateWonProposalWithMultipleSectionsAsync();

        // Get proposal to find section IDs
        var proposalResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var firstSectionId = proposal!.Sections.First().Id;

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Mode = "SelectedSections",
            SelectedSectionIds = new[] { firstSectionId }
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);
        var result = await response.Content.ReadFromJsonAsync<ConversionResultDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        // Only items from the selected section should be converted
        result.CreatedOrders.Should().NotBeEmpty();
        result.CreatedOrders.First().ItemCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertToStockOrders_SelectedItemsMode_ConvertsOnlySelectedItems()
    {
        // Arrange - Create a won proposal with multiple items
        var proposalId = await CreateWonProposalWithMultipleItemsAsync();

        // Get proposal to find line item IDs
        var proposalResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var firstSection = proposal!.Sections.First();
        var firstItemId = firstSection.LineItems.First().Id;

        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            Mode = "SelectedItems",
            SelectedLineItemIds = new[] { firstItemId }
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);
        var result = await response.Content.ReadFromJsonAsync<ConversionResultDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.CreatedOrders.First().ItemCount.Should().Be(1); // Only 1 item selected
    }

    #endregion

    #region Full Integration Tests

    [Fact]
    public async Task FullConversionWorkflow_ProposalToStockOrder_CompletesSuccessfully()
    {
        // 1. Create proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // 2. Submit proposal
        await SubmitProposalAsync(proposalId);

        // 3. Approve proposal
        await ApproveProposalAsync(proposalId);

        // 4. Mark as won
        await WinProposalAsync(proposalId);

        // 5. Verify can convert (API returns just bool)
        var canConvertResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}/can-convert");
        var canConvert = await canConvertResponse.Content.ReadFromJsonAsync<bool>();
        canConvert.Should().BeTrue();

        // 6. Convert to stock order
        var convertDto = new
        {
            ProposalId = proposalId,
            SiteId = TestTenantConstants.Sites.MainSite,
            SourceLocationId = TestTenantConstants.StockManagement.Locations.MainWarehouse,
            RequiredDate = DateTime.UtcNow.AddDays(7),
            Notes = "Full workflow test",
            Mode = "AllItems"
        };

        var convertResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/convert-to-orders", convertDto);
        var result = await convertResponse.Content.ReadFromJsonAsync<ConversionResultDto>();

        // 7. Verify stock order created
        result!.Success.Should().BeTrue();
        result.CreatedOrders.Should().HaveCount(1);

        // 8. Verify order details
        var orderId = result.CreatedOrders.First().StockOrderId;
        var orderResponse = await AdminClient.GetAsync($"/api/stock-orders/{orderId}");
        var orderResult = await orderResponse.Content.ReadFromJsonAsync<ResultWrapper<StockOrderDto>>();

        orderResult.Should().NotBeNull();
        orderResult!.Success.Should().BeTrue();
        orderResult.Data!.Notes.Should().Contain("Full workflow test");
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateDraftProposalWithItemsAsync()
    {
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Conversion Test Proposal {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow,
            VatRate = 23m
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Test Section",
            SortOrder = 1
        };

        var sectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        sectionResponse.EnsureSuccessStatusCode();
        var section = await sectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add a line item with a product
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            ProductId = TestTenantConstants.StockManagement.Products.HardHat,
            Description = TestTenantConstants.StockManagement.Products.HardHatName,
            Quantity = 10m,
            Unit = "Each",
            UnitCost = TestTenantConstants.StockManagement.Products.HardHatCostPrice,
            UnitPrice = TestTenantConstants.StockManagement.Products.HardHatSellPrice,
            SortOrder = 1
        };

        var itemResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);
        itemResponse.EnsureSuccessStatusCode();

        return proposalId;
    }

    private async Task<Guid> CreateWonProposalAsync()
    {
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);
        await WinProposalAsync(proposalId);
        return proposalId;
    }

    private async Task<Guid> CreateWonProposalWithMultipleItemsAsync()
    {
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // Get the section to add more items
        var proposalResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var sectionId = proposal!.Sections.First().Id;

        // Add another line item
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            ProductId = TestTenantConstants.StockManagement.Products.SafetyVest,
            Description = TestTenantConstants.StockManagement.Products.SafetyVestName,
            Quantity = 20m,
            Unit = "Each",
            UnitCost = TestTenantConstants.StockManagement.Products.SafetyVestCostPrice,
            UnitPrice = TestTenantConstants.StockManagement.Products.SafetyVestSellPrice,
            SortOrder = 2
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);

        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);
        await WinProposalAsync(proposalId);

        return proposalId;
    }

    private async Task<Guid> CreateWonProposalWithMultipleSectionsAsync()
    {
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // Add a second section with items
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Second Section",
            SortOrder = 2
        };

        var sectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        sectionResponse.EnsureSuccessStatusCode();
        var section2 = await sectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var section2Id = section2!.Id;

        // Add item to second section
        var lineItemCommand = new
        {
            ProposalSectionId = section2Id,
            ProductId = TestTenantConstants.StockManagement.Products.PowerDrill,
            Description = TestTenantConstants.StockManagement.Products.PowerDrillName,
            Quantity = 5m,
            Unit = "Each",
            UnitCost = TestTenantConstants.StockManagement.Products.PowerDrillCostPrice,
            UnitPrice = TestTenantConstants.StockManagement.Products.PowerDrillSellPrice,
            SortOrder = 1
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{section2Id}/items", lineItemCommand);

        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);
        await WinProposalAsync(proposalId);

        return proposalId;
    }

    private async Task SubmitProposalAsync(Guid proposalId)
    {
        var submitDto = new { Notes = "Submitting for test" };
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/submit", submitDto);
        response.EnsureSuccessStatusCode();
    }

    private async Task ApproveProposalAsync(Guid proposalId)
    {
        var approveDto = new { Notes = "Approved for test" };
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/approve", approveDto);
        response.EnsureSuccessStatusCode();
    }

    private async Task WinProposalAsync(Guid proposalId)
    {
        var winDto = new { Reason = "Client accepted" };
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/win", winDto);
        response.EnsureSuccessStatusCode();
    }

    private async Task LoseProposalAsync(Guid proposalId, string reason)
    {
        var loseDto = new { Reason = reason };
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/lose", loseDto);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(bool Success, T? Data, string? Message = null);

    // API returns ConversionPreviewDto
    private record ConversionPreviewDto(
        int TotalItems,
        decimal TotalQuantity,
        decimal TotalValue,
        bool HasStockWarnings,
        bool HasAdHocItems,
        List<ConversionPreviewItemDto> Items
    );

    // ConversionPreviewItemDto
    private record ConversionPreviewItemDto(
        Guid? ProductId,
        string? ProductCode,
        string Description,
        decimal Quantity,
        decimal AvailableStock,
        bool HasSufficientStock,
        bool IsAdHocItem,
        decimal UnitPrice,
        decimal LineTotal
    );

    // API returns ConversionResultDto
    private record ConversionResultDto(
        bool Success,
        List<CreatedStockOrderDto> CreatedOrders,
        List<string> Warnings,
        string? ErrorMessage
    );

    // CreatedStockOrderDto
    private record CreatedStockOrderDto(
        Guid StockOrderId,
        string OrderNumber,
        int ItemCount,
        decimal TotalValue
    );

    private record ProposalDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        string ProjectName,
        string Status,
        List<ProposalSectionDto> Sections,
        DateTime CreatedAt
    );

    private record ProposalSectionDto(
        Guid Id,
        Guid ProposalId,
        string SectionName,
        int SortOrder,
        List<ProposalLineItemDto> LineItems
    );

    private record ProposalLineItemDto(
        Guid Id,
        Guid ProposalSectionId,
        Guid? ProductId,
        string Description,
        decimal Quantity,
        decimal UnitPrice,
        decimal LineTotal
    );

    private record StockOrderDto(
        Guid Id,
        string OrderNumber,
        Guid SiteId,
        string SiteName,
        Guid SourceLocationId,
        string Status,
        string? Notes,
        List<StockOrderLineDto> Lines,
        DateTime CreatedAt
    );

    private record StockOrderLineDto(
        Guid Id,
        Guid ProductId,
        string ProductName,
        decimal QuantityRequested
    );

    #endregion
}
