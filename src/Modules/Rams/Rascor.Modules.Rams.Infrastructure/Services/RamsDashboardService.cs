using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for RAMS dashboard statistics and reporting
/// </summary>
public class RamsDashboardService : IRamsDashboardService
{
    private readonly IRamsDbContext _context;
    private readonly ICoreDbContext _coreContext;
    private readonly ILogger<RamsDashboardService> _logger;

    public RamsDashboardService(
        IRamsDbContext context,
        ICoreDbContext coreContext,
        ILogger<RamsDashboardService> logger)
    {
        _context = context;
        _coreContext = coreContext;
        _logger = logger;
    }

    public async Task<RamsDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var last30Days = now.AddDays(-30);

        // Get all documents with risk assessments
        var documents = await _context.RamsDocuments
            .Include(d => d.RiskAssessments)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var riskAssessments = documents.SelectMany(d => d.RiskAssessments).ToList();

        // Summary stats
        var summary = new RamsSummaryStatsDto
        {
            TotalDocuments = documents.Count,
            DraftDocuments = documents.Count(d => d.Status == RamsStatus.Draft),
            PendingReviewDocuments = documents.Count(d => d.Status == RamsStatus.PendingReview),
            ApprovedDocuments = documents.Count(d => d.Status == RamsStatus.Approved),
            RejectedDocuments = documents.Count(d => d.Status == RamsStatus.Rejected),
            ArchivedDocuments = documents.Count(d => d.Status == RamsStatus.Archived),
            TotalRiskAssessments = riskAssessments.Count,
            HighRiskCount = riskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.High),
            MediumRiskCount = riskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.Medium),
            LowRiskCount = riskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.Low),
            DocumentsThisMonth = documents.Count(d => d.CreatedAt >= startOfMonth),
            ApprovalsThisMonth = documents.Count(d => d.DateApproved >= startOfMonth)
        };

        // Status counts for pie chart
        var statusCounts = Enum.GetValues<RamsStatus>()
            .Select(status => new RamsStatusCountDto
            {
                Status = status.ToString(),
                Count = documents.Count(d => d.Status == status),
                Percentage = documents.Count > 0
                    ? Math.Round((decimal)documents.Count(d => d.Status == status) / documents.Count * 100, 1)
                    : 0
            })
            .Where(s => s.Count > 0)
            .OrderByDescending(s => s.Count)
            .ToList();

        // Project type counts
        var projectTypeCounts = Enum.GetValues<ProjectType>()
            .Select(pt => new RamsProjectTypeCountDto
            {
                ProjectType = FormatProjectType(pt),
                Count = documents.Count(d => d.ProjectType == pt),
                Percentage = documents.Count > 0
                    ? Math.Round((decimal)documents.Count(d => d.ProjectType == pt) / documents.Count * 100, 1)
                    : 0
            })
            .Where(p => p.Count > 0)
            .OrderByDescending(p => p.Count)
            .ToList();

        // Risk distribution (comparing initial vs residual)
        var riskDistribution = new List<RamsRiskDistributionDto>
        {
            new()
            {
                RiskLevel = "High",
                InitialCount = riskAssessments.Count(r => r.InitialRiskLevel == RiskLevel.High),
                ResidualCount = riskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.High)
            },
            new()
            {
                RiskLevel = "Medium",
                InitialCount = riskAssessments.Count(r => r.InitialRiskLevel == RiskLevel.Medium),
                ResidualCount = riskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.Medium)
            },
            new()
            {
                RiskLevel = "Low",
                InitialCount = riskAssessments.Count(r => r.InitialRiskLevel == RiskLevel.Low),
                ResidualCount = riskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.Low)
            }
        };

        // Monthly trends (last 6 months)
        var monthlyTrends = GetMonthlyTrends(documents, now);

        // Pending approvals
        var pendingApprovals = await GetPendingApprovalsAsync(cancellationToken);

        // Overdue documents
        var overdueDocuments = await GetOverdueDocumentsAsync(cancellationToken);

        // Approval metrics
        var approvalMetrics = CalculateApprovalMetrics(documents, last30Days);

        return new RamsDashboardDto
        {
            Summary = summary,
            StatusCounts = statusCounts,
            ProjectTypeCounts = projectTypeCounts,
            RiskDistribution = riskDistribution,
            MonthlyTrends = monthlyTrends,
            PendingApprovals = pendingApprovals.Take(10).ToList(),
            OverdueDocuments = overdueDocuments.Take(10).ToList(),
            ApprovalMetrics = approvalMetrics
        };
    }

    public async Task<List<RamsPendingApprovalDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var pendingDocs = await _context.RamsDocuments
            .Where(d => d.Status == RamsStatus.PendingReview)
            .Include(d => d.RiskAssessments)
            .OrderBy(d => d.UpdatedAt ?? d.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pendingDocs.Select(d => new RamsPendingApprovalDto
        {
            Id = d.Id,
            ProjectReference = d.ProjectReference,
            ProjectName = d.ProjectName,
            ProjectType = FormatProjectType(d.ProjectType),
            ClientName = d.ClientName,
            SubmittedAt = d.UpdatedAt ?? d.CreatedAt,
            DaysPending = (int)(now - (d.UpdatedAt ?? d.CreatedAt)).TotalDays,
            RiskAssessmentCount = d.RiskAssessments.Count,
            HighRiskCount = d.RiskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.High),
            SubmittedByName = null // Would need user lookup
        }).ToList();
    }

    public async Task<List<RamsOverdueDocumentDto>> GetOverdueDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var overdueDocs = await _context.RamsDocuments
            .Where(d => d.ProposedEndDate < today &&
                       (d.Status == RamsStatus.Draft ||
                        d.Status == RamsStatus.PendingReview ||
                        d.Status == RamsStatus.Rejected))
            .OrderBy(d => d.ProposedEndDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Get safety officer names
        var safetyOfficerIds = overdueDocs
            .Where(d => d.SafetyOfficerId.HasValue)
            .Select(d => d.SafetyOfficerId!.Value)
            .Distinct()
            .ToList();

        var safetyOfficerNames = new Dictionary<Guid, string>();
        if (safetyOfficerIds.Any())
        {
            safetyOfficerNames = await _coreContext.Employees
                .Where(e => safetyOfficerIds.Contains(e.Id))
                .ToDictionaryAsync(
                    e => e.Id,
                    e => $"{e.FirstName} {e.LastName}",
                    cancellationToken);
        }

        return overdueDocs.Select(d => new RamsOverdueDocumentDto
        {
            Id = d.Id,
            ProjectReference = d.ProjectReference,
            ProjectName = d.ProjectName,
            Status = d.Status.ToString(),
            ProposedEndDate = d.ProposedEndDate,
            DaysOverdue = d.ProposedEndDate.HasValue
                ? today.DayNumber - d.ProposedEndDate.Value.DayNumber
                : 0,
            SafetyOfficerName = d.SafetyOfficerId.HasValue && safetyOfficerNames.TryGetValue(d.SafetyOfficerId.Value, out var name)
                ? name
                : null
        }).ToList();
    }

    public async Task<byte[]> ExportToExcelAsync(RamsExportRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting RAMS Excel export");

        var query = _context.RamsDocuments
            .Include(d => d.RiskAssessments)
            .Include(d => d.MethodSteps)
            .AsNoTracking();

        // Apply filters
        if (request.DateFrom.HasValue)
        {
            var dateFrom = DateTime.SpecifyKind(request.DateFrom.Value, DateTimeKind.Utc);
            query = query.Where(d => d.CreatedAt >= dateFrom);
        }

        if (request.DateTo.HasValue)
        {
            var dateTo = DateTime.SpecifyKind(request.DateTo.Value.AddDays(1), DateTimeKind.Utc);
            query = query.Where(d => d.CreatedAt < dateTo);
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<RamsStatus>(request.Status, out var status))
        {
            query = query.Where(d => d.Status == status);
        }

        if (!string.IsNullOrEmpty(request.ProjectType) && Enum.TryParse<ProjectType>(request.ProjectType, out var projectType))
        {
            query = query.Where(d => d.ProjectType == projectType);
        }

        var documents = await query.OrderByDescending(d => d.CreatedAt).ToListAsync(cancellationToken);

        // Get related data for lookups
        var siteIds = documents.Where(d => d.SiteId.HasValue).Select(d => d.SiteId!.Value).Distinct().ToList();
        var safetyOfficerIds = documents.Where(d => d.SafetyOfficerId.HasValue).Select(d => d.SafetyOfficerId!.Value).Distinct().ToList();

        var siteNames = new Dictionary<Guid, string>();
        if (siteIds.Any())
        {
            siteNames = await _coreContext.Sites
                .Where(s => siteIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SiteName, cancellationToken);
        }

        var employeeNames = new Dictionary<Guid, string>();
        if (safetyOfficerIds.Any())
        {
            employeeNames = await _coreContext.Employees
                .Where(e => safetyOfficerIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => $"{e.FirstName} {e.LastName}", cancellationToken);
        }

        using var workbook = new XLWorkbook();

        // Documents sheet
        AddDocumentsSheet(workbook, documents, siteNames, employeeNames);

        // Risk Assessments sheet
        if (request.IncludeRiskAssessments)
        {
            AddRiskAssessmentsSheet(workbook, documents);
        }

        // Method Steps sheet
        if (request.IncludeMethodSteps)
        {
            AddMethodStepsSheet(workbook, documents);
        }

        // Summary sheet
        AddSummarySheet(workbook, documents);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        _logger.LogInformation("RAMS Excel export completed with {Count} documents", documents.Count);

        return stream.ToArray();
    }

    private static List<RamsMonthlyTrendDto> GetMonthlyTrends(List<RamsDocument> documents, DateTime now)
    {
        var monthlyTrends = new List<RamsMonthlyTrendDto>();

        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);

            monthlyTrends.Add(new RamsMonthlyTrendDto
            {
                Month = monthStart.ToString("MMM"),
                Year = monthStart.Year,
                Created = documents.Count(d => d.CreatedAt >= monthStart && d.CreatedAt < monthEnd),
                Approved = documents.Count(d => d.DateApproved >= monthStart && d.DateApproved < monthEnd),
                Rejected = documents.Count(d =>
                    d.Status == RamsStatus.Rejected &&
                    d.UpdatedAt >= monthStart &&
                    d.UpdatedAt < monthEnd)
            });
        }

        return monthlyTrends;
    }

    private static RamsApprovalMetricsDto CalculateApprovalMetrics(List<RamsDocument> documents, DateTime last30Days)
    {
        // Documents that have been approved and have both dates
        var approvedDocs = documents
            .Where(d => d.DateApproved.HasValue && d.CreatedAt != default)
            .Select(d => new { d.DateApproved, d.CreatedAt, d.UpdatedAt })
            .ToList();

        var approvalDays = approvedDocs
            .Select(d => (d.DateApproved!.Value - d.CreatedAt).TotalDays)
            .Where(days => days >= 0)
            .ToList();

        return new RamsApprovalMetricsDto
        {
            AverageApprovalDays = approvalDays.Any() ? Math.Round(approvalDays.Average(), 1) : 0,
            AverageRejectionRate = documents.Any()
                ? Math.Round((double)documents.Count(d => d.Status == RamsStatus.Rejected) / documents.Count * 100, 1)
                : 0,
            FastestApprovalDays = approvalDays.Any() ? (int)approvalDays.Min() : 0,
            SlowestApprovalDays = approvalDays.Any() ? (int)approvalDays.Max() : 0,
            TotalApprovedLast30Days = documents.Count(d => d.DateApproved >= last30Days),
            TotalRejectedLast30Days = documents.Count(d =>
                d.Status == RamsStatus.Rejected &&
                d.UpdatedAt >= last30Days)
        };
    }

    private static string FormatProjectType(ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.RemedialInjection => "Remedial Injection",
            ProjectType.RascotankNewBuild => "Rascotank New Build",
            ProjectType.CarParkCoating => "Car Park Coating",
            ProjectType.GroundGasBarrier => "Ground Gas Barrier",
            ProjectType.Other => "Other",
            _ => projectType.ToString()
        };
    }

    private static void AddDocumentsSheet(
        XLWorkbook workbook,
        List<RamsDocument> documents,
        Dictionary<Guid, string> siteNames,
        Dictionary<Guid, string> employeeNames)
    {
        var sheet = workbook.AddWorksheet("RAMS Documents");

        // Header
        var headers = new[]
        {
            "Reference", "Project Name", "Project Type", "Client", "Site", "Site Address",
            "Status", "Start Date", "End Date", "Safety Officer", "Date Approved",
            "Risk Assessments", "High Risks", "Method Steps", "Created", "Modified"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
            sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Data
        int row = 2;
        foreach (var doc in documents)
        {
            var siteName = doc.SiteId.HasValue && siteNames.TryGetValue(doc.SiteId.Value, out var sn) ? sn : "";
            var safetyOfficerName = doc.SafetyOfficerId.HasValue && employeeNames.TryGetValue(doc.SafetyOfficerId.Value, out var en) ? en : "";

            sheet.Cell(row, 1).Value = doc.ProjectReference;
            sheet.Cell(row, 2).Value = doc.ProjectName;
            sheet.Cell(row, 3).Value = FormatProjectType(doc.ProjectType);
            sheet.Cell(row, 4).Value = doc.ClientName ?? "";
            sheet.Cell(row, 5).Value = siteName;
            sheet.Cell(row, 6).Value = doc.SiteAddress ?? "";
            sheet.Cell(row, 7).Value = doc.Status.ToString();
            sheet.Cell(row, 8).Value = doc.ProposedStartDate?.ToString("dd/MM/yyyy") ?? "";
            sheet.Cell(row, 9).Value = doc.ProposedEndDate?.ToString("dd/MM/yyyy") ?? "";
            sheet.Cell(row, 10).Value = safetyOfficerName;
            sheet.Cell(row, 11).Value = doc.DateApproved?.ToString("dd/MM/yyyy") ?? "";
            sheet.Cell(row, 12).Value = doc.RiskAssessments.Count;
            sheet.Cell(row, 13).Value = doc.RiskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.High);
            sheet.Cell(row, 14).Value = doc.MethodSteps.Count;
            sheet.Cell(row, 15).Value = doc.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            sheet.Cell(row, 16).Value = doc.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";

            // Color code status
            var statusCell = sheet.Cell(row, 7);
            SetStatusColor(statusCell, doc.Status);

            row++;
        }

        sheet.Columns().AdjustToContents();
    }

    private static void AddRiskAssessmentsSheet(XLWorkbook workbook, List<RamsDocument> documents)
    {
        var sheet = workbook.AddWorksheet("Risk Assessments");

        var headers = new[]
        {
            "Document Ref", "Project Name", "Task/Activity", "Location", "Hazard", "Who at Risk",
            "Initial L", "Initial S", "Initial Rating", "Initial Level",
            "Control Measures", "Legislation",
            "Residual L", "Residual S", "Residual Rating", "Residual Level",
            "AI Generated"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
            sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
        }

        int row = 2;
        foreach (var doc in documents)
        {
            foreach (var risk in doc.RiskAssessments.OrderBy(r => r.SortOrder))
            {
                sheet.Cell(row, 1).Value = doc.ProjectReference;
                sheet.Cell(row, 2).Value = doc.ProjectName;
                sheet.Cell(row, 3).Value = risk.TaskActivity;
                sheet.Cell(row, 4).Value = risk.LocationArea ?? "";
                sheet.Cell(row, 5).Value = risk.HazardIdentified;
                sheet.Cell(row, 6).Value = risk.WhoAtRisk ?? "";
                sheet.Cell(row, 7).Value = risk.InitialLikelihood;
                sheet.Cell(row, 8).Value = risk.InitialSeverity;
                sheet.Cell(row, 9).Value = risk.InitialRiskRating;
                sheet.Cell(row, 10).Value = risk.InitialRiskLevel.ToString();
                sheet.Cell(row, 11).Value = risk.ControlMeasures ?? "";
                sheet.Cell(row, 12).Value = risk.RelevantLegislation ?? "";
                sheet.Cell(row, 13).Value = risk.ResidualLikelihood;
                sheet.Cell(row, 14).Value = risk.ResidualSeverity;
                sheet.Cell(row, 15).Value = risk.ResidualRiskRating;
                sheet.Cell(row, 16).Value = risk.ResidualRiskLevel.ToString();
                sheet.Cell(row, 17).Value = risk.IsAiGenerated ? "Yes" : "No";

                // Color code risk levels
                SetRiskLevelColor(sheet.Cell(row, 10), risk.InitialRiskLevel);
                SetRiskLevelColor(sheet.Cell(row, 16), risk.ResidualRiskLevel);

                row++;
            }
        }

        sheet.Columns().AdjustToContents();
        sheet.Column(11).Width = 50; // Control measures column
    }

    private static void AddMethodStepsSheet(XLWorkbook workbook, List<RamsDocument> documents)
    {
        var sheet = workbook.AddWorksheet("Method Steps");

        var headers = new[]
        {
            "Document Ref", "Project Name", "Step #", "Title", "Procedure",
            "Required Permits", "Requires Sign-off"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
            sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
        }

        int row = 2;
        foreach (var doc in documents)
        {
            foreach (var step in doc.MethodSteps.OrderBy(s => s.StepNumber))
            {
                sheet.Cell(row, 1).Value = doc.ProjectReference;
                sheet.Cell(row, 2).Value = doc.ProjectName;
                sheet.Cell(row, 3).Value = step.StepNumber;
                sheet.Cell(row, 4).Value = step.StepTitle;
                sheet.Cell(row, 5).Value = step.DetailedProcedure ?? "";
                sheet.Cell(row, 6).Value = step.RequiredPermits ?? "";
                sheet.Cell(row, 7).Value = step.RequiresSignoff ? "Yes" : "No";
                row++;
            }
        }

        sheet.Columns().AdjustToContents();
        sheet.Column(5).Width = 60; // Procedure column
    }

    private static void AddSummarySheet(XLWorkbook workbook, List<RamsDocument> documents)
    {
        var sheet = workbook.AddWorksheet("Summary");

        sheet.Cell(1, 1).Value = "RAMS Export Summary";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        sheet.Cell(3, 1).Value = "Export Date:";
        sheet.Cell(3, 2).Value = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");

        sheet.Cell(5, 1).Value = "Total Documents:";
        sheet.Cell(5, 1).Style.Font.Bold = true;
        sheet.Cell(5, 2).Value = documents.Count;

        sheet.Cell(7, 1).Value = "Status Breakdown:";
        sheet.Cell(7, 1).Style.Font.Bold = true;

        var statusGroups = documents.GroupBy(d => d.Status).OrderByDescending(g => g.Count());
        int row = 8;
        foreach (var group in statusGroups)
        {
            sheet.Cell(row, 2).Value = group.Key.ToString();
            sheet.Cell(row, 3).Value = group.Count();
            SetStatusColor(sheet.Cell(row, 2), group.Key);
            row++;
        }

        row += 2;
        sheet.Cell(row, 1).Value = "Risk Assessment Summary:";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var allRisks = documents.SelectMany(d => d.RiskAssessments).ToList();
        sheet.Cell(row, 2).Value = "Total Risk Assessments:";
        sheet.Cell(row, 3).Value = allRisks.Count;
        row++;

        sheet.Cell(row, 2).Value = "High Risk (Residual):";
        sheet.Cell(row, 3).Value = allRisks.Count(r => r.ResidualRiskLevel == RiskLevel.High);
        sheet.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.Red;
        sheet.Cell(row, 3).Style.Font.FontColor = XLColor.White;
        row++;

        sheet.Cell(row, 2).Value = "Medium Risk (Residual):";
        sheet.Cell(row, 3).Value = allRisks.Count(r => r.ResidualRiskLevel == RiskLevel.Medium);
        sheet.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.Orange;
        row++;

        sheet.Cell(row, 2).Value = "Low Risk (Residual):";
        sheet.Cell(row, 3).Value = allRisks.Count(r => r.ResidualRiskLevel == RiskLevel.Low);
        sheet.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.Green;
        sheet.Cell(row, 3).Style.Font.FontColor = XLColor.White;

        sheet.Columns().AdjustToContents();
    }

    private static void SetStatusColor(IXLCell cell, RamsStatus status)
    {
        switch (status)
        {
            case RamsStatus.Draft:
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                break;
            case RamsStatus.PendingReview:
                cell.Style.Fill.BackgroundColor = XLColor.Orange;
                break;
            case RamsStatus.Approved:
                cell.Style.Fill.BackgroundColor = XLColor.Green;
                cell.Style.Font.FontColor = XLColor.White;
                break;
            case RamsStatus.Rejected:
                cell.Style.Fill.BackgroundColor = XLColor.Red;
                cell.Style.Font.FontColor = XLColor.White;
                break;
            case RamsStatus.Archived:
                cell.Style.Fill.BackgroundColor = XLColor.DarkGray;
                cell.Style.Font.FontColor = XLColor.White;
                break;
        }
    }

    private static void SetRiskLevelColor(IXLCell cell, RiskLevel level)
    {
        switch (level)
        {
            case RiskLevel.High:
                cell.Style.Fill.BackgroundColor = XLColor.Red;
                cell.Style.Font.FontColor = XLColor.White;
                break;
            case RiskLevel.Medium:
                cell.Style.Fill.BackgroundColor = XLColor.Orange;
                break;
            case RiskLevel.Low:
                cell.Style.Fill.BackgroundColor = XLColor.Green;
                cell.Style.Font.FontColor = XLColor.White;
                break;
        }
    }
}
