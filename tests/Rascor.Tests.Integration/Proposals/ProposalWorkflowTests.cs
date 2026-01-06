using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Proposals;

/// <summary>
/// Integration tests for Proposal workflow state transitions.
/// Tests the full lifecycle: Draft -> Submitted -> Approved -> Won/Lost
/// </summary>
public class ProposalWorkflowTests : IntegrationTestBase
{
    public ProposalWorkflowTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Submit Workflow Tests

    [Fact]
    public async Task SubmitProposal_FromDraft_ChangesStatusToSubmitted()
    {
        // Arrange - Create a new proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        var submitDto = new
        {
            Notes = "Submitting for review"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/submit", submitDto);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Submitted");
        result.SubmittedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitProposal_EmptyProposal_ReturnsBadRequest()
    {
        // Arrange - Create proposal without any items
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Empty Proposal {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow
        };

        var createResponse = await AdminClient.PostAsJsonAsync("/api/proposals", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalDto>();
        var proposalId = createdProposal!.Id;

        var submitDto = new { Notes = "Trying to submit empty proposal" };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/submit", submitDto);

        // Assert - Should fail because proposal has no items
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitProposal_AlreadySubmitted_ReturnsBadRequest()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        var submitDto = new { Notes = "First submission" };
        var firstSubmit = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/submit", submitDto);
        firstSubmit.EnsureSuccessStatusCode();

        // Act - Try to submit again
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/submit", submitDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitProposal_WithoutPermission_Returns403()
    {
        // Arrange - Create a proposal first
        var proposalId = await CreateDraftProposalWithItemsAsync();
        var submitDto = new { Notes = "Trying to submit" };

        // Act - Warehouse user doesn't have Submit permission
        var response = await WarehouseClient.PostAsJsonAsync($"/api/proposals/{proposalId}/submit", submitDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Approve Workflow Tests

    [Fact]
    public async Task ApproveProposal_FromSubmitted_ChangesStatusToApproved()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);

        var approveDto = new
        {
            Notes = "Approved for client presentation"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/approve", approveDto);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Approved");
        result.ApprovedDate.Should().NotBeNull();
        result.ApprovedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ApproveProposal_FromDraft_ReturnsBadRequest()
    {
        // Arrange - Create a draft proposal (not submitted)
        var proposalId = await CreateDraftProposalWithItemsAsync();
        var approveDto = new { Notes = "Trying to approve draft" };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/approve", approveDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApproveProposal_WithoutPermission_Returns403()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);

        var approveDto = new { Notes = "Trying to approve" };

        // Act - SiteManager doesn't have Approve permission
        var response = await SiteManagerClient.PostAsJsonAsync($"/api/proposals/{proposalId}/approve", approveDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Reject Workflow Tests

    [Fact]
    public async Task RejectProposal_FromSubmitted_ChangesStatusToRejected()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);

        var rejectDto = new
        {
            Reason = "Pricing too high, please revise"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/reject", rejectDto);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Rejected");
        result.WonLostReason.Should().Be("Pricing too high, please revise");
    }

    [Fact]
    public async Task RejectProposal_MissingReason_ReturnsBadRequest()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);

        var rejectDto = new { Reason = "" }; // Empty reason

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/reject", rejectDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Win Workflow Tests

    [Fact]
    public async Task WinProposal_FromApproved_ChangesStatusToWon()
    {
        // Arrange - Create, submit, and approve a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        var winDto = new
        {
            Reason = "Client accepted our proposal",
            WonDate = DateTime.UtcNow
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/win", winDto);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Won");
        result.WonDate.Should().NotBeNull();
        result.WonLostReason.Should().Be("Client accepted our proposal");
    }

    [Fact]
    public async Task WinProposal_FromDraft_ReturnsBadRequest()
    {
        // Arrange - Create a draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        var winDto = new { Reason = "Trying to win draft" };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/win", winDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WinProposal_AlreadyWon_ReturnsBadRequest()
    {
        // Arrange - Use existing won proposal
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.WonProposal;
        var winDto = new { Reason = "Trying to win again" };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/win", winDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Lose Workflow Tests

    [Fact]
    public async Task LoseProposal_FromApproved_ChangesStatusToLost()
    {
        // Arrange - Create, submit, and approve a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        var loseDto = new
        {
            Reason = "Competitor offered lower price",
            LostDate = DateTime.UtcNow
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/lose", loseDto);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Lost");
        result.LostDate.Should().NotBeNull();
        result.WonLostReason.Should().Be("Competitor offered lower price");
    }

    [Fact]
    public async Task LoseProposal_MissingReason_ReturnsBadRequest()
    {
        // Arrange - Create, submit, and approve a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        var loseDto = new { Reason = "" }; // Empty reason

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/lose", loseDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Cancel Workflow Tests

    [Fact]
    public async Task CancelProposal_FromDraft_ChangesStatusToCancelled()
    {
        // Arrange - Create a draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // Act
        var response = await AdminClient.PostAsync($"/api/proposals/{proposalId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelProposal_FromSubmitted_ChangesStatusToCancelled()
    {
        // Arrange - Create and submit a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);

        // Act
        var response = await AdminClient.PostAsync($"/api/proposals/{proposalId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelProposal_FromApproved_ChangesStatusToCancelled()
    {
        // Arrange - Create, submit, and approve a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        // Act
        var response = await AdminClient.PostAsync($"/api/proposals/{proposalId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelProposal_AlreadyWon_ReturnsBadRequest()
    {
        // Arrange - Use existing won proposal
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.WonProposal;

        // Act
        var response = await AdminClient.PostAsync($"/api/proposals/{proposalId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Revision Workflow Tests

    [Fact]
    public async Task CreateRevision_CopiesProposalWithNewVersion()
    {
        // Arrange - Create, submit, and approve a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        var revisionDto = new
        {
            Notes = "Creating revision based on client feedback"
        };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/revise", revisionDto);
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Version.Should().Be(2); // New version
        result.Status.Should().Be("Draft"); // Back to draft
        result.ParentProposalId.Should().Be(proposalId); // Links to original
    }

    [Fact]
    public async Task CreateRevision_FromDraft_ReturnsBadRequest()
    {
        // Arrange - Create a draft proposal (cannot create revision from draft)
        var proposalId = await CreateDraftProposalWithItemsAsync();
        var revisionDto = new { Notes = "Trying to revise draft" };

        // Act
        var response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/revise", revisionDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRevisions_ReturnsAllVersions()
    {
        // Arrange - Create a proposal and create a revision
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        // Create revision
        var revisionDto = new { Notes = "First revision" };
        var revisionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/revise", revisionDto);
        revisionResponse.EnsureSuccessStatusCode();

        // Act - Get revisions of original proposal
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}/revisions");
        var revisions = await response.Content.ReadFromJsonAsync<List<ProposalListDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        revisions.Should().NotBeNull();
        revisions!.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task CreateMultipleRevisions_IncreasesVersionNumber()
    {
        // Arrange - Create a proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();
        await SubmitProposalAsync(proposalId);
        await ApproveProposalAsync(proposalId);

        // Create first revision
        var revision1Response = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/revise", new { Notes = "Revision 1" });
        revision1Response.EnsureSuccessStatusCode();
        var revision1 = await revision1Response.Content.ReadFromJsonAsync<ProposalDto>();
        var revision1Id = revision1!.Id;

        // Submit and approve revision 1
        await SubmitProposalAsync(revision1Id);
        await ApproveProposalAsync(revision1Id);

        // Create second revision from revision 1
        var revision2Response = await AdminClient.PostAsJsonAsync($"/api/proposals/{revision1Id}/revise", new { Notes = "Revision 2" });
        revision2Response.EnsureSuccessStatusCode();
        var revision2 = await revision2Response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        revision1!.Version.Should().Be(2);
        revision2!.Version.Should().Be(3);
    }

    #endregion

    #region Full Workflow Integration Tests

    [Fact]
    public async Task FullWorkflow_DraftToWon_CompletesSuccessfully()
    {
        // 1. Create draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // 2. Verify draft status
        var draftProposal = await GetProposalAsync(proposalId);
        draftProposal.Status.Should().Be("Draft");

        // 3. Submit proposal
        await SubmitProposalAsync(proposalId);
        var submittedProposal = await GetProposalAsync(proposalId);
        submittedProposal.Status.Should().Be("Submitted");

        // 4. Approve proposal
        await ApproveProposalAsync(proposalId);
        var approvedProposal = await GetProposalAsync(proposalId);
        approvedProposal.Status.Should().Be("Approved");

        // 5. Mark as won
        var winDto = new { Reason = "Client accepted" };
        var winResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/win", winDto);
        winResponse.EnsureSuccessStatusCode();

        var wonProposal = await GetProposalAsync(proposalId);
        wonProposal.Status.Should().Be("Won");
        wonProposal.WonDate.Should().NotBeNull();
    }

    [Fact]
    public async Task FullWorkflow_DraftToLost_CompletesSuccessfully()
    {
        // 1. Create draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // 2. Submit proposal
        await SubmitProposalAsync(proposalId);

        // 3. Approve proposal
        await ApproveProposalAsync(proposalId);

        // 4. Mark as lost
        var loseDto = new { Reason = "Client went with competitor" };
        var loseResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/lose", loseDto);
        loseResponse.EnsureSuccessStatusCode();

        var lostProposal = await GetProposalAsync(proposalId);
        lostProposal.Status.Should().Be("Lost");
        lostProposal.LostDate.Should().NotBeNull();
        lostProposal.WonLostReason.Should().Be("Client went with competitor");
    }

    [Fact]
    public async Task FullWorkflow_RejectAndRevise_CompletesSuccessfully()
    {
        // 1. Create draft proposal
        var proposalId = await CreateDraftProposalWithItemsAsync();

        // 2. Submit proposal
        await SubmitProposalAsync(proposalId);

        // 3. Reject proposal
        var rejectDto = new { Reason = "Pricing needs adjustment" };
        var rejectResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/reject", rejectDto);
        rejectResponse.EnsureSuccessStatusCode();

        var rejectedProposal = await GetProposalAsync(proposalId);
        rejectedProposal.Status.Should().Be("Rejected");

        // 4. Create revision from rejected proposal
        var revisionDto = new { Notes = "Adjusted pricing" };
        var revisionResponse = await AdminClient.PostAsJsonAsync($"/api/proposals/{proposalId}/revise", revisionDto);
        revisionResponse.EnsureSuccessStatusCode();
        var revision = await revisionResponse.Content.ReadFromJsonAsync<ProposalDto>();

        revision!.Version.Should().Be(2);
        revision.Status.Should().Be("Draft");
        revision.ParentProposalId.Should().Be(proposalId);
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateDraftProposalWithItemsAsync()
    {
        // Create proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Workflow Test Proposal {Guid.NewGuid():N}",
            ProposalDate = DateTime.UtcNow,
            ValidUntilDate = DateTime.UtcNow.AddDays(30),
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

        // Add a line item
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
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

    private async Task<ProposalDto> GetProposalAsync(Guid proposalId)
    {
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProposalDto>())!;
    }

    #endregion

    #region Response DTOs

    private record ProposalDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        Guid? ParentProposalId,
        Guid CompanyId,
        string CompanyName,
        string ProjectName,
        string Status,
        DateTime? SubmittedDate,
        DateTime? ApprovedDate,
        string? ApprovedBy,
        DateTime? WonDate,
        DateTime? LostDate,
        string? WonLostReason,
        decimal Subtotal,
        decimal VatAmount,
        decimal GrandTotal,
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
        string Description,
        decimal Quantity,
        decimal UnitPrice,
        decimal LineTotal
    );

    private record ProposalListDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        string ProjectName,
        string Status,
        DateTime CreatedAt
    );

    #endregion
}
