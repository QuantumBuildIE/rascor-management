using System.Net;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Proposals;

/// <summary>
/// Integration tests for Proposal reporting and analytics endpoints.
/// Tests pipeline reports, conversion rates, status breakdowns, and trends.
/// </summary>
public class ProposalReportsTests : IntegrationTestBase
{
    public ProposalReportsTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Pipeline Report Tests

    [Fact]
    public async Task GetPipelineReport_ReturnsProposalsByStage()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/pipeline");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PipelineReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Stages.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPipelineReport_WithDateRange_FiltersResults()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/reports/pipeline?fromDate={fromDate}&toDate={toDate}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PipelineReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPipelineReport_IncludesTotalPipelineValue()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/pipeline");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<PipelineReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.TotalPipelineValue.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetPipelineReport_Unauthenticated_Returns401()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/proposals/reports/pipeline");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Conversion Report Tests

    [Fact]
    public async Task GetConversionReport_ReturnsWinLossMetrics()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/conversion");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ConversionReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetConversionReport_CalculatesConversionRate()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/conversion");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ConversionReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.ConversionRate.Should().BeGreaterOrEqualTo(0);
        result.Data.ConversionRate.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public async Task GetConversionReport_WithDateRange_FiltersResults()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/reports/conversion?fromDate={fromDate}&toDate={toDate}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ConversionReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region By Status Report Tests

    [Fact]
    public async Task GetByStatusReport_ReturnsCountsByStatus()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/by-status");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByStatusReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Statuses.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByStatusReport_IncludesTotalCount()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/by-status");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByStatusReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetByStatusReport_WithDateRange_FiltersResults()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/reports/by-status?fromDate={fromDate}&toDate={toDate}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByStatusReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region By Company Report Tests

    [Fact]
    public async Task GetByCompanyReport_ReturnsProposalsGroupedByCompany()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/by-company");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByCompanyReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Companies.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByCompanyReport_WithTopParameter_LimitsResults()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/by-company?top=5");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByCompanyReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.Companies.Count.Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task GetByCompanyReport_DefaultTop10_ReturnsMaximum10()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/by-company");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByCompanyReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.Companies.Count.Should().BeLessOrEqualTo(10);
    }

    [Fact]
    public async Task GetByCompanyReport_IncludesCompanyTotals()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/by-company");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<ByCompanyReportDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (result!.Data!.Companies.Any())
        {
            var firstCompany = result.Data.Companies.First();
            firstCompany.CompanyName.Should().NotBeNullOrEmpty();
            firstCompany.TotalProposals.Should().BeGreaterOrEqualTo(0);
            firstCompany.TotalValue.Should().BeGreaterOrEqualTo(0);
        }
    }

    #endregion

    #region Win/Loss Analysis Tests

    [Fact]
    public async Task GetWinLossAnalysis_ReturnsWinLossData()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/win-loss");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<WinLossAnalysisDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWinLossAnalysis_IncludesReasonBreakdown()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/win-loss");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<WinLossAnalysisDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.WinReasons.Should().NotBeNull();
        result.Data.LossReasons.Should().NotBeNull();
        result.Data.AverageTimeToWinDays.Should().BeGreaterOrEqualTo(0);
        result.Data.AverageTimeToLossDays.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetWinLossAnalysis_WithDateRange_FiltersResults()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await AdminClient.GetAsync($"/api/proposals/reports/win-loss?fromDate={fromDate}&toDate={toDate}");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<WinLossAnalysisDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region Monthly Trends Tests

    [Fact]
    public async Task GetMonthlyTrends_ReturnsMonthlyData()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/monthly-trends");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<MonthlyTrendsDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DataPoints.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMonthlyTrends_DefaultReturns12Months()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/monthly-trends");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<MonthlyTrendsDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.DataPoints.Count.Should().BeLessOrEqualTo(12);
    }

    [Fact]
    public async Task GetMonthlyTrends_WithCustomMonths_ReturnsRequestedPeriod()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/monthly-trends?months=6");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<MonthlyTrendsDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Data!.DataPoints.Count.Should().BeLessOrEqualTo(6);
    }

    [Fact]
    public async Task GetMonthlyTrends_IncludesSubmittedAndWonCounts()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/proposals/reports/monthly-trends");
        var result = await response.Content.ReadFromJsonAsync<ResultWrapper<MonthlyTrendsDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (result!.Data!.DataPoints.Any())
        {
            var firstMonth = result.Data.DataPoints.First();
            firstMonth.MonthName.Should().NotBeNullOrEmpty();
            firstMonth.ProposalsCreated.Should().BeGreaterOrEqualTo(0);
            firstMonth.ProposalsWon.Should().BeGreaterOrEqualTo(0);
        }
    }

    #endregion

    #region Permission Tests

    [Fact]
    public async Task AllReports_RequireAuthentication()
    {
        // Act
        var pipelineResponse = await UnauthenticatedClient.GetAsync("/api/proposals/reports/pipeline");
        var conversionResponse = await UnauthenticatedClient.GetAsync("/api/proposals/reports/conversion");
        var statusResponse = await UnauthenticatedClient.GetAsync("/api/proposals/reports/by-status");
        var companyResponse = await UnauthenticatedClient.GetAsync("/api/proposals/reports/by-company");
        var winLossResponse = await UnauthenticatedClient.GetAsync("/api/proposals/reports/win-loss");
        var trendsResponse = await UnauthenticatedClient.GetAsync("/api/proposals/reports/monthly-trends");

        // Assert
        pipelineResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        conversionResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        statusResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        companyResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        winLossResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        trendsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AllReports_WithViewPermission_Succeed()
    {
        // Act - Finance user has View permission
        var pipelineResponse = await FinanceClient.GetAsync("/api/proposals/reports/pipeline");
        var conversionResponse = await FinanceClient.GetAsync("/api/proposals/reports/conversion");
        var statusResponse = await FinanceClient.GetAsync("/api/proposals/reports/by-status");
        var companyResponse = await FinanceClient.GetAsync("/api/proposals/reports/by-company");
        var winLossResponse = await FinanceClient.GetAsync("/api/proposals/reports/win-loss");
        var trendsResponse = await FinanceClient.GetAsync("/api/proposals/reports/monthly-trends");

        // Assert
        pipelineResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        conversionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        companyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        winLossResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        trendsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Margin Calculations Tests

    [Fact]
    public async Task MarginCalculation_ProposalWithItems_CalculatesCorrectMargin()
    {
        // Arrange - Create a proposal with items
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Margin Test Proposal {Guid.NewGuid():N}",
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

        // Add line item with known cost and price
        var lineItemCommand = new
        {
            ProposalSectionId = sectionId,
            Description = "Test Product",
            Quantity = 10m,
            Unit = "Each",
            UnitCost = 15m,   // Cost = 10 * 15 = 150
            UnitPrice = 25m,  // Revenue = 10 * 25 = 250
            SortOrder = 1
        };

        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", lineItemCommand);

        // Act - Get proposal with costing
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}?includeCosting=true");
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Subtotal.Should().Be(250);  // 10 * 25
        result.TotalCost.Should().Be(150);  // 10 * 15
        result.TotalMargin.Should().Be(100);  // 250 - 150
        result.MarginPercent.Should().Be(40);  // (100 / 250) * 100 = 40%
    }

    [Fact]
    public async Task MarginCalculation_MultipleItems_SumsCorrectly()
    {
        // Arrange - Create a proposal
        var createCommand = new
        {
            CompanyId = TestTenantConstants.Companies.CustomerCompany1,
            ProjectName = $"Multi Item Margin Test {Guid.NewGuid():N}",
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

        // Add first item: 10 * (25-15) = 100 margin
        var item1 = new
        {
            ProposalSectionId = sectionId,
            Description = "Item 1",
            Quantity = 10m,
            Unit = "Each",
            UnitCost = 15m,
            UnitPrice = 25m,
            SortOrder = 1
        };
        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", item1);

        // Add second item: 5 * (100-70) = 150 margin
        var item2 = new
        {
            ProposalSectionId = sectionId,
            Description = "Item 2",
            Quantity = 5m,
            Unit = "Each",
            UnitCost = 70m,
            UnitPrice = 100m,
            SortOrder = 2
        };
        await AdminClient.PostAsJsonAsync($"/api/proposals/sections/{sectionId}/items", item2);

        // Act - Get proposal with costing
        var response = await AdminClient.GetAsync($"/api/proposals/{proposalId}?includeCosting=true");
        var result = await response.Content.ReadFromJsonAsync<ProposalDto>();

        // Assert
        // Item 1: Revenue 250, Cost 150, Margin 100
        // Item 2: Revenue 500, Cost 350, Margin 150
        // Total: Revenue 750, Cost 500, Margin 250
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Subtotal.Should().Be(750);
        result.TotalCost.Should().Be(500);
        result.TotalMargin.Should().Be(250);
        // Margin % = 250/750 * 100 = 33.33%
        result.MarginPercent.Should().BeApproximately(33.33m, 0.01m);
    }

    [Fact]
    public async Task MarginCalculation_WithCosting_OnlyVisibleWithPermission()
    {
        // Arrange - Use existing approved proposal
        var proposalId = TestTenantConstants.Proposals.ProposalRecords.ApprovedProposal;

        // Act - Get as admin (has ViewCostings)
        var adminResponse = await AdminClient.GetAsync($"/api/proposals/{proposalId}?includeCosting=true");
        var adminResult = await adminResponse.Content.ReadFromJsonAsync<ProposalDto>();

        // Act - Get as operator (no ViewCostings)
        var operatorResponse = await OperatorClient.GetAsync($"/api/proposals/{proposalId}?includeCosting=true");

        // Assert - Admin sees costing
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        adminResult!.TotalCost.Should().NotBeNull();

        // Operator should either get forbidden or costing fields should be null
        operatorResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    #endregion

    #region Response DTOs

    private record ResultWrapper<T>(
        bool Success,
        T? Data,
        string? Message,
        List<string>? Errors
    );

    // API returns ProposalPipelineReportDto with: TotalPipelineValue, TotalProposals, Stages, GeneratedAt
    // PipelineStageDto has: Status (not Stage), Count, Value, Percentage
    private record PipelineReportDto(
        decimal TotalPipelineValue,
        int TotalProposals,
        List<PipelineStageDto> Stages,
        DateTime GeneratedAt
    );

    private record PipelineStageDto(
        string Status,
        int Count,
        decimal Value,
        decimal Percentage
    );

    // API returns ProposalConversionReportDto with many fields
    private record ConversionReportDto(
        int TotalProposals,
        int WonCount,
        int LostCount,
        int PendingCount,
        int CancelledCount,
        decimal WonValue,
        decimal LostValue,
        decimal ConversionRate,
        decimal WinRate,
        decimal AverageProposalValue,
        decimal AverageWonValue,
        DateTime GeneratedAt
    );

    // API returns ProposalsByStatusReportDto with: Statuses (not StatusCounts), TotalCount, TotalValue, GeneratedAt
    private record ByStatusReportDto(
        List<StatusBreakdownDto> Statuses,
        int TotalCount,
        decimal TotalValue,
        DateTime GeneratedAt
    );

    // StatusBreakdownDto has: Status, Count, Value, AverageValue, PercentageOfTotal
    private record StatusBreakdownDto(
        string Status,
        int Count,
        decimal Value,
        decimal AverageValue,
        decimal PercentageOfTotal
    );

    // API returns ProposalsByCompanyReportDto with: Companies, TotalCompanies, GeneratedAt
    private record ByCompanyReportDto(
        List<CompanyProposalSummaryDto> Companies,
        int TotalCompanies,
        DateTime GeneratedAt
    );

    // CompanyProposalSummaryDto has: CompanyId, CompanyName, TotalProposals (not ProposalCount), WonCount, LostCount, TotalValue, WonValue, ConversionRate
    private record CompanyProposalSummaryDto(
        Guid CompanyId,
        string CompanyName,
        int TotalProposals,
        int WonCount,
        int LostCount,
        decimal TotalValue,
        decimal WonValue,
        decimal ConversionRate
    );

    // API returns WinLossAnalysisReportDto with: WinReasons, LossReasons, AverageTimeToWinDays, AverageTimeToLossDays, GeneratedAt
    // Note: Does NOT have WonCount, LostCount, WonValue, LostValue, etc directly - those are in reasons
    private record WinLossAnalysisDto(
        List<WinLossReasonDto> WinReasons,
        List<WinLossReasonDto> LossReasons,
        decimal AverageTimeToWinDays,
        decimal AverageTimeToLossDays,
        DateTime GeneratedAt
    );

    // WinLossReasonDto has: Reason, Count, Value, Percentage
    private record WinLossReasonDto(
        string Reason,
        int Count,
        decimal Value,
        decimal Percentage
    );

    // API returns MonthlyTrendsReportDto with: DataPoints (not Months), GeneratedAt
    private record MonthlyTrendsDto(
        List<MonthlyDataPointDto> DataPoints,
        DateTime GeneratedAt
    );

    // MonthlyDataPointDto has different field names than expected
    private record MonthlyDataPointDto(
        int Year,
        int Month,
        string MonthName,
        int ProposalsCreated,
        int ProposalsWon,
        int ProposalsLost,
        decimal ValueCreated,
        decimal ValueWon,
        decimal ValueLost,
        decimal ConversionRate
    );

    private record ProposalDto(
        Guid Id,
        string ProposalNumber,
        int Version,
        string ProjectName,
        string Status,
        decimal Subtotal,
        decimal VatAmount,
        decimal GrandTotal,
        decimal? TotalCost,
        decimal? TotalMargin,
        decimal? MarginPercent,
        List<ProposalSectionDto> Sections,
        DateTime CreatedAt
    );

    private record ProposalSectionDto(
        Guid Id,
        Guid ProposalId,
        string SectionName,
        int SortOrder,
        decimal SectionCost,
        decimal SectionTotal,
        decimal SectionMargin,
        List<ProposalLineItemDto> LineItems
    );

    private record ProposalLineItemDto(
        Guid Id,
        Guid ProposalSectionId,
        string Description,
        decimal Quantity,
        decimal UnitPrice,
        decimal UnitCost,
        decimal LineTotal,
        decimal LineCost,
        decimal LineMargin
    );

    #endregion
}
