using Rascor.Modules.Proposals.Application.DTOs;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for generating proposal analytics and reports
/// </summary>
public interface IProposalReportsService
{
    /// <summary>
    /// Get pipeline report showing proposals by stage with values
    /// </summary>
    Task<ProposalPipelineReportDto> GetPipelineReportAsync(DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Get conversion report with win/loss metrics
    /// </summary>
    Task<ProposalConversionReportDto> GetConversionReportAsync(DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Get proposals breakdown by status
    /// </summary>
    Task<ProposalsByStatusReportDto> GetByStatusReportAsync(DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Get proposals breakdown by company
    /// </summary>
    Task<ProposalsByCompanyReportDto> GetByCompanyReportAsync(DateTime? fromDate, DateTime? toDate, int top = 10);

    /// <summary>
    /// Get win/loss analysis with reasons
    /// </summary>
    Task<WinLossAnalysisReportDto> GetWinLossAnalysisAsync(DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Get monthly trends over specified number of months
    /// </summary>
    Task<MonthlyTrendsReportDto> GetMonthlyTrendsAsync(int months = 12);
}
