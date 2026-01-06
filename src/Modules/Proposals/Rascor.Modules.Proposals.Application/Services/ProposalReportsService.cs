using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Application.DTOs;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for generating proposal analytics and reports
/// </summary>
public class ProposalReportsService : IProposalReportsService
{
    private readonly IProposalsDbContext _context;

    // Active pipeline statuses
    private static readonly ProposalStatus[] PipelineStatuses = new[]
    {
        ProposalStatus.Draft,
        ProposalStatus.Submitted,
        ProposalStatus.UnderReview,
        ProposalStatus.Approved
    };

    public ProposalReportsService(IProposalsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Ensures a DateTime is in UTC format for PostgreSQL compatibility
    /// </summary>
    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    public async Task<ProposalPipelineReportDto> GetPipelineReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Proposals.AsQueryable();

        // Apply date filters (ensure UTC for PostgreSQL)
        if (fromDate.HasValue)
        {
            var utcFromDate = EnsureUtc(fromDate.Value);
            query = query.Where(p => p.CreatedAt >= utcFromDate);
        }
        if (toDate.HasValue)
        {
            var utcToDate = EnsureUtc(toDate.Value);
            query = query.Where(p => p.CreatedAt <= utcToDate);
        }

        // Filter to pipeline statuses only
        query = query.Where(p => PipelineStatuses.Contains(p.Status));

        var proposals = await query
            .Select(p => new { p.Status, p.GrandTotal })
            .ToListAsync();

        var totalValue = proposals.Sum(p => p.GrandTotal);
        var totalCount = proposals.Count;

        var stages = proposals
            .GroupBy(p => p.Status)
            .Select(g => new PipelineStageDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Value = g.Sum(p => p.GrandTotal),
                Percentage = totalValue > 0 ? Math.Round(g.Sum(p => p.GrandTotal) / totalValue * 100, 1) : 0
            })
            .OrderBy(s => GetStatusOrder(s.Status))
            .ToList();

        // Ensure all pipeline stages are represented
        foreach (var status in PipelineStatuses)
        {
            if (!stages.Any(s => s.Status == status.ToString()))
            {
                stages.Add(new PipelineStageDto
                {
                    Status = status.ToString(),
                    Count = 0,
                    Value = 0,
                    Percentage = 0
                });
            }
        }

        stages = stages.OrderBy(s => GetStatusOrder(s.Status)).ToList();

        return new ProposalPipelineReportDto
        {
            TotalPipelineValue = totalValue,
            TotalProposals = totalCount,
            Stages = stages,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<ProposalConversionReportDto> GetConversionReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Proposals.AsQueryable();

        // Apply date filters (ensure UTC for PostgreSQL)
        if (fromDate.HasValue)
        {
            var utcFromDate = EnsureUtc(fromDate.Value);
            query = query.Where(p => p.CreatedAt >= utcFromDate);
        }
        if (toDate.HasValue)
        {
            var utcToDate = EnsureUtc(toDate.Value);
            query = query.Where(p => p.CreatedAt <= utcToDate);
        }

        var proposals = await query
            .Select(p => new { p.Status, p.GrandTotal })
            .ToListAsync();

        var totalCount = proposals.Count;
        var wonProposals = proposals.Where(p => p.Status == ProposalStatus.Won).ToList();
        var lostProposals = proposals.Where(p => p.Status == ProposalStatus.Lost).ToList();
        var cancelledProposals = proposals.Where(p => p.Status == ProposalStatus.Cancelled).ToList();
        var pendingProposals = proposals.Where(p =>
            p.Status != ProposalStatus.Won &&
            p.Status != ProposalStatus.Lost &&
            p.Status != ProposalStatus.Cancelled).ToList();

        var wonCount = wonProposals.Count;
        var lostCount = lostProposals.Count;
        var wonValue = wonProposals.Sum(p => p.GrandTotal);
        var lostValue = lostProposals.Sum(p => p.GrandTotal);
        var totalValue = proposals.Sum(p => p.GrandTotal);

        // Conversion rate: Won / (Won + Lost)
        var conversionRate = wonCount + lostCount > 0
            ? Math.Round((decimal)wonCount / (wonCount + lostCount) * 100, 1)
            : 0;

        // Win rate: Won / Total
        var winRate = totalCount > 0
            ? Math.Round((decimal)wonCount / totalCount * 100, 1)
            : 0;

        return new ProposalConversionReportDto
        {
            TotalProposals = totalCount,
            WonCount = wonCount,
            LostCount = lostCount,
            PendingCount = pendingProposals.Count,
            CancelledCount = cancelledProposals.Count,
            WonValue = wonValue,
            LostValue = lostValue,
            ConversionRate = conversionRate,
            WinRate = winRate,
            AverageProposalValue = totalCount > 0 ? Math.Round(totalValue / totalCount, 2) : 0,
            AverageWonValue = wonCount > 0 ? Math.Round(wonValue / wonCount, 2) : 0,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<ProposalsByStatusReportDto> GetByStatusReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Proposals.AsQueryable();

        // Apply date filters (ensure UTC for PostgreSQL)
        if (fromDate.HasValue)
        {
            var utcFromDate = EnsureUtc(fromDate.Value);
            query = query.Where(p => p.CreatedAt >= utcFromDate);
        }
        if (toDate.HasValue)
        {
            var utcToDate = EnsureUtc(toDate.Value);
            query = query.Where(p => p.CreatedAt <= utcToDate);
        }

        var proposals = await query
            .Select(p => new { p.Status, p.GrandTotal })
            .ToListAsync();

        var totalCount = proposals.Count;
        var totalValue = proposals.Sum(p => p.GrandTotal);

        var statuses = proposals
            .GroupBy(p => p.Status)
            .Select(g => new StatusBreakdownDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Value = g.Sum(p => p.GrandTotal),
                AverageValue = g.Any() ? Math.Round(g.Sum(p => p.GrandTotal) / g.Count(), 2) : 0,
                PercentageOfTotal = totalCount > 0 ? Math.Round((decimal)g.Count() / totalCount * 100, 1) : 0
            })
            .OrderBy(s => GetStatusOrder(s.Status))
            .ToList();

        return new ProposalsByStatusReportDto
        {
            Statuses = statuses,
            TotalCount = totalCount,
            TotalValue = totalValue,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<ProposalsByCompanyReportDto> GetByCompanyReportAsync(DateTime? fromDate, DateTime? toDate, int top = 10)
    {
        var query = _context.Proposals.AsQueryable();

        // Apply date filters (ensure UTC for PostgreSQL)
        if (fromDate.HasValue)
        {
            var utcFromDate = EnsureUtc(fromDate.Value);
            query = query.Where(p => p.CreatedAt >= utcFromDate);
        }
        if (toDate.HasValue)
        {
            var utcToDate = EnsureUtc(toDate.Value);
            query = query.Where(p => p.CreatedAt <= utcToDate);
        }

        var proposals = await query
            .Select(p => new { p.CompanyId, p.CompanyName, p.Status, p.GrandTotal })
            .ToListAsync();

        var companies = proposals
            .GroupBy(p => new { p.CompanyId, p.CompanyName })
            .Select(g =>
            {
                var wonCount = g.Count(p => p.Status == ProposalStatus.Won);
                var lostCount = g.Count(p => p.Status == ProposalStatus.Lost);
                var wonValue = g.Where(p => p.Status == ProposalStatus.Won).Sum(p => p.GrandTotal);
                var totalValue = g.Sum(p => p.GrandTotal);
                var conversionRate = wonCount + lostCount > 0
                    ? Math.Round((decimal)wonCount / (wonCount + lostCount) * 100, 1)
                    : 0;

                return new CompanyProposalSummaryDto
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    TotalProposals = g.Count(),
                    WonCount = wonCount,
                    LostCount = lostCount,
                    TotalValue = totalValue,
                    WonValue = wonValue,
                    ConversionRate = conversionRate
                };
            })
            .OrderByDescending(c => c.TotalValue)
            .Take(top)
            .ToList();

        return new ProposalsByCompanyReportDto
        {
            Companies = companies,
            TotalCompanies = proposals.Select(p => p.CompanyId).Distinct().Count(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<WinLossAnalysisReportDto> GetWinLossAnalysisAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Proposals.AsQueryable();

        // Apply date filters (ensure UTC for PostgreSQL)
        if (fromDate.HasValue)
        {
            var utcFromDate = EnsureUtc(fromDate.Value);
            query = query.Where(p => p.CreatedAt >= utcFromDate);
        }
        if (toDate.HasValue)
        {
            var utcToDate = EnsureUtc(toDate.Value);
            query = query.Where(p => p.CreatedAt <= utcToDate);
        }

        // Get won and lost proposals with their details
        var wonLostProposals = await query
            .Where(p => p.Status == ProposalStatus.Won || p.Status == ProposalStatus.Lost)
            .Select(p => new
            {
                p.Status,
                p.GrandTotal,
                p.WonLostReason,
                p.CreatedAt,
                p.WonDate,
                p.LostDate
            })
            .ToListAsync();

        var wonProposals = wonLostProposals.Where(p => p.Status == ProposalStatus.Won).ToList();
        var lostProposals = wonLostProposals.Where(p => p.Status == ProposalStatus.Lost).ToList();

        // Parse and group win reasons
        var winReasons = wonProposals
            .GroupBy(p => string.IsNullOrWhiteSpace(p.WonLostReason) ? "Not specified" : p.WonLostReason.Trim())
            .Select(g => new WinLossReasonDto
            {
                Reason = g.Key,
                Count = g.Count(),
                Value = g.Sum(p => p.GrandTotal),
                Percentage = wonProposals.Count > 0
                    ? Math.Round((decimal)g.Count() / wonProposals.Count * 100, 1)
                    : 0
            })
            .OrderByDescending(r => r.Count)
            .ToList();

        // Parse and group loss reasons
        var lossReasons = lostProposals
            .GroupBy(p => string.IsNullOrWhiteSpace(p.WonLostReason) ? "Not specified" : p.WonLostReason.Trim())
            .Select(g => new WinLossReasonDto
            {
                Reason = g.Key,
                Count = g.Count(),
                Value = g.Sum(p => p.GrandTotal),
                Percentage = lostProposals.Count > 0
                    ? Math.Round((decimal)g.Count() / lostProposals.Count * 100, 1)
                    : 0
            })
            .OrderByDescending(r => r.Count)
            .ToList();

        // Calculate average time to win/lose
        var averageTimeToWin = 0m;
        if (wonProposals.Any(p => p.WonDate.HasValue))
        {
            var timesWon = wonProposals
                .Where(p => p.WonDate.HasValue)
                .Select(p => (p.WonDate!.Value - p.CreatedAt).TotalDays)
                .ToList();
            if (timesWon.Any())
                averageTimeToWin = Math.Round((decimal)timesWon.Average(), 1);
        }

        var averageTimeToLose = 0m;
        if (lostProposals.Any(p => p.LostDate.HasValue))
        {
            var timesLost = lostProposals
                .Where(p => p.LostDate.HasValue)
                .Select(p => (p.LostDate!.Value - p.CreatedAt).TotalDays)
                .ToList();
            if (timesLost.Any())
                averageTimeToLose = Math.Round((decimal)timesLost.Average(), 1);
        }

        return new WinLossAnalysisReportDto
        {
            WinReasons = winReasons,
            LossReasons = lossReasons,
            AverageTimeToWinDays = averageTimeToWin,
            AverageTimeToLossDays = averageTimeToLose,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<MonthlyTrendsReportDto> GetMonthlyTrendsAsync(int months = 12)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc); // Start of month with UTC

        var proposals = await _context.Proposals
            .Where(p => p.CreatedAt >= startDate || p.WonDate >= startDate || p.LostDate >= startDate)
            .Select(p => new
            {
                p.CreatedAt,
                p.Status,
                p.GrandTotal,
                p.WonDate,
                p.LostDate
            })
            .ToListAsync();

        var dataPoints = new List<MonthlyDataPointDto>();

        for (int i = 0; i < months; i++)
        {
            var monthDate = DateTime.UtcNow.AddMonths(-months + 1 + i);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            // Created in this month
            var createdThisMonth = proposals.Where(p =>
                p.CreatedAt >= monthStart && p.CreatedAt < monthEnd).ToList();

            // Won in this month (by WonDate)
            var wonThisMonth = proposals.Where(p =>
                p.WonDate.HasValue && p.WonDate.Value >= monthStart && p.WonDate.Value < monthEnd).ToList();

            // Lost in this month (by LostDate)
            var lostThisMonth = proposals.Where(p =>
                p.LostDate.HasValue && p.LostDate.Value >= monthStart && p.LostDate.Value < monthEnd).ToList();

            var wonCount = wonThisMonth.Count;
            var lostCount = lostThisMonth.Count;
            var conversionRate = wonCount + lostCount > 0
                ? Math.Round((decimal)wonCount / (wonCount + lostCount) * 100, 1)
                : 0;

            dataPoints.Add(new MonthlyDataPointDto
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                MonthName = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                ProposalsCreated = createdThisMonth.Count,
                ProposalsWon = wonCount,
                ProposalsLost = lostCount,
                ValueCreated = createdThisMonth.Sum(p => p.GrandTotal),
                ValueWon = wonThisMonth.Sum(p => p.GrandTotal),
                ValueLost = lostThisMonth.Sum(p => p.GrandTotal),
                ConversionRate = conversionRate
            });
        }

        return new MonthlyTrendsReportDto
        {
            DataPoints = dataPoints,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static int GetStatusOrder(string status)
    {
        return status switch
        {
            "Draft" => 0,
            "Submitted" => 1,
            "UnderReview" => 2,
            "Approved" => 3,
            "Rejected" => 4,
            "Won" => 5,
            "Lost" => 6,
            "Expired" => 7,
            "Cancelled" => 8,
            _ => 99
        };
    }
}
