using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Proposals;

/// <summary>
/// Integration tests for Proposal CRUD operations.
/// </summary>
public class ProposalCrudTests : IntegrationTestBase
{
    public ProposalCrudTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Get Proposals Tests

    [Fact]
    public async Task GetProposals_ReturnsPagedResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PaginatedResult<ProposalListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetProposals_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals?status=Draft");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PaginatedResult<ProposalListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Items.Should().OnlyContain(p => p.Status == "Draft");
    }

    [Fact]
    public async Task GetProposals_WithCompanyFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await AdminClient.GetAsync($"/api/proposals?companyId={TestTenantConstants.Companies.CustomerCompany1}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PaginatedResult<ProposalListDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProposalById_ExistingProposal_ReturnsProposalWithSections()
    {
        // Arrange
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.DraftProposal;

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Id.Should().Be(proposalId);
        result.ProposalNumber.Should().Be(TestTenantConstants.Proposals.ProposalRecords.DraftProposalReference);
        result.Status.Should().Be("Draft");
        result.Sections.Should().NotBeNull();
        result.Contacts.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProposalById_NonExistingProposal_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProposalSummary_ReturnsAggregatedData()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/summary");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ProposalSummaryDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    #endregion

    #region Create Proposal Tests

    [Fact]
    public async Task CreateProposal_ValidData_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            PrimaryContactId = TestTenantConstants.Contacts.Contact1,
            ProjectName = $"Test Project {Guid.NewGuid():N}",
            ProjectAddress = "123 Test Street, Dublin",
            ProjectDescription = "Integration test project",
            ProposalDate = DateTime.UtcNow,
            ValidUntilDate = DateTime.UtcNow.AddDays(30),
            Currency = "EUR",
            VatRate = 23m,
            DiscountPercent = 0m,
            PaymentTerms = "Net 30",
            Notes = "Created by integration test"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/proposals", command);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.ProjectName.Should().Be(command.ProjectName);
        result.CompanyId.Should().Be(command.CompanyId);
        result.Status.Should().Be("Draft");
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task CreateProposal_MinimalData_ReturnsCreated()
    {
        // Arrange - Only required fields
        var command = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Minimal Proposal {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync("/api/proposals", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateProposal_Unauthenticated_Returns401()
    {
        // Arrange
        var command = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = "Unauthenticated Test",
            ProposalDate = DateTime.UtcNow
        };

        // Act
        var response = await UnauthenticatedClient.PostAsJsonAsync("/api/proposals", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProposal_WithoutPermission_Returns403()
    {
        // Arrange
        var command = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = "No Permission Test",
            ProposalDate = DateTime.UtcNow
        };

        // Act - Warehouse user doesn't have Proposals.Create permission
        var response = await WarehouseClient.PostAsJsonAsync("/api/proposals", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Update Proposal Tests

    [Fact]
    public async Task UpdateProposal_ValidData_ReturnsOk()
    {
        // Arrange - First create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Original Proposal {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Update the proposal
        var updateCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = "Updated Project Name",
            ProjectAddress = "456 Updated Street",
            ProjectDescription = "Updated description",
            ProposalDate = DateTime.UtcNow,
            ValidUntilDate = DateTime.UtcNow.AddDays(60),
            Currency = "EUR",
            VatRate = 23m,
            PaymentTerms = "Net 60"
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/proposals/{proposalId}", updateCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("Updated Project Name");
        result.ProjectAddress.Should().Be("456 Updated Street");
        result.PaymentTerms.Should().Be("Net 60");
    }

    [Fact]
    public async Task UpdateProposal_NonDraftStatus_ReturnsBadRequest()
    {
        // Arrange - Use approved proposal (can't edit non-draft)
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        var updateCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = "Trying to update approved",
            ProposalDate = DateTime.UtcNow
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/proposals/{proposalId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Delete Proposal Tests

    [Fact]
    public async Task DeleteProposal_DraftProposal_ReturnsNoContent()
    {
        // Arrange - Create a new proposal to delete
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal to Delete {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Act
        var response = await AdminClient.DeleteAsync($"/api/proposals/{proposalId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProposal_NonDraftStatus_ReturnsBadRequest()
    {
        // Arrange - Use approved proposal
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act
        var response = await AdminClient.DeleteAsync($"/api/proposals/{proposalId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Section Tests

    [Fact]
    public async Task AddSection_ToProposal_ReturnsOk()
    {
        // Arrange - Create a proposal first
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal with Section {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "New Section",
            Description = "Section added by integration test",
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalSectionDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.SectionName.Should().Be("New Section");
        result.ProposalId.Should().Be(proposalId);
    }

    [Fact]
    public async Task UpdateSection_ValidData_ReturnsOk()
    {
        // Arrange - Create a proposal with a section
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal for Section Update {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Original Section",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Update the section
        var updateSectionCommand = new
        {
            SectionName = "Updated Section Name",
            Description = "Updated description",
            SortOrder = 2
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/proposals/sections/{sectionId}", updateSectionCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalSectionDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.SectionName.Should().Be("Updated Section Name");
    }

    [Fact]
    public async Task DeleteSection_ValidSection_ReturnsNoContent()
    {
        // Arrange - Create a proposal with a section
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal for Section Delete {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Section to Delete",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Act
        var response = await AdminClient.DeleteAsync($"/api/proposals/sections/{sectionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Line Item Tests

    [Fact]
    public async Task AddLineItem_ToSection_ReturnsOkAndCalculatesTotals()
    {
        // Arrange - Create a proposal with a section
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal with Line Item {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Test Section",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add a line item
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

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalLineItemDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Description.Should().Be(TestTenantConstants.StockManagement.Products.HardHatName);
        result.Quantity.Should().Be(10);
        result.LineTotal.Should().Be(250); // 10 * 25 = 250

        // Verify proposal totals updated
        var updatedProposal = await AdminClient.GetAsync($"/api/proposals/{proposalId}?includeCosting=true");
        var proposalResult = await updatedProposal.Content.ReadFromJsonAsync<ProposalDto>();
        proposalResult!.Subtotal.Should().Be(250);
    }

    [Fact]
    public async Task AddLineItem_WithoutProduct_AdHocItem_ReturnsOk()
    {
        // Arrange - Create a proposal with a section
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal with Ad-hoc Item {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Test Section",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add ad-hoc line item (no ProductId)
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            Description = "Custom Labor Charge",
            Quantity = 8m,
            Unit = "Hours",
            UnitCost = 35m,
            UnitPrice = 50m,
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalLineItemDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.ProductId.Should().BeNull();
        result.Description.Should().Be("Custom Labor Charge");
        result.LineTotal.Should().Be(400); // 8 * 50 = 400
    }

    [Fact]
    public async Task UpdateLineItem_ValidData_ReturnsOk()
    {
        // Arrange - Create a proposal with a section and line item
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal for Line Item Update {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Test Section",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add a line item
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            Description = "Original Item",
            Quantity = 5m,
            Unit = "Each",
            UnitCost = 10m,
            UnitPrice = 15m,
            SortOrder = 1
        };

        var addItemResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);
        addItemResponse.EnsureSuccessStatusCode();
        var lineItem = await addItemResponse.Content.ReadFromJsonAsync<ProposalLineItemDto>();
        var itemId = lineItem!.Id;

        // Update the line item
        var updateItemCommand = new
        {
            Description = "Updated Item Description",
            Quantity = 20m,
            Unit = "Each",
            UnitCost = 12m,
            UnitPrice = 18m,
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/proposals/items/{itemId}", updateItemCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalLineItemDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Description.Should().Be("Updated Item Description");
        result.Quantity.Should().Be(20);
        result.LineTotal.Should().Be(360); // 20 * 18 = 360
    }

    [Fact]
    public async Task DeleteLineItem_ValidItem_ReturnsNoContent()
    {
        // Arrange - Create a proposal with a section and line item
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal for Line Item Delete {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Test Section",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add a line item
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            Description = "Item to Delete",
            Quantity = 1m,
            Unit = "Each",
            UnitCost = 10m,
            UnitPrice = 15m,
            SortOrder = 1
        };

        var addItemResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);
        addItemResponse.EnsureSuccessStatusCode();
        var lineItem = await addItemResponse.Content.ReadFromJsonAsync<ProposalLineItemDto>();
        var itemId = lineItem!.Id;

        // Act
        var response = await AdminClient.DeleteAsync($"/api/proposals/items/{itemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Contact Tests

    [Fact]
    public async Task AddContact_ToProposal_ReturnsOk()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal with Contact {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        var contactCommand = new
        {
            ProposalId = proposalId,
            ContactId = TestTenantConstants.Contacts.Contact1,
            ContactName = "Contact One",
            Email = TestTenantConstants.Contacts.Contact1Email,
            Phone = "+353 87 123 4567",
            Role = "Decision Maker",
            IsPrimary = true
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/contacts", contactCommand);
        var result = await response.Content.ReadFromJsonAsync<ProposalContactDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.ContactName.Should().Be("Contact One");
        result.Role.Should().Be("Decision Maker");
        result.IsPrimary.Should().BeTrue();
    }

    #endregion

    #region Recalculate Tests

    [Fact]
    public async Task RecalculateProposal_UpdatesTotals()
    {
        // Arrange - Create a proposal with items
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Proposal for Recalculate {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow,
            VatRate = 23m
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        // Add a section
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Test Section",
            SortOrder = 1
        };

        var addSectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        addSectionResponse.EnsureSuccessStatusCode();
        var section = await addSectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add line items
        var lineItem1 = new
        {
            ProposalSectionId = sectionId,
            Description = "Item 1",
            Quantity = 10m,
            Unit = "Each",
            UnitCost = 10m,
            UnitPrice = 20m,
            SortOrder = 1
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItem1);

        var lineItem2 = new
        {
            ProposalSectionId = sectionId,
            Description = "Item 2",
            Quantity = 5m,
            Unit = "Each",
            UnitCost = 20m,
            UnitPrice = 40m,
            SortOrder = 2
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItem2);

        // Act - Recalculate
        var response = await AdminClient.PostAsync($"/api/proposals/{proposalId}/recalculate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify updated totals
        var updatedProposal = await AdminClient.GetAsync($"/api/proposals/{proposalId}?includeCosting=true");
        var result = await updatedProposal.Content.ReadFromJsonAsync<ProposalDto>();

        // Subtotal = (10 * 20) + (5 * 40) = 200 + 200 = 400
        result!.Subtotal.Should().Be(400);
        // VAT = 400 * 0.23 = 92
        result.VatAmount.Should().Be(92);
        // Grand Total = 400 + 92 = 492
        result.GrandTotal.Should().Be(492);
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

    private record ProposalListDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        string ProjectName,
        string CompanyName,
        DateTime ProposalDate,
        DateTime? ValidUntilDate,
        string Status,
        decimal GrandTotal,
        string Currency,
        decimal? MarginPercent,
        DateTime CreatedAt
    );

    private record ProposalDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        Guid? ParentProposalId,
        Guid CompanyId,
        string CompanyName,
        Guid? PrimaryContactId,
        string? PrimaryContactName,
        string ProjectName,
        string? ProjectAddress,
        string? ProjectDescription,
        DateTime ProposalDate,
        DateTime? ValidUntilDate,
        DateTime? SubmittedDate,
        DateTime? ApprovedDate,
        string? ApprovedBy,
        DateTime? WonDate,
        DateTime? LostDate,
        string Status,
        string? WonLostReason,
        string Currency,
        decimal Subtotal,
        decimal DiscountPercent,
        decimal DiscountAmount,
        decimal NetTotal,
        decimal VatRate,
        decimal VatAmount,
        decimal GrandTotal,
        decimal? TotalCost,
        decimal? TotalMargin,
        decimal? MarginPercent,
        string? PaymentTerms,
        string? TermsAndConditions,
        string? Notes,
        string? DrawingFileName,
        string? DrawingUrl,
        List<ProposalSectionDto> Sections,
        List<ProposalContactDto> Contacts,
        DateTime CreatedAt,
        string CreatedBy,
        DateTime? UpdatedAt
    );

    private record ProposalSectionDto(
        Guid Id,
        Guid ProposalId,
        Guid? SourceKitId,
        string? SourceKitName,
        string SectionName,
        string? Description,
        int SortOrder,
        decimal SectionCost,
        decimal SectionTotal,
        decimal SectionMargin,
        List<ProposalLineItemDto> LineItems
    );

    private record ProposalLineItemDto(
        Guid Id,
        Guid ProposalSectionId,
        Guid? ProductId,
        string Description,
        decimal Quantity,
        string Unit,
        decimal UnitPrice,
        decimal UnitCost,
        decimal LineTotal,
        decimal LineCost,
        decimal LineMargin,
        decimal MarginPercent,
        int SortOrder,
        string? Notes
    );

    private record ProposalContactDto(
        Guid Id,
        Guid ProposalId,
        Guid? ContactId,
        string ContactName,
        string? Email,
        string? Phone,
        string Role,
        bool IsPrimary
    );

    private record ProposalSummaryDto(
        int TotalCount,
        decimal PipelineValue,
        int WonThisMonth,
        decimal ConversionRate
    );

    #endregion
}
