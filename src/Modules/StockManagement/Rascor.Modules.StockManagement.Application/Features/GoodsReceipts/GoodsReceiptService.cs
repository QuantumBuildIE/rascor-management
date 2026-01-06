using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly IStockManagementDbContext _context;

    public GoodsReceiptService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GoodsReceiptDto>>> GetAllAsync()
    {
        try
        {
            var receipts = await _context.GoodsReceipts
                .Include(gr => gr.Supplier)
                .Include(gr => gr.Location)
                .Include(gr => gr.PurchaseOrder)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.Product)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.BayLocation)
                .OrderByDescending(gr => gr.ReceiptDate)
                .ThenByDescending(gr => gr.GrnNumber)
                .Select(gr => new GoodsReceiptDto(
                    gr.Id,
                    gr.GrnNumber,
                    gr.PurchaseOrderId,
                    gr.PurchaseOrder != null ? gr.PurchaseOrder.PoNumber : null,
                    gr.SupplierId,
                    gr.Supplier.SupplierName,
                    gr.DeliveryNoteRef,
                    gr.LocationId,
                    gr.Location.LocationName,
                    gr.ReceiptDate,
                    gr.ReceivedBy,
                    gr.Notes,
                    gr.Lines.Select(l => new GoodsReceiptLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.PurchaseOrderLineId,
                        l.QuantityReceived,
                        l.Notes,
                        l.QuantityRejected,
                        l.RejectionReason,
                        l.BatchNumber,
                        l.ExpiryDate,
                        l.BayLocationId,
                        l.BayLocation != null ? l.BayLocation.BayCode : null
                    )).ToList()
                ))
                .ToListAsync();

            return Result.Ok(receipts);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<GoodsReceiptDto>>($"Error retrieving goods receipts: {ex.Message}");
        }
    }

    public async Task<Result<GoodsReceiptDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var receipt = await _context.GoodsReceipts
                .Include(gr => gr.Supplier)
                .Include(gr => gr.Location)
                .Include(gr => gr.PurchaseOrder)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.Product)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.BayLocation)
                .Where(gr => gr.Id == id)
                .Select(gr => new GoodsReceiptDto(
                    gr.Id,
                    gr.GrnNumber,
                    gr.PurchaseOrderId,
                    gr.PurchaseOrder != null ? gr.PurchaseOrder.PoNumber : null,
                    gr.SupplierId,
                    gr.Supplier.SupplierName,
                    gr.DeliveryNoteRef,
                    gr.LocationId,
                    gr.Location.LocationName,
                    gr.ReceiptDate,
                    gr.ReceivedBy,
                    gr.Notes,
                    gr.Lines.Select(l => new GoodsReceiptLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.PurchaseOrderLineId,
                        l.QuantityReceived,
                        l.Notes,
                        l.QuantityRejected,
                        l.RejectionReason,
                        l.BatchNumber,
                        l.ExpiryDate,
                        l.BayLocationId,
                        l.BayLocation != null ? l.BayLocation.BayCode : null
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (receipt == null)
            {
                return Result.Fail<GoodsReceiptDto>($"Goods receipt with ID {id} not found");
            }

            return Result.Ok(receipt);
        }
        catch (Exception ex)
        {
            return Result.Fail<GoodsReceiptDto>($"Error retrieving goods receipt: {ex.Message}");
        }
    }

    public async Task<Result<List<GoodsReceiptDto>>> GetBySupplierAsync(Guid supplierId)
    {
        try
        {
            var receipts = await _context.GoodsReceipts
                .Include(gr => gr.Supplier)
                .Include(gr => gr.Location)
                .Include(gr => gr.PurchaseOrder)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.Product)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.BayLocation)
                .Where(gr => gr.SupplierId == supplierId)
                .OrderByDescending(gr => gr.ReceiptDate)
                .ThenByDescending(gr => gr.GrnNumber)
                .Select(gr => new GoodsReceiptDto(
                    gr.Id,
                    gr.GrnNumber,
                    gr.PurchaseOrderId,
                    gr.PurchaseOrder != null ? gr.PurchaseOrder.PoNumber : null,
                    gr.SupplierId,
                    gr.Supplier.SupplierName,
                    gr.DeliveryNoteRef,
                    gr.LocationId,
                    gr.Location.LocationName,
                    gr.ReceiptDate,
                    gr.ReceivedBy,
                    gr.Notes,
                    gr.Lines.Select(l => new GoodsReceiptLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.PurchaseOrderLineId,
                        l.QuantityReceived,
                        l.Notes,
                        l.QuantityRejected,
                        l.RejectionReason,
                        l.BatchNumber,
                        l.ExpiryDate,
                        l.BayLocationId,
                        l.BayLocation != null ? l.BayLocation.BayCode : null
                    )).ToList()
                ))
                .ToListAsync();

            return Result.Ok(receipts);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<GoodsReceiptDto>>($"Error retrieving goods receipts by supplier: {ex.Message}");
        }
    }

    public async Task<Result<List<GoodsReceiptDto>>> GetByPurchaseOrderAsync(Guid purchaseOrderId)
    {
        try
        {
            var receipts = await _context.GoodsReceipts
                .Include(gr => gr.Supplier)
                .Include(gr => gr.Location)
                .Include(gr => gr.PurchaseOrder)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.Product)
                .Include(gr => gr.Lines)
                    .ThenInclude(l => l.BayLocation)
                .Where(gr => gr.PurchaseOrderId == purchaseOrderId)
                .OrderByDescending(gr => gr.ReceiptDate)
                .ThenByDescending(gr => gr.GrnNumber)
                .Select(gr => new GoodsReceiptDto(
                    gr.Id,
                    gr.GrnNumber,
                    gr.PurchaseOrderId,
                    gr.PurchaseOrder != null ? gr.PurchaseOrder.PoNumber : null,
                    gr.SupplierId,
                    gr.Supplier.SupplierName,
                    gr.DeliveryNoteRef,
                    gr.LocationId,
                    gr.Location.LocationName,
                    gr.ReceiptDate,
                    gr.ReceivedBy,
                    gr.Notes,
                    gr.Lines.Select(l => new GoodsReceiptLineDto(
                        l.Id,
                        l.ProductId,
                        l.Product.ProductCode,
                        l.Product.ProductName,
                        l.PurchaseOrderLineId,
                        l.QuantityReceived,
                        l.Notes,
                        l.QuantityRejected,
                        l.RejectionReason,
                        l.BatchNumber,
                        l.ExpiryDate,
                        l.BayLocationId,
                        l.BayLocation != null ? l.BayLocation.BayCode : null
                    )).ToList()
                ))
                .ToListAsync();

            return Result.Ok(receipts);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<GoodsReceiptDto>>($"Error retrieving goods receipts by purchase order: {ex.Message}");
        }
    }

    public async Task<Result<GoodsReceiptDto>> CreateAsync(CreateGoodsReceiptDto dto)
    {
        try
        {
            // Validate that SupplierId exists
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.Id == dto.SupplierId);

            if (!supplierExists)
            {
                return Result.Fail<GoodsReceiptDto>($"Supplier with ID {dto.SupplierId} not found");
            }

            // Validate that LocationId exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == dto.LocationId);

            if (!locationExists)
            {
                return Result.Fail<GoodsReceiptDto>($"Stock location with ID {dto.LocationId} not found");
            }

            // Validate PurchaseOrderId if provided
            PurchaseOrder? purchaseOrder = null;
            if (dto.PurchaseOrderId.HasValue)
            {
                purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.Lines)
                    .FirstOrDefaultAsync(po => po.Id == dto.PurchaseOrderId.Value);

                if (purchaseOrder == null)
                {
                    return Result.Fail<GoodsReceiptDto>($"Purchase order with ID {dto.PurchaseOrderId} not found");
                }

                if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled)
                {
                    return Result.Fail<GoodsReceiptDto>("Cannot receive goods against a cancelled purchase order");
                }

                if (purchaseOrder.Status == PurchaseOrderStatus.FullyReceived)
                {
                    return Result.Fail<GoodsReceiptDto>("Purchase order has already been fully received");
                }
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
                return Result.Fail<GoodsReceiptDto>($"Products not found: {string.Join(", ", missingProductIds)}");
            }

            // Generate GRN number
            var grnNumber = await GenerateGrnNumberAsync();

            var goodsReceipt = new GoodsReceipt
            {
                Id = Guid.NewGuid(),
                GrnNumber = grnNumber,
                PurchaseOrderId = dto.PurchaseOrderId,
                SupplierId = dto.SupplierId,
                DeliveryNoteRef = dto.DeliveryNoteRef,
                LocationId = dto.LocationId,
                ReceiptDate = DateTime.SpecifyKind(dto.ReceiptDate, DateTimeKind.Utc),
                ReceivedBy = dto.ReceivedBy,
                Notes = dto.Notes
            };

            _context.GoodsReceipts.Add(goodsReceipt);

            // Process each line
            foreach (var lineDto in dto.Lines)
            {
                var line = new GoodsReceiptLine
                {
                    Id = Guid.NewGuid(),
                    GoodsReceiptId = goodsReceipt.Id,
                    PurchaseOrderLineId = lineDto.PurchaseOrderLineId,
                    ProductId = lineDto.ProductId,
                    QuantityReceived = lineDto.QuantityReceived,
                    Notes = lineDto.Notes,
                    QuantityRejected = lineDto.QuantityRejected,
                    RejectionReason = lineDto.RejectionReason,
                    BatchNumber = lineDto.BatchNumber,
                    ExpiryDate = lineDto.ExpiryDate.HasValue
                        ? DateTime.SpecifyKind(lineDto.ExpiryDate.Value, DateTimeKind.Utc)
                        : null,
                    BayLocationId = lineDto.BayLocationId
                };
                _context.GoodsReceiptLines.Add(line);

                // Update or create StockLevel
                await UpdateStockLevelAsync(lineDto.ProductId, dto.LocationId, lineDto.QuantityReceived);

                // Create StockTransaction
                await CreateStockTransactionAsync(
                    lineDto.ProductId,
                    dto.LocationId,
                    lineDto.QuantityReceived,
                    goodsReceipt.Id,
                    grnNumber);

                // Update PurchaseOrderLine if linked
                if (lineDto.PurchaseOrderLineId.HasValue && purchaseOrder != null)
                {
                    await UpdatePurchaseOrderLineAsync(lineDto.PurchaseOrderLineId.Value, lineDto.QuantityReceived);
                }
            }

            // Update PurchaseOrder status if linked
            if (purchaseOrder != null)
            {
                await UpdatePurchaseOrderStatusAsync(purchaseOrder);
            }

            await _context.SaveChangesAsync();

            // Reload with related entities
            return await GetByIdAsync(goodsReceipt.Id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<GoodsReceiptDto>($"Error creating goods receipt: {innerMessage}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var receipt = await _context.GoodsReceipts
                .Include(gr => gr.Lines)
                .FirstOrDefaultAsync(gr => gr.Id == id);

            if (receipt == null)
            {
                return Result.Fail($"Goods receipt with ID {id} not found");
            }

            // Note: In a real system, you might want to reverse the stock movements
            // For now, we just soft delete without reversing
            receipt.IsDeleted = true;
            foreach (var line in receipt.Lines)
            {
                line.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting goods receipt: {ex.Message}");
        }
    }

    private async Task<string> GenerateGrnNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");
        var prefix = $"GRN-{datePrefix}-";

        var lastReceipt = await _context.GoodsReceipts
            .Where(gr => gr.GrnNumber.StartsWith(prefix))
            .OrderByDescending(gr => gr.GrnNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastReceipt != null)
        {
            var lastNumber = lastReceipt.GrnNumber;
            var sequencePart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D3}";
    }

    private async Task UpdateStockLevelAsync(Guid productId, Guid locationId, int quantityReceived)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.LocationId == locationId);

        if (stockLevel == null)
        {
            // Create new stock level
            stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                LocationId = locationId,
                QuantityOnHand = quantityReceived,
                QuantityReserved = 0,
                QuantityOnOrder = 0,
                LastMovementDate = DateTime.UtcNow
            };
            _context.StockLevels.Add(stockLevel);
        }
        else
        {
            // Update existing stock level
            stockLevel.QuantityOnHand += quantityReceived;
            stockLevel.LastMovementDate = DateTime.UtcNow;
        }
    }

    private async Task CreateStockTransactionAsync(
        Guid productId,
        Guid locationId,
        int quantity,
        Guid grnId,
        string grnNumber)
    {
        var transactionNumber = await GenerateTransactionNumberAsync();

        var transaction = new StockTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.GrnReceipt,
            ProductId = productId,
            LocationId = locationId,
            Quantity = quantity,
            ReferenceType = "GRN",
            ReferenceId = grnId,
            Notes = $"Received via {grnNumber}"
        };

        _context.StockTransactions.Add(transaction);
    }

    private async Task<string> GenerateTransactionNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");
        var prefix = $"TXN-{datePrefix}-";

        var lastTransaction = await _context.StockTransactions
            .Where(st => st.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(st => st.TransactionNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastTransaction != null)
        {
            var lastNumber = lastTransaction.TransactionNumber;
            var sequencePart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D3}";
    }

    private async Task UpdatePurchaseOrderLineAsync(Guid poLineId, int quantityReceived)
    {
        var poLine = await _context.PurchaseOrderLines
            .FirstOrDefaultAsync(pol => pol.Id == poLineId);

        if (poLine != null)
        {
            poLine.QuantityReceived += quantityReceived;

            // Update line status
            if (poLine.QuantityReceived >= poLine.QuantityOrdered)
            {
                poLine.LineStatus = PurchaseOrderLineStatus.Complete;
            }
            else if (poLine.QuantityReceived > 0)
            {
                poLine.LineStatus = PurchaseOrderLineStatus.Partial;
            }
        }
    }

    private async Task UpdatePurchaseOrderStatusAsync(PurchaseOrder purchaseOrder)
    {
        // Reload lines to get updated quantities
        var lines = await _context.PurchaseOrderLines
            .Where(pol => pol.PurchaseOrderId == purchaseOrder.Id)
            .ToListAsync();

        var allComplete = lines.All(l => l.LineStatus == PurchaseOrderLineStatus.Complete || l.LineStatus == PurchaseOrderLineStatus.Cancelled);
        var anyPartial = lines.Any(l => l.LineStatus == PurchaseOrderLineStatus.Partial || l.LineStatus == PurchaseOrderLineStatus.Complete);

        if (allComplete)
        {
            purchaseOrder.Status = PurchaseOrderStatus.FullyReceived;
        }
        else if (anyPartial)
        {
            purchaseOrder.Status = PurchaseOrderStatus.PartiallyReceived;
        }
    }
}
