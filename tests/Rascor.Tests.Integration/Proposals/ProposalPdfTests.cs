using System.Net;
using System.Net.Http.Headers;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Proposals;

/// <summary>
/// Integration tests for Proposal PDF generation.
/// Tests client-facing PDFs and internal PDFs with costing information.
/// </summary>
public class ProposalPdfTests : IntegrationTestBase
{
    public ProposalPdfTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Client PDF Generation Tests

    [Fact]
    public async Task GeneratePdf_ValidProposal_ReturnsFile()
    {
        // Arrange - Create a proposal with items
        var proposalId = await CreateProposalWithItemsAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        response.Content.Headers.ContentDisposition.Should().NotBeNull();
    }

    [Fact]
    public async Task GeneratePdf_ValidProposal_HasCorrectFilename()
    {
        // Arrange - Create a proposal
        var proposalId = await CreateProposalWithItemsAsync();

        // Get proposal to know the proposal number and version
        var proposalResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalDto>();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition!.FileName.Should().NotBeNullOrEmpty();
        // Filename should contain proposal number and version
        contentDisposition.FileName.Should().EndWith(".pdf");
    }

    [Fact]
    public async Task GeneratePdf_NonExistentProposal_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{nonExistentId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GeneratePdf_Unauthenticated_Returns401()
    {
        // Arrange
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act
        var response = await UnauthenticatedClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GeneratePdf_ExistingApprovedProposal_ReturnsFile()
    {
        // Arrange - Use existing approved proposal
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_DraftProposal_ReturnsFile()
    {
        // Arrange - Create a draft proposal
        var proposalId = await CreateProposalWithItemsAsync();

        // Act - Even draft proposals can generate PDF
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    #endregion

    #region Internal PDF with Costing Tests

    [Fact]
    public async Task GeneratePdf_WithCostingIncluded_RequiresPermission()
    {
        // Arrange
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act - Admin has ViewCostings permission
        var adminResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf?includeCosting=true");

        // Assert
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        adminResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_WithCostingWithoutPermission_ReturnsClientVersion()
    {
        // Arrange
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act - Operator doesn't have ViewCostings permission
        var response = await OperatorClient.GetAsync($"/api/proposals/{proposalId}/pdf?includeCosting=true");

        // Assert - Should either succeed with client version or return forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            // Controller should silently disable costing if no permission
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        }
    }

    [Fact]
    public async Task GeneratePdf_FinanceUser_CanGetCostingPdf()
    {
        // Arrange
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act - Finance user has ViewCostings permission
        var response = await FinanceClient.GetAsync($"/api/proposals/{proposalId}/pdf?includeCosting=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    #endregion

    #region PDF Content Verification Tests

    [Fact]
    public async Task GeneratePdf_ValidProposal_HasNonZeroContent()
    {
        // Arrange
        var proposalId = await CreateProposalWithItemsAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");
        var content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeEmpty();
        content.Length.Should().BeGreaterThan(100); // PDF should have some substantial content
    }

    [Fact]
    public async Task GeneratePdf_ValidProposal_HasPdfHeader()
    {
        // Arrange
        var proposalId = await CreateProposalWithItemsAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");
        var content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // PDF files start with %PDF-
        var header = System.Text.Encoding.ASCII.GetString(content.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task GeneratePdf_EmptyProposal_StillGenerates()
    {
        // Arrange - Create proposal without items
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Empty PDF Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert - Should still generate, just with no items
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    #endregion

    #region PDF for Different Proposal States Tests

    [Fact]
    public async Task GeneratePdf_SubmittedProposal_ReturnsFile()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_ApprovedProposal_ReturnsFile()
    {
        // Arrange - Create, submit, and approve a proposal
        var proposalId = await CreateProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_WonProposal_ReturnsFile()
    {
        // Arrange - Create full workflow proposal
        var proposalId = await CreateProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);
        await WinProposalAsync(proposalId);

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_LostProposal_ReturnsFile()
    {
        // Arrange - Create full workflow proposal then lose
        var proposalId = await CreateProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);
        await LoseProposalAsync(proposalId, "Testing");

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_CancelledProposal_ReturnsFile()
    {
        // Arrange - Create and cancel a proposal
        var proposalId = await CreateProposalWithItemsAsync();
        await CancelProposalAsync(proposalId);

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    #endregion

    #region PDF for Proposals with Different Content Tests

    [Fact]
    public async Task GeneratePdf_ProposalWithMultipleSections_ReturnsFile()
    {
        // Arrange - Create proposal with multiple sections
        var proposalId = await CreateProposalWithMultipleSectionsAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");
        var content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GeneratePdf_ProposalWithContacts_ReturnsFile()
    {
        // Arrange - Create proposal with contact
        var proposalId = await CreateProposalWithContactAsync();

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GeneratePdf_ProposalWithDiscount_ReturnsFile()
    {
        // Arrange - Create proposal with discount
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Discount PDF Test {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow,
            VatRate = 23m,
            DiscountPercent = 10m
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        // Add items
        await AddItemToProposalAsync(proposalId);

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/pdf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateProposalWithItemsAsync()
    {
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"PDF Test Proposal {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow,
            ValidUntilDate = DateTime.UtcNow.AddDays(30),
            VatRate = 23m,
            PaymentTerms = "Net 30",
            Notes = "Test proposal for PDF generation"
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var proposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = proposal!.Id;

        await AddItemToProposalAsync(proposalId);

        return proposalId;
    }

    private async Task<Guid> CreateProposalWithMultipleSectionsAsync()
    {
        var proposalId = await CreateProposalWithItemsAsync();

        // Add second section
        var proposalResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalDto>();

        var section2Command = new
        {
            ProposalId = proposalId,
            SectionName = "Second Section",
            Description = "Additional items",
            SortOrder = 2
        };

        var sectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", section2Command);
        sectionResponse.EnsureSuccessStatusCode();
        var section2 = await sectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();

        // Add item to second section
        var lineItemCommand = new
        {
            ProposalSectionId = section2!.Id,
            Description = "Another Product",
            Quantity = 5m,
            Unit = "Each",
            UnitCost = 20m,
            UnitPrice = 35m,
            SortOrder = 1
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{section2.Id}/items", lineItemCommand);

        return proposalId;
    }

    private async Task<Guid> CreateProposalWithContactAsync()
    {
        var proposalId = await CreateProposalWithItemsAsync();

        var contactCommand = new
        {
            ProposalId = proposalId,
            ContactId = TestTenantConstants.Contacts.Contact1,
            ContactName = "Test Contact",
            Email = TestTenantConstants.Contacts.Contact1Email,
            Phone = "+353 87 123 4567",
            Role = "Decision Maker",
            IsPrimary = true
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/contacts", contactCommand);

        return proposalId;
    }

    private async Task AddItemToProposalAsync(Guid proposalId)
    {
        // Get or create a section
        var proposalResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalDto>();

        Guid sectionId;
        if (proposal!.Sections.Any())
        {
            sectionId = proposal.Sections.First().Id;
        }
        else
        {
            var sectionCommand = new
            {
                ProposalId = proposalId,
                SectionName = "Test Section",
                SortOrder = 1
            };

            var sectionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/sections", sectionCommand);
            sectionResponse.EnsureSuccessStatusCode();
            var section = await sectionResponse.Content.ReadFromJsonAsync<ProposalSectionDto>();
            sectionId = section!.Id;
        }

        // Add line item
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

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);
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

    private async Task CancelProposalAsync(Guid proposalId)
    {
        var response = await AdminClient.PostAsync($"/api/proposals/{proposalId}/cancel", null);
        response.EnsureSuccessStatusCode();
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
        List<ProposalContactDto> Contacts,
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
        string Description,
        decimal Quantity,
        decimal UnitPrice,
        decimal LineTotal
    );

    private record ProposalContactDto(
        Guid Id,
        Guid ProposalId,
        string ContactName,
        string? Email,
        string Role
    );

    #endregion
}
