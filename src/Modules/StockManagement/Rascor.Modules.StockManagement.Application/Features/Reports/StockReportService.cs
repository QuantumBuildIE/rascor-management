using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.Reports;

public class StockReportService : IStockReportService
{
    private readonly IStockManagementDbContext _context;

    public StockReportService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductValueByMonthDto>>> GetTopProductsByMonthAsync(int months = 4, int topN = 10)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months + 1);
            startDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Get all collected orders within the date range
            var collectedOrders = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Where(so => so.Status == StockOrderStatus.Collected)
                .Where(so => so.CollectedDate >= startDate)
                .ToListAsync();

            // Group by month and product, calculate value
            var productValuesByMonth = collectedOrders
                .Where(so => so.CollectedDate.HasValue)
                .SelectMany(so => so.Lines.Select(l => new
                {
                    Month = so.CollectedDate!.Value.ToString("MMM yyyy"),
                    MonthDate = new DateTime(so.CollectedDate!.Value.Year, so.CollectedDate!.Value.Month, 1),
                    ProductName = l.Product.ProductName,
                    Value = l.QuantityIssued * l.UnitPrice
                }))
                .GroupBy(x => new { x.Month, x.MonthDate, x.ProductName })
                .Select(g => new
                {
                    g.Key.Month,
                    g.Key.MonthDate,
                    g.Key.ProductName,
                    TotalValue = g.Sum(x => x.Value)
                })
                .ToList();

            // Get top N products across all months
            var topProducts = productValuesByMonth
                .GroupBy(x => x.ProductName)
                .Select(g => new { ProductName = g.Key, TotalValue = g.Sum(x => x.TotalValue) })
                .OrderByDescending(x => x.TotalValue)
                .Take(topN)
                .Select(x => x.ProductName)
                .ToHashSet();

            // Filter to only include top products, ordered by month
            var result = productValuesByMonth
                .Where(x => topProducts.Contains(x.ProductName))
                .OrderBy(x => x.MonthDate)
                .ThenBy(x => x.ProductName)
                .Select(x => new ProductValueByMonthDto(x.Month, x.ProductName, x.TotalValue))
                .ToList();

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<ProductValueByMonthDto>>($"Error retrieving products by month: {ex.Message}");
        }
    }

    public async Task<Result<List<ProductValueBySiteDto>>> GetTopProductsBySiteAsync(int topN = 10)
    {
        try
        {
            // Get all collected orders
            var collectedOrders = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Where(so => so.Status == StockOrderStatus.Collected)
                .ToListAsync();

            // Group by site and product, calculate value
            var productValuesBySite = collectedOrders
                .SelectMany(so => so.Lines.Select(l => new
                {
                    SiteName = so.SiteName,
                    ProductName = l.Product.ProductName,
                    Value = l.QuantityIssued * l.UnitPrice
                }))
                .GroupBy(x => new { x.SiteName, x.ProductName })
                .Select(g => new
                {
                    g.Key.SiteName,
                    g.Key.ProductName,
                    TotalValue = g.Sum(x => x.Value)
                })
                .ToList();

            // Get top N products across all sites
            var topProducts = productValuesBySite
                .GroupBy(x => x.ProductName)
                .Select(g => new { ProductName = g.Key, TotalValue = g.Sum(x => x.TotalValue) })
                .OrderByDescending(x => x.TotalValue)
                .Take(topN)
                .Select(x => x.ProductName)
                .ToHashSet();

            // Filter to only include top products, ordered by site
            var result = productValuesBySite
                .Where(x => topProducts.Contains(x.ProductName))
                .OrderBy(x => x.SiteName)
                .ThenBy(x => x.ProductName)
                .Select(x => new ProductValueBySiteDto(x.SiteName, x.ProductName, x.TotalValue))
                .ToList();

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<ProductValueBySiteDto>>($"Error retrieving products by site: {ex.Message}");
        }
    }

    public async Task<Result<List<ProductValueByWeekDto>>> GetTopProductsByWeekAsync(int weeks = 12, int topN = 10)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-weeks * 7);
            // Align to start of week (Monday)
            var daysToMonday = ((int)startDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            startDate = startDate.AddDays(-daysToMonday).Date;
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            // Get all collected orders within the date range
            var collectedOrders = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Where(so => so.Status == StockOrderStatus.Collected)
                .Where(so => so.CollectedDate >= startDate)
                .ToListAsync();

            // Group by week and product, calculate value
            var productValuesByWeek = collectedOrders
                .Where(so => so.CollectedDate.HasValue)
                .SelectMany(so => so.Lines.Select(l => new
                {
                    WeekStartDate = GetWeekStartDate(so.CollectedDate!.Value),
                    ProductName = l.Product.ProductName,
                    Value = l.QuantityIssued * l.UnitPrice
                }))
                .GroupBy(x => new { x.WeekStartDate, x.ProductName })
                .Select(g => new
                {
                    g.Key.WeekStartDate,
                    g.Key.ProductName,
                    TotalValue = g.Sum(x => x.Value)
                })
                .ToList();

            // Get top N products across all weeks
            var topProducts = productValuesByWeek
                .GroupBy(x => x.ProductName)
                .Select(g => new { ProductName = g.Key, TotalValue = g.Sum(x => x.TotalValue) })
                .OrderByDescending(x => x.TotalValue)
                .Take(topN)
                .Select(x => x.ProductName)
                .ToHashSet();

            // Filter to only include top products, ordered by week
            var result = productValuesByWeek
                .Where(x => topProducts.Contains(x.ProductName))
                .OrderBy(x => x.WeekStartDate)
                .ThenBy(x => x.ProductName)
                .Select(x => new ProductValueByWeekDto(x.WeekStartDate, x.ProductName, x.TotalValue))
                .ToList();

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<ProductValueByWeekDto>>($"Error retrieving products by week: {ex.Message}");
        }
    }

    private static DateTime GetWeekStartDate(DateTime date)
    {
        var daysToMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return DateTime.SpecifyKind(date.AddDays(-daysToMonday).Date, DateTimeKind.Utc);
    }

    public async Task<Result<StockValuationReportDto>> GetStockValuationAsync(Guid? locationId = null, Guid? categoryId = null)
    {
        try
        {
            // Build query for stock levels with positive quantities
            var query = _context.StockLevels
                .Include(sl => sl.Product)
                    .ThenInclude(p => p.Category)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.QuantityOnHand > 0);

            // Apply location filter if specified
            if (locationId.HasValue)
            {
                query = query.Where(sl => sl.LocationId == locationId.Value);
            }

            // Apply category filter if specified
            if (categoryId.HasValue)
            {
                query = query.Where(sl => sl.Product.CategoryId == categoryId.Value);
            }

            var stockLevels = await query.ToListAsync();

            // Map to DTOs and calculate values
            var items = stockLevels
                .Select(sl => new StockValuationItemDto(
                    sl.ProductId,
                    sl.Product.ProductCode,
                    sl.Product.ProductName,
                    sl.Product.CategoryId,
                    sl.Product.Category?.CategoryName,
                    sl.LocationId,
                    sl.Location.LocationName,
                    sl.BayLocation?.BayCode,
                    sl.QuantityOnHand,
                    sl.Product.CostPrice,
                    sl.QuantityOnHand * (sl.Product.CostPrice ?? 0)
                ))
                .OrderBy(i => i.LocationName)
                .ThenBy(i => i.CategoryName ?? "")
                .ThenBy(i => i.ProductName)
                .ToList();

            var report = new StockValuationReportDto(
                items,
                items.Count,
                items.Sum(i => i.QuantityOnHand),
                items.Sum(i => i.TotalValue),
                DateTime.UtcNow
            );

            return Result.Ok(report);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockValuationReportDto>($"Error retrieving stock valuation: {ex.Message}");
        }
    }
}
