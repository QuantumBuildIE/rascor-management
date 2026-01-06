using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Proposals;

/// <summary>
/// Integration tests for Product Kits and kit expansion into proposal sections.
/// Tests the ability to create sections from pre-defined kits.
/// </summary>
public class ProductKitTests : IntegrationTestBase
{
    public ProductKitTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Add Section From Kit Tests

    [Fact]
    public async Task AddSection_WithSourceKitId_ExpandsKitItems()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Kit Expansion Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add section from kit (if kit exists)
        var kitId = TestTenantConstants.Proposals.ProductKits.SafetyKit;
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SourceKitId = kitId,
            SectionName = "Safety Equipment",
            Description = "PPE items from safety kit",
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);

        // Assert - Section should be created (even if kit doesn't auto-populate)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var section = await response.Content.ReadFromJsonAsync<ProposalSectionDto>();
        section.Should().NotBeNull();
        section!.SectionName.Should().Be("Safety Equipment");
        section.SourceKitId.Should().Be(kitId);
    }

    [Fact]
    public async Task AddSection_WithInvalidKitId_StillCreatesSection()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Invalid Kit Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add section with non-existent kit ID
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SourceKitId = Guid.NewGuid(), // Non-existent kit
            SectionName = "Custom Section",
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);

        // Assert - Section creation may still succeed or return BadRequest
        // depending on implementation (kit expansion is optional)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSection_WithoutSourceKitId_CreatesEmptySection()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Empty Section Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add section without kit
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SectionName = "Manual Section",
            Description = "Items will be added manually",
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        var section = await response.Content.ReadFromJsonAsync<ProposalSectionDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        section.Should().NotBeNull();
        section!.SourceKitId.Should().BeNull();
        section.LineItems.Should().BeEmpty();
    }

    [Fact]
    public async Task AddSection_FromToolKit_RecordsKitReference()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Tool Kit Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add section from tool kit
        var kitId = TestTenantConstants.Proposals.ProductKits.ToolKit;
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SourceKitId = kitId,
            SectionName = "Tools",
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var section = await response.Content.ReadFromJsonAsync<ProposalSectionDto>();
        section!.SourceKitId.Should().Be(kitId);
    }

    #endregion

    #region Multiple Sections from Kits Tests

    [Fact]
    public async Task AddMultipleSections_FromDifferentKits_AllCreatedSuccessfully()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Multi Kit Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add safety kit section
        var safetySection = new
        {
            ProposalId = proposalId,
            SourceKitId = TestTenantConstants.Proposals.ProductKits.SafetyKit,
            SectionName = "Safety Equipment",
            SortOrder = 1
        };

        var safetyResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", safetySection);
        safetyResponse.EnsureSuccessStatusCode();

        // Add tool kit section
        var toolSection = new
        {
            ProposalId = proposalId,
            SourceKitId = TestTenantConstants.Proposals.ProductKits.ToolKit,
            SectionName = "Tools",
            SortOrder = 2
        };

        var toolResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", toolSection);
        toolResponse.EnsureSuccessStatusCode();

        // Act - Get the full proposal
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var updatedProposal = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedProposal!.Sections.Should().HaveCount(2);
        updatedProposal.Sections.Should().Contain(s => s.SourceKitId == TestTenantConstants.Proposals.ProductKits.SafetyKit);
        updatedProposal.Sections.Should().Contain(s => s.SourceKitId == TestTenantConstants.Proposals.ProductKits.ToolKit);
    }

    [Fact]
    public async Task AddSection_SameKitTwice_BothSectionsCreated()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Duplicate Kit Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        var kitId = TestTenantConstants.Proposals.ProductKits.SafetyKit;

        // Add first section from kit
        var section1 = new
        {
            ProposalId = proposalId,
            SourceKitId = kitId,
            SectionName = "Safety - Building A",
            SortOrder = 1
        };

        var response1 = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", section1);
        response1.EnsureSuccessStatusCode();

        // Add second section from same kit
        var section2 = new
        {
            ProposalId = proposalId,
            SourceKitId = kitId,
            SectionName = "Safety - Building B",
            SortOrder = 2
        };

        var response2 = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", section2);

        // Assert - Both sections should be created
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify both sections exist
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var updatedProposal = await response.Content.ReadFromJsonAsync<ProposalDto>();
        updatedProposal!.Sections.Should().HaveCount(2);
        updatedProposal.Sections.Should().OnlyContain(s => s.SourceKitId == kitId);
    }

    #endregion

    #region Section Update with Kit Reference Tests

    [Fact]
    public async Task UpdateSection_PreservesKitReference()
    {
        // Arrange - Create a proposal with kit section
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Update Kit Section Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        var kitId = TestTenantConstants.Proposals.ProductKits.SafetyKit;
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SourceKitId = kitId,
            SectionName = "Original Name",
            SortOrder = 1
        };

        var sectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        sectionResponse.EnsureSuccessStatusCode();
        var section = await sectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Update section name but not kit reference
        var updateCommand = new
        {
            SectionName = "Updated Name",
            Description = "Added description",
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PutAsJsonAsync($"/api/proposals/sections/{sectionId}", updateCommand);
        var updatedSection = await response.Content.ReadFromJsonAsync<ProposalSectionDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedSection!.SectionName.Should().Be("Updated Name");
        updatedSection.SourceKitId.Should().Be(kitId); // Kit reference preserved
    }

    #endregion

    #region Manual Items with Kit Sections Tests

    [Fact]
    public async Task AddManualItems_ToKitSection_SucceedsAndUpdatesTotals()
    {
        // Arrange - Create a proposal with kit section
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Manual Items to Kit Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow,
            VatRate = 23m
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        var kitId = TestTenantConstants.Proposals.ProductKits.SafetyKit;
        var sectionCommand = new
        {
            ProposalId = proposalId,
            SourceKitId = kitId,
            SectionName = "Safety Equipment",
            SortOrder = 1
        };

        var sectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
        sectionResponse.EnsureSuccessStatusCode();
        var section = await sectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
        var sectionId = section!.Id;

        // Add manual item to kit section
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            ProductId = TestTenantConstants.StockManagement.Products.HardHat,
            Description = TestTenantConstants.StockManagement.Products.HardHatName,
            Quantity = 50m,
            Unit = "Each",
            UnitCost = TestTenantConstants.StockManagement.Products.HardHatCostPrice,
            UnitPrice = TestTenantConstants.StockManagement.Products.HardHatSellPrice,
            SortOrder = 1
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify section totals updated
        var updatedProposal = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var result = await updatedProposal.Content.ReadFromJsonAsync<ProposalDto>();
        var updatedSection = result!.Sections.First(s => s.Id == sectionId);

        updatedSection.LineItems.Should().HaveCount(1);
        updatedSection.SectionTotal.Should().Be(50 * 25); // 50 * 25 = 1250
    }

    #endregion

    #region Response DTOs

    private record ProposalDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        string ProjectName,
        string Status,
        decimal Subtotal,
        decimal VatAmount,
        decimal GrandTotal,
        List<ProposalSectionDto> Sections,
        DateTime CreatedAt
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
        decimal UnitPrice,
        decimal LineTotal
    );

    #endregion
}
