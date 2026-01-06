using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Reports;

/// <summary>
/// Service for stock reports and analytics
/// </summary>
public interface IStockReportService
{
    /// <summary>
    /// Get top products by value for each month (from collected stock orders)
    /// </summary>
    /// <param name="months">Number of months to include (default 4)</param>
    /// <param name="topN">Number of top products per month (default 10)</param>
    Task<Result<List<ProductValueByMonthDto>>> GetTopProductsByMonthAsync(int months = 4, int topN = 10);

    /// <summary>
    /// Get top products by value for each site (from collected stock orders)
    /// </summary>
    /// <param name="topN">Number of top products (default 10)</param>
    Task<Result<List<ProductValueBySiteDto>>> GetTopProductsBySiteAsync(int topN = 10);

    /// <summary>
    /// Get top products by value for each week (from collected stock orders)
    /// </summary>
    /// <param name="weeks">Number of weeks to include (default 12)</param>
    /// <param name="topN">Number of top products (default 10)</param>
    Task<Result<List<ProductValueByWeekDto>>> GetTopProductsByWeekAsync(int weeks = 12, int topN = 10);

    /// <summary>
    /// Get stock valuation report showing current stock value by product and location
    /// </summary>
    /// <param name="locationId">Optional location filter</param>
    /// <param name="categoryId">Optional category filter</param>
    Task<Result<StockValuationReportDto>> GetStockValuationAsync(Guid? locationId = null, Guid? categoryId = null);
}
