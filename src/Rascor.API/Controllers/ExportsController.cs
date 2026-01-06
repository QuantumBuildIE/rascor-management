using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.StockManagement.Application.Interfaces;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/exports")]
[Authorize(Policy = "StockManagement.View")]
public class ExportsController : ControllerBase
{
    private readonly IStockManagementDbContext _context;
    private readonly IExportService _exportService;
    private readonly ILogger<ExportsController> _logger;

    public ExportsController(
        IStockManagementDbContext context,
        IExportService exportService,
        ILogger<ExportsController> logger)
    {
        _context = context;
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Export products to Excel or PDF
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> ExportProducts([FromQuery] string format = "excel")
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProductCode)
                .Select(p => new
                {
                    p.ProductCode,
                    ProductName = p.ProductName,
                    Category = p.Category != null ? p.Category.CategoryName : "",
                    Supplier = p.Supplier != null ? p.Supplier.SupplierName : "",
                    p.UnitType,
                    BaseRate = p.BaseRate,
                    CostPrice = p.CostPrice.HasValue ? p.CostPrice.Value : 0m,
                    SellPrice = p.SellPrice.HasValue ? p.SellPrice.Value : 0m,
                    ReorderLevel = p.ReorderLevel,
                    Active = p.IsActive ? "Yes" : "No"
                })
                .ToListAsync();

            var columns = new Dictionary<string, Func<dynamic, object>>
            {
                { "Product Code", d => d.ProductCode },
                { "Product Name", d => d.ProductName },
                { "Category", d => d.Category },
                { "Supplier", d => d.Supplier },
                { "Unit Type", d => d.UnitType },
                { "Base Rate", d => d.BaseRate },
                { "Cost Price", d => d.CostPrice },
                { "Sell Price", d => d.SellPrice },
                { "Reorder Level", d => d.ReorderLevel },
                { "Active", d => d.Active }
            };

            byte[] fileBytes;
            string fileName;
            string contentType;

            if (format.ToLower() == "pdf")
            {
                fileBytes = _exportService.ExportToPdf(products, "Products Report", columns);
                fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                fileBytes = _exportService.ExportToExcel(products, "Products", columns);
                fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting products");
            return StatusCode(500, new { Success = false, Message = "Error exporting products" });
        }
    }

    /// <summary>
    /// Export stock levels to Excel or PDF
    /// </summary>
    [HttpGet("stock-levels")]
    public async Task<IActionResult> ExportStockLevels(
        [FromQuery] string format = "excel",
        [FromQuery] Guid? locationId = null)
    {
        try
        {
            var query = _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => !sl.IsDeleted);

            if (locationId.HasValue)
            {
                query = query.Where(sl => sl.LocationId == locationId.Value);
            }

            var stockLevels = await query
                .OrderBy(sl => sl.Product.ProductCode)
                .Select(sl => new
                {
                    ProductCode = sl.Product.ProductCode,
                    ProductName = sl.Product.ProductName,
                    Location = sl.Location.LocationName,
                    Bay = sl.BayLocation != null ? sl.BayLocation.BayCode : "",
                    QtyOnHand = sl.QuantityOnHand,
                    QtyReserved = sl.QuantityReserved,
                    QtyAvailable = sl.QuantityOnHand - sl.QuantityReserved
                })
                .ToListAsync();

            var columns = new Dictionary<string, Func<dynamic, object>>
            {
                { "Product Code", d => d.ProductCode },
                { "Product Name", d => d.ProductName },
                { "Location", d => d.Location },
                { "Bay", d => d.Bay },
                { "Qty On Hand", d => d.QtyOnHand },
                { "Qty Reserved", d => d.QtyReserved },
                { "Qty Available", d => d.QtyAvailable }
            };

            byte[] fileBytes;
            string fileName;
            string contentType;

            if (format.ToLower() == "pdf")
            {
                fileBytes = _exportService.ExportToPdf(stockLevels, "Stock Levels Report", columns);
                fileName = $"StockLevels_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                fileBytes = _exportService.ExportToExcel(stockLevels, "Stock Levels", columns);
                fileName = $"StockLevels_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting stock levels");
            return StatusCode(500, new { Success = false, Message = "Error exporting stock levels" });
        }
    }

    /// <summary>
    /// Export stock orders to Excel or PDF
    /// </summary>
    [HttpGet("stock-orders")]
    public async Task<IActionResult> ExportStockOrders(
        [FromQuery] string format = "excel",
        [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.StockOrders
                .Include(so => so.SourceLocation)
                .Where(so => !so.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(so => so.Status.ToString() == status);
            }

            var stockOrders = await query
                .OrderByDescending(so => so.OrderDate)
                .Select(so => new
                {
                    OrderNumber = so.OrderNumber,
                    OrderDate = so.OrderDate,
                    Site = so.SiteName,
                    SourceLocation = so.SourceLocation.LocationName,
                    Status = so.Status.ToString(),
                    RequestedBy = so.RequestedBy,
                    Total = so.OrderTotal
                })
                .ToListAsync();

            var columns = new Dictionary<string, Func<dynamic, object>>
            {
                { "Order Number", d => d.OrderNumber },
                { "Order Date", d => d.OrderDate },
                { "Site", d => d.Site },
                { "Source Location", d => d.SourceLocation },
                { "Status", d => d.Status },
                { "Requested By", d => d.RequestedBy },
                { "Total", d => d.Total }
            };

            byte[] fileBytes;
            string fileName;
            string contentType;

            if (format.ToLower() == "pdf")
            {
                fileBytes = _exportService.ExportToPdf(stockOrders, "Stock Orders Report", columns);
                fileName = $"StockOrders_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                fileBytes = _exportService.ExportToExcel(stockOrders, "Stock Orders", columns);
                fileName = $"StockOrders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting stock orders");
            return StatusCode(500, new { Success = false, Message = "Error exporting stock orders" });
        }
    }

    /// <summary>
    /// Export purchase orders to Excel or PDF
    /// </summary>
    [HttpGet("purchase-orders")]
    public async Task<IActionResult> ExportPurchaseOrders([FromQuery] string format = "excel")
    {
        try
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => !po.IsDeleted)
                .OrderByDescending(po => po.OrderDate)
                .Select(po => new
                {
                    PONumber = po.PoNumber,
                    OrderDate = po.OrderDate,
                    Supplier = po.Supplier.SupplierName,
                    Status = po.Status,
                    TotalValue = po.TotalValue
                })
                .ToListAsync();

            var columns = new Dictionary<string, Func<dynamic, object>>
            {
                { "PO Number", d => d.PONumber },
                { "Order Date", d => d.OrderDate },
                { "Supplier", d => d.Supplier },
                { "Status", d => d.Status },
                { "Total Value", d => d.TotalValue }
            };

            byte[] fileBytes;
            string fileName;
            string contentType;

            if (format.ToLower() == "pdf")
            {
                fileBytes = _exportService.ExportToPdf(purchaseOrders, "Purchase Orders Report", columns);
                fileName = $"PurchaseOrders_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                fileBytes = _exportService.ExportToExcel(purchaseOrders, "Purchase Orders", columns);
                fileName = $"PurchaseOrders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting purchase orders");
            return StatusCode(500, new { Success = false, Message = "Error exporting purchase orders" });
        }
    }

    /// <summary>
    /// Export stock valuation report to Excel or PDF
    /// </summary>
    [HttpGet("stock-valuation")]
    [Authorize(Policy = "StockManagement.ViewCostings")]
    public async Task<IActionResult> ExportStockValuation(
        [FromQuery] string format = "excel",
        [FromQuery] Guid? locationId = null,
        [FromQuery] Guid? categoryId = null)
    {
        try
        {
            var query = _context.StockLevels
                .Include(sl => sl.Product)
                    .ThenInclude(p => p.Category)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => !sl.IsDeleted && sl.QuantityOnHand > 0);

            if (locationId.HasValue)
            {
                query = query.Where(sl => sl.LocationId == locationId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(sl => sl.Product.CategoryId == categoryId.Value);
            }

            var stockValuation = await query
                .Select(sl => new
                {
                    ProductCode = sl.Product.ProductCode,
                    ProductName = sl.Product.ProductName,
                    Category = sl.Product.Category != null ? sl.Product.Category.CategoryName : "",
                    Location = sl.Location.LocationName,
                    Bay = sl.BayLocation != null ? sl.BayLocation.BayCode : "",
                    QtyOnHand = sl.QuantityOnHand,
                    CostPrice = sl.Product.CostPrice.HasValue ? sl.Product.CostPrice.Value : 0m,
                    TotalValue = sl.QuantityOnHand * (sl.Product.CostPrice.HasValue ? sl.Product.CostPrice.Value : 0m)
                })
                .OrderByDescending(x => x.TotalValue)
                .ToListAsync();

            var columns = new Dictionary<string, Func<dynamic, object>>
            {
                { "Product Code", d => d.ProductCode },
                { "Product Name", d => d.ProductName },
                { "Category", d => d.Category },
                { "Location", d => d.Location },
                { "Bay", d => d.Bay },
                { "Qty On Hand", d => d.QtyOnHand },
                { "Cost Price", d => d.CostPrice },
                { "Total Value", d => d.TotalValue }
            };

            byte[] fileBytes;
            string fileName;
            string contentType;

            if (format.ToLower() == "pdf")
            {
                fileBytes = _exportService.ExportToPdf(stockValuation, "Stock Valuation Report", columns);
                fileName = $"StockValuation_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                fileBytes = _exportService.ExportToExcel(stockValuation, "Stock Valuation", columns);
                fileName = $"StockValuation_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting stock valuation");
            return StatusCode(500, new { Success = false, Message = "Error exporting stock valuation" });
        }
    }
}
