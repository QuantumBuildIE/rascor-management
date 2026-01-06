using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.Reports;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/stock/reports")]
[Authorize(Policy = "StockManagement.View")]
public class StockReportsController : ControllerBase
{
    private readonly IStockReportService _stockReportService;

    public StockReportsController(IStockReportService stockReportService)
    {
        _stockReportService = stockReportService;
    }

    /// <summary>
    /// Get top products by value for each month (from collected stock orders)
    /// </summary>
    /// <param name="months">Number of months to include (default 4)</param>
    /// <param name="topN">Number of top products (default 10)</param>
    /// <returns>List of product values by month</returns>
    [HttpGet("products-by-month")]
    public async Task<IActionResult> GetProductsByMonth([FromQuery] int months = 4, [FromQuery] int topN = 10)
    {
        var result = await _stockReportService.GetTopProductsByMonthAsync(months, topN);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get top products by value for each site (from collected stock orders)
    /// </summary>
    /// <param name="topN">Number of top products (default 10)</param>
    /// <returns>List of product values by site</returns>
    [HttpGet("products-by-site")]
    public async Task<IActionResult> GetProductsBySite([FromQuery] int topN = 10)
    {
        var result = await _stockReportService.GetTopProductsBySiteAsync(topN);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get top products by value for each week (from collected stock orders)
    /// </summary>
    /// <param name="weeks">Number of weeks to include (default 12)</param>
    /// <param name="topN">Number of top products (default 10)</param>
    /// <returns>List of product values by week</returns>
    [HttpGet("products-by-week")]
    public async Task<IActionResult> GetProductsByWeek([FromQuery] int weeks = 12, [FromQuery] int topN = 10)
    {
        var result = await _stockReportService.GetTopProductsByWeekAsync(weeks, topN);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock valuation report showing current stock value by product and location
    /// </summary>
    /// <param name="locationId">Optional filter by location</param>
    /// <param name="categoryId">Optional filter by category</param>
    /// <returns>Stock valuation report with items and totals</returns>
    [HttpGet("valuation")]
    [Authorize(Policy = "StockManagement.ViewCostings")]
    public async Task<IActionResult> GetStockValuation([FromQuery] Guid? locationId = null, [FromQuery] Guid? categoryId = null)
    {
        var result = await _stockReportService.GetStockValuationAsync(locationId, categoryId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
