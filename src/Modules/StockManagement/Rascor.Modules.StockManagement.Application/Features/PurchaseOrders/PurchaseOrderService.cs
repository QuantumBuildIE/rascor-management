using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IStockManagementDbContext _context;

    public PurchaseOrderService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PurchaseOrderDto>>> GetAllAsync()
    {
        try
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Lines)
                    .ThenInclude(l => l.Product)
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.PoNumber)
                .Select(po => new PurchaseOrderDto(
                    po.Id,
                    po.PoNumber,
                    po.SupplierId,
                    po.Supplier.SupplierName,
                    po.OrderDate,
                    po.ExpectedDate,
                    po.Status.ToString(),
                    po.TotalValue,
                    po.Notes,
                    po.Lines.Select(l => new PurchaseOrderLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.QuantityOrdered,
                        l.QuantityReceived,
                        l.UnitPrice,
                        l.QuantityOrdered * l.UnitPrice,
                        l.LineStatus.ToString()
                    )).ToList()
                ))
                .ToListAsync();

            return Result.Ok(purchaseOrders);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<PurchaseOrderDto>>($"Error retrieving purchase orders: {ex.Message}");
        }
    }

    public async Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Lines)
                    .ThenInclude(l => l.Product)
                .Where(po => po.Id == id)
                .Select(po => new PurchaseOrderDto(
                    po.Id,
                    po.PoNumber,
                    po.SupplierId,
                    po.Supplier.SupplierName,
                    po.OrderDate,
                    po.ExpectedDate,
                    po.Status.ToString(),
                    po.TotalValue,
                    po.Notes,
                    po.Lines.Select(l => new PurchaseOrderLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.QuantityOrdered,
                        l.QuantityReceived,
                        l.UnitPrice,
                        l.QuantityOrdered * l.UnitPrice,
                        l.LineStatus.ToString()
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (purchaseOrder == null)
            {
                return Result.Fail<PurchaseOrderDto>($"Purchase order with ID {id} not found");
            }

            return Result.Ok(purchaseOrder);
        }
        catch (Exception ex)
        {
            return Result.Fail<PurchaseOrderDto>($"Error retrieving purchase order: {ex.Message}");
        }
    }

    public async Task<Result<List<PurchaseOrderDto>>> GetBySupplierAsync(Guid supplierId)
    {
        try
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Lines)
                    .ThenInclude(l => l.Product)
                .Where(po => po.SupplierId == supplierId)
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.PoNumber)
                .Select(po => new PurchaseOrderDto(
                    po.Id,
                    po.PoNumber,
                    po.SupplierId,
                    po.Supplier.SupplierName,
                    po.OrderDate,
                    po.ExpectedDate,
                    po.Status.ToString(),
                    po.TotalValue,
                    po.Notes,
                    po.Lines.Select(l => new PurchaseOrderLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.QuantityOrdered,
                        l.QuantityReceived,
                        l.UnitPrice,
                        l.QuantityOrdered * l.UnitPrice,
                        l.LineStatus.ToString()
                    )).ToList()
                ))
                .ToListAsync();

            return Result.Ok(purchaseOrders);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<PurchaseOrderDto>>($"Error retrieving purchase orders by supplier: {ex.Message}");
        }
    }

    public async Task<Result<List<PurchaseOrderDto>>> GetByStatusAsync(string status)
    {
        try
        {
            if (!Enum.TryParse<PurchaseOrderStatus>(status, ignoreCase: true, out var orderStatus))
            {
                return Result.Fail<List<PurchaseOrderDto>>($"Invalid status: {status}");
            }

            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Lines)
                    .ThenInclude(l => l.Product)
                .Where(po => po.Status == orderStatus)
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.PoNumber)
                .Select(po => new PurchaseOrderDto(
                    po.Id,
                    po.PoNumber,
                    po.SupplierId,
                    po.Supplier.SupplierName,
                    po.OrderDate,
                    po.ExpectedDate,
                    po.Status.ToString(),
                    po.TotalValue,
                    po.Notes,
                    po.Lines.Select(l => new PurchaseOrderLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.QuantityOrdered,
                        l.QuantityReceived,
                        l.UnitPrice,
                        l.QuantityOrdered * l.UnitPrice,
                        l.LineStatus.ToString()
                    )).ToList()
                ))
                .ToListAsync();

            return Result.Ok(purchaseOrders);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<PurchaseOrderDto>>($"Error retrieving purchase orders by status: {ex.Message}");
        }
    }

    public async Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto dto)
    {
        try
        {
            // Validate that SupplierId exists
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.Id == dto.SupplierId);

            if (!supplierExists)
            {
                return Result.Fail<PurchaseOrderDto>($"Supplier with ID {dto.SupplierId} not found");
            }

            // Validate all product IDs exist
            var productIds = dto.Lines.Select(l => l.ProductId).Distinct().ToList();
            var existingProductIds = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingProductIds = productIds.Except(existingProductIds).ToList();
            if (missingProductIds.Any())
            {
                return Result.Fail<PurchaseOrderDto>($"Products not found: {string.Join(", ", missingProductIds)}");
            }

            // Generate PO number
            var poNumber = await GeneratePoNumberAsync();

            // Calculate total value
            var totalValue = dto.Lines.Sum(l => l.QuantityOrdered * l.UnitPrice);

            var purchaseOrder = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                PoNumber = poNumber,
                SupplierId = dto.SupplierId,
                OrderDate = DateTime.SpecifyKind(dto.OrderDate, DateTimeKind.Utc),
                ExpectedDate = dto.ExpectedDate.HasValue
                    ? DateTime.SpecifyKind(dto.ExpectedDate.Value, DateTimeKind.Utc)
                    : null,
                Status = PurchaseOrderStatus.Draft,
                TotalValue = totalValue,
                Notes = dto.Notes
            };

            _context.PurchaseOrders.Add(purchaseOrder);

            // Create lines - add them after the parent is tracked so EF properly cascades
            foreach (var lineDto in dto.Lines)
            {
                var line = new PurchaseOrderLine
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrder.Id,
                    ProductId = lineDto.ProductId,
                    QuantityOrdered = lineDto.QuantityOrdered,
                    QuantityReceived = 0,
                    UnitPrice = lineDto.UnitPrice,
                    LineStatus = PurchaseOrderLineStatus.Open
                };
                _context.PurchaseOrderLines.Add(line);
            }

            await _context.SaveChangesAsync();

            // Reload with related entities
            return await GetByIdAsync(purchaseOrder.Id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<PurchaseOrderDto>($"Error creating purchase order: {innerMessage}");
        }
    }

    public async Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto)
    {
        try
        {
            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return Result.Fail<PurchaseOrderDto>($"Purchase order with ID {id} not found");
            }

            // Only allow updates to Draft orders
            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
            {
                return Result.Fail<PurchaseOrderDto>($"Cannot update purchase order in status {purchaseOrder.Status}");
            }

            purchaseOrder.ExpectedDate = dto.ExpectedDate;
            purchaseOrder.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return Result.Fail<PurchaseOrderDto>($"Error updating purchase order: {ex.Message}");
        }
    }

    public async Task<Result<PurchaseOrderDto>> ConfirmAsync(Guid id)
    {
        try
        {
            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return Result.Fail<PurchaseOrderDto>($"Purchase order with ID {id} not found");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
            {
                return Result.Fail<PurchaseOrderDto>($"Cannot confirm purchase order in status {purchaseOrder.Status}. Only Draft orders can be confirmed.");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Confirmed;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return Result.Fail<PurchaseOrderDto>($"Error confirming purchase order: {ex.Message}");
        }
    }

    public async Task<Result<PurchaseOrderDto>> CancelAsync(Guid id)
    {
        try
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Lines)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return Result.Fail<PurchaseOrderDto>($"Purchase order with ID {id} not found");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.FullyReceived)
            {
                return Result.Fail<PurchaseOrderDto>("Cannot cancel a fully received purchase order");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled)
            {
                return Result.Fail<PurchaseOrderDto>("Purchase order is already cancelled");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;

            // Cancel all open lines
            foreach (var line in purchaseOrder.Lines.Where(l => l.LineStatus == PurchaseOrderLineStatus.Open))
            {
                line.LineStatus = PurchaseOrderLineStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return Result.Fail<PurchaseOrderDto>($"Error cancelling purchase order: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Lines)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return Result.Fail($"Purchase order with ID {id} not found");
            }

            // Only allow deletion of Draft orders
            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
            {
                return Result.Fail($"Cannot delete purchase order in status {purchaseOrder.Status}. Only Draft orders can be deleted.");
            }

            purchaseOrder.IsDeleted = true;
            foreach (var line in purchaseOrder.Lines)
            {
                line.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting purchase order: {ex.Message}");
        }
    }

    private async Task<string> GeneratePoNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");
        var prefix = $"PO-{datePrefix}-";

        // Find the highest sequence number for today
        var lastOrder = await _context.PurchaseOrders
            .Where(po => po.PoNumber.StartsWith(prefix))
            .OrderByDescending(po => po.PoNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastOrder != null)
        {
            var lastNumber = lastOrder.PoNumber;
            var sequencePart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D3}";
    }
}
