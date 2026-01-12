namespace Rascor.Modules.Rams.Application.DTOs;

/// <summary>
/// Complete dashboard data including summary stats, charts data, and lists
/// </summary>
public record RamsDashboardDto
{
    public RamsSummaryStatsDto Summary { get; init; } = new();
    public List<RamsStatusCountDto> StatusCounts { get; init; } = [];
    public List<RamsProjectTypeCountDto> ProjectTypeCounts { get; init; } = [];
    public List<RamsRiskDistributionDto> RiskDistribution { get; init; } = [];
    public List<RamsMonthlyTrendDto> MonthlyTrends { get; init; } = [];
    public List<RamsPendingApprovalDto> PendingApprovals { get; init; } = [];
    public List<RamsOverdueDocumentDto> OverdueDocuments { get; init; } = [];
    public RamsApprovalMetricsDto ApprovalMetrics { get; init; } = new();
}

/// <summary>
/// Summary statistics for the dashboard cards
/// </summary>
public record RamsSummaryStatsDto
{
    public int TotalDocuments { get; init; }
    public int DraftDocuments { get; init; }
    public int PendingReviewDocuments { get; init; }
    public int ApprovedDocuments { get; init; }
    public int RejectedDocuments { get; init; }
    public int ArchivedDocuments { get; init; }
    public int TotalRiskAssessments { get; init; }
    public int HighRiskCount { get; init; }
    public int MediumRiskCount { get; init; }
    public int LowRiskCount { get; init; }
    public int DocumentsThisMonth { get; init; }
    public int ApprovalsThisMonth { get; init; }
}

/// <summary>
/// Document count by status for pie/bar charts
/// </summary>
public record RamsStatusCountDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

/// <summary>
/// Document count by project type
/// </summary>
public record RamsProjectTypeCountDto
{
    public string ProjectType { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

/// <summary>
/// Risk distribution comparing initial vs residual risk levels
/// </summary>
public record RamsRiskDistributionDto
{
    public string RiskLevel { get; init; } = string.Empty;
    public int InitialCount { get; init; }
    public int ResidualCount { get; init; }
}

/// <summary>
/// Monthly trend data for line charts
/// </summary>
public record RamsMonthlyTrendDto
{
    public string Month { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Created { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
}

/// <summary>
/// Document pending approval with key details
/// </summary>
public record RamsPendingApprovalDto
{
    public Guid Id { get; init; }
    public string ProjectReference { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectType { get; init; } = string.Empty;
    public string? ClientName { get; init; }
    public DateTime SubmittedAt { get; init; }
    public int DaysPending { get; init; }
    public int RiskAssessmentCount { get; init; }
    public int HighRiskCount { get; init; }
    public string? SubmittedByName { get; init; }
}

/// <summary>
/// Document that is overdue (past proposed end date and not approved)
/// </summary>
public record RamsOverdueDocumentDto
{
    public Guid Id { get; init; }
    public string ProjectReference { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly? ProposedEndDate { get; init; }
    public int DaysOverdue { get; init; }
    public string? SafetyOfficerName { get; init; }
}

/// <summary>
/// Metrics related to the approval process
/// </summary>
public record RamsApprovalMetricsDto
{
    public double AverageApprovalDays { get; init; }
    public double AverageRejectionRate { get; init; }
    public int FastestApprovalDays { get; init; }
    public int SlowestApprovalDays { get; init; }
    public int TotalApprovedLast30Days { get; init; }
    public int TotalRejectedLast30Days { get; init; }
}

/// <summary>
/// Request for exporting RAMS data to Excel
/// </summary>
public record RamsExportRequestDto
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? Status { get; init; }
    public string? ProjectType { get; init; }
    public bool IncludeRiskAssessments { get; init; } = true;
    public bool IncludeMethodSteps { get; init; } = true;
}
