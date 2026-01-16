using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.StockOrders;

public class StockOrderService : IStockOrderService
{
    private readonly IStockManagementDbContext _context;

    public StockOrderService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StockOrderDto>>> GetAllAsync()
    {
        try
        {
            var orders = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Include(so => so.SourceLocation)
                .OrderByDescending(so => so.OrderDate)
                .ThenByDescending(so => so.OrderNumber)
                .Select(so => MapToDto(so))
                .ToListAsync();

            return Result.Ok(orders);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockOrderDto>>($"Error retrieving stock orders: {ex.Message}");
        }
    }

    public async Task<Result<StockOrderDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Include(so => so.SourceLocation)
                .Where(so => so.Id == id)
                .Select(so => MapToDto(so))
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            return Result.Ok(order);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockOrderDto>($"Error retrieving stock order: {ex.Message}");
        }
    }

    public async Task<Result<StockOrderDto>> GetForDocketAsync(Guid id, Guid warehouseLocationId)
    {
        try
        {
            var order = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Include(so => so.SourceLocation)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            // Get bay locations for all products at the source warehouse location
            var productIds = order.Lines.Select(l => l.ProductId).ToList();
            var stockLevels = await _context.StockLevels
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.LocationId == order.SourceLocationId && productIds.Contains(sl.ProductId))
                .ToListAsync();

            var bayCodesByProduct = stockLevels.ToDictionary(
                sl => sl.ProductId,
                sl => sl.BayLocation?.BayCode
            );

            // Map to DTO with bay codes, sorted by bay code for picking efficiency
            var lines = order.Lines
                .Select(l => new StockOrderLineDto(
                    l.Id,
                    l.ProductId,
                    l.Product.ProductCode,
                    l.Product.ProductName,
                    l.QuantityRequested,
                    l.QuantityIssued,
                    l.UnitPrice,
                    l.QuantityRequested * l.UnitPrice,
                    bayCodesByProduct.GetValueOrDefault(l.ProductId)
                ))
                .OrderBy(l => l.BayCode ?? "zzz")
                .ThenBy(l => l.ProductCode)
                .ToList();

            var dto = new StockOrderDto(
                order.Id,
                order.OrderNumber,
                order.SiteId,
                order.SiteName,
                order.OrderDate,
                order.RequiredDate,
                order.Status.ToString(),
                order.OrderTotal,
                order.RequestedBy,
                order.ApprovedBy,
                order.ApprovedDate,
                order.CollectedDate,
                order.Notes,
                order.SourceLocationId,
                order.SourceLocation.LocationName,
                lines
            );

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockOrderDto>($"Error retrieving stock order for docket: {ex.Message}");
        }
    }

    public async Task<Result<List<StockOrderDto>>> GetBySiteAsync(Guid siteId)
    {
        try
        {
            var orders = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Include(so => so.SourceLocation)
                .Where(so => so.SiteId == siteId)
                .OrderByDescending(so => so.OrderDate)
                .ThenByDescending(so => so.OrderNumber)
                .Select(so => MapToDto(so))
                .ToListAsync();

            return Result.Ok(orders);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockOrderDto>>($"Error retrieving stock orders by site: {ex.Message}");
        }
    }

    public async Task<Result<List<StockOrderDto>>> GetByStatusAsync(string status)
    {
        try
        {
            if (!Enum.TryParse<StockOrderStatus>(status, true, out var statusEnum))
            {
                return Result.Fail<List<StockOrderDto>>($"Invalid status: {status}. Valid values are: {string.Join(", ", Enum.GetNames<StockOrderStatus>())}");
            }

            var orders = await _context.StockOrders
                .Include(so => so.Lines)
                    .ThenInclude(l => l.Product)
                .Include(so => so.SourceLocation)
                .Where(so => so.Status == statusEnum)
                .OrderByDescending(so => so.OrderDate)
                .ThenByDescending(so => so.OrderNumber)
                .Select(so => MapToDto(so))
                .ToListAsync();

            return Result.Ok(orders);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockOrderDto>>($"Error retrieving stock orders by status: {ex.Message}");
        }
    }

    public async Task<Result<StockOrderDto>> CreateAsync(CreateStockOrderDto dto)
    {
        try
        {
            // Validate source location exists and is active
            var sourceLocation = await _context.StockLocations
                .FirstOrDefaultAsync(l => l.Id == dto.SourceLocationId);

            if (sourceLocation == null)
            {
                return Result.Fail<StockOrderDto>($"Source location with ID {dto.SourceLocationId} not found");
            }

            // Validate all product IDs exist and get their prices
            var productIds = dto.Lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            var missingProductIds = productIds.Except(products.Keys).ToList();
            if (missingProductIds.Any())
            {
                return Result.Fail<StockOrderDto>($"Products not found: {string.Join(", ", missingProductIds)}");
            }

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Calculate total
            decimal orderTotal = 0;
            foreach (var lineDto in dto.Lines)
            {
                var product = products[lineDto.ProductId];
                orderTotal += lineDto.QuantityRequested * product.BaseRate;
            }

            var stockOrder = new StockOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                SiteId = dto.SiteId,
                SiteName = dto.SiteName,
                OrderDate = DateTime.SpecifyKind(dto.OrderDate, DateTimeKind.Utc),
                RequiredDate = dto.RequiredDate.HasValue
                    ? DateTime.SpecifyKind(dto.RequiredDate.Value, DateTimeKind.Utc)
                    : null,
                Status = StockOrderStatus.Draft,
                OrderTotal = orderTotal,
                RequestedBy = dto.RequestedBy,
                Notes = dto.Notes,
                SourceLocationId = dto.SourceLocationId,
                SourceProposalId = dto.SourceProposalId,
                SourceProposalNumber = dto.SourceProposalNumber
            };

            _context.StockOrders.Add(stockOrder);

            // Create lines
            foreach (var lineDto in dto.Lines)
            {
                var product = products[lineDto.ProductId];
                var line = new StockOrderLine
                {
                    Id = Guid.NewGuid(),
                    StockOrderId = stockOrder.Id,
                    ProductId = lineDto.ProductId,
                    QuantityRequested = lineDto.QuantityRequested,
                    QuantityIssued = 0,
                    UnitPrice = product.BaseRate
                };
                _context.StockOrderLines.Add(line);
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(stockOrder.Id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error creating stock order: {innerMessage}");
        }
    }

    public async Task<Result<StockOrderDto>> SubmitAsync(Guid id)
    {
        try
        {
            var order = await _context.StockOrders
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            if (order.Status != StockOrderStatus.Draft)
            {
                return Result.Fail<StockOrderDto>($"Stock order can only be submitted when in Draft status. Current status: {order.Status}");
            }

            order.Status = StockOrderStatus.PendingApproval;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error submitting stock order: {innerMessage}");
        }
    }

    public async Task<Result<StockOrderDto>> ApproveAsync(Guid id, string approvedBy, Guid warehouseLocationId)
    {
        try
        {
            var order = await _context.StockOrders
                .Include(so => so.Lines)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            if (order.Status != StockOrderStatus.PendingApproval)
            {
                return Result.Fail<StockOrderDto>($"Stock order can only be approved when in PendingApproval status. Current status: {order.Status}");
            }

            // Validate source location exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == order.SourceLocationId);

            if (!locationExists)
            {
                return Result.Fail<StockOrderDto>($"Source location with ID {order.SourceLocationId} not found");
            }

            // Reserve stock for each line from the source location
            foreach (var line in order.Lines)
            {
                var reserveResult = await ReserveStockAsync(line.ProductId, order.SourceLocationId, line.QuantityRequested);
                if (!reserveResult.Success)
                {
                    // Error is in Errors list, not Message (Result.Fail puts error in Errors)
                    var errorMessage = reserveResult.Errors.FirstOrDefault() ?? "Failed to reserve stock";
                    return Result.Fail<StockOrderDto>(errorMessage);
                }
            }

            order.Status = StockOrderStatus.Approved;
            order.ApprovedBy = approvedBy;
            order.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error approving stock order: {innerMessage}");
        }
    }

    public async Task<Result<StockOrderDto>> RejectAsync(Guid id, string rejectedBy, string reason)
    {
        try
        {
            var order = await _context.StockOrders
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            if (order.Status != StockOrderStatus.PendingApproval)
            {
                return Result.Fail<StockOrderDto>($"Stock order can only be rejected when in PendingApproval status. Current status: {order.Status}");
            }

            order.Status = StockOrderStatus.Draft;
            order.Notes = string.IsNullOrEmpty(order.Notes)
                ? $"Rejected by {rejectedBy}: {reason}"
                : $"{order.Notes}\n\nRejected by {rejectedBy}: {reason}";

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error rejecting stock order: {innerMessage}");
        }
    }

    public async Task<Result<StockOrderDto>> ReadyForCollectionAsync(Guid id)
    {
        try
        {
            var order = await _context.StockOrders
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            if (order.Status != StockOrderStatus.Approved && order.Status != StockOrderStatus.AwaitingPick)
            {
                return Result.Fail<StockOrderDto>($"Stock order can only be marked ready for collection when in Approved or AwaitingPick status. Current status: {order.Status}");
            }

            order.Status = StockOrderStatus.ReadyForCollection;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error marking stock order ready for collection: {innerMessage}");
        }
    }

    public async Task<Result<StockOrderDto>> CollectAsync(Guid id, Guid warehouseLocationId)
    {
        try
        {
            var order = await _context.StockOrders
                .Include(so => so.Lines)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            if (order.Status != StockOrderStatus.ReadyForCollection)
            {
                return Result.Fail<StockOrderDto>($"Stock order can only be collected when in ReadyForCollection status. Current status: {order.Status}");
            }

            // Validate source location exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == order.SourceLocationId);

            if (!locationExists)
            {
                return Result.Fail<StockOrderDto>($"Source location with ID {order.SourceLocationId} not found");
            }

            // Issue stock for each line from the source location
            foreach (var line in order.Lines)
            {
                var issueResult = await IssueStockAsync(
                    line.ProductId,
                    order.SourceLocationId,
                    line.QuantityRequested,
                    order.Id,
                    order.OrderNumber);

                if (!issueResult.Success)
                {
                    var errorMessage = issueResult.Errors.FirstOrDefault() ?? "Failed to issue stock";
                    return Result.Fail<StockOrderDto>(errorMessage);
                }

                line.QuantityIssued = line.QuantityRequested;
            }

            order.Status = StockOrderStatus.Collected;
            order.CollectedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error collecting stock order: {innerMessage}");
        }
    }

    public async Task<Result<StockOrderDto>> CancelAsync(Guid id, Guid? warehouseLocationId)
    {
        try
        {
            var order = await _context.StockOrders
                .Include(so => so.Lines)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail<StockOrderDto>($"Stock order with ID {id} not found");
            }

            if (order.Status == StockOrderStatus.Collected)
            {
                return Result.Fail<StockOrderDto>("Cannot cancel a stock order that has already been collected");
            }

            if (order.Status == StockOrderStatus.Cancelled)
            {
                return Result.Fail<StockOrderDto>("Stock order is already cancelled");
            }

            // If order was approved or beyond, release reserved stock from the source location
            if (order.Status >= StockOrderStatus.Approved && order.Status < StockOrderStatus.Collected)
            {
                foreach (var line in order.Lines)
                {
                    await ReleaseReservedStockAsync(line.ProductId, order.SourceLocationId, line.QuantityRequested);
                }
            }

            order.Status = StockOrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StockOrderDto>($"Error cancelling stock order: {innerMessage}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var order = await _context.StockOrders
                .Include(so => so.Lines)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
            {
                return Result.Fail($"Stock order with ID {id} not found");
            }

            if (order.Status != StockOrderStatus.Draft && order.Status != StockOrderStatus.Cancelled)
            {
                return Result.Fail("Only Draft or Cancelled stock orders can be deleted");
            }

            order.IsDeleted = true;
            foreach (var line in order.Lines)
            {
                line.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting stock order: {ex.Message}");
        }
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");
        var prefix = $"SO-{datePrefix}-";

        var lastOrder = await _context.StockOrders
            .Where(so => so.OrderNumber.StartsWith(prefix))
            .OrderByDescending(so => so.OrderNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastOrder != null)
        {
            var lastNumber = lastOrder.OrderNumber;
            var sequencePart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D3}";
    }

    private async Task<Result> ReserveStockAsync(Guid productId, Guid locationId, int quantity)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.LocationId == locationId);

        if (stockLevel == null)
        {
            return Result.Fail($"No stock level found for product {productId} at location {locationId}");
        }

        var availableQuantity = stockLevel.QuantityOnHand - stockLevel.QuantityReserved;
        if (availableQuantity < quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return Result.Fail($"Insufficient available stock for product {product?.ProductCode ?? productId.ToString()}. Available: {availableQuantity}, Requested: {quantity}");
        }

        stockLevel.QuantityReserved += quantity;
        stockLevel.LastMovementDate = DateTime.UtcNow;

        return Result.Ok();
    }

    private async Task ReleaseReservedStockAsync(Guid productId, Guid locationId, int quantity)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.LocationId == locationId);

        if (stockLevel != null)
        {
            stockLevel.QuantityReserved = Math.Max(0, stockLevel.QuantityReserved - quantity);
            stockLevel.LastMovementDate = DateTime.UtcNow;
        }
    }

    private async Task<Result> IssueStockAsync(
        Guid productId,
        Guid locationId,
        int quantity,
        Guid orderId,
        string orderNumber)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.LocationId == locationId);

        if (stockLevel == null)
        {
            return Result.Fail($"No stock level found for product {productId} at location {locationId}");
        }

        // Decrease on-hand and reserved quantities
        stockLevel.QuantityOnHand -= quantity;
        stockLevel.QuantityReserved = Math.Max(0, stockLevel.QuantityReserved - quantity);
        stockLevel.LastMovementDate = DateTime.UtcNow;

        // Create stock transaction
        var transactionNumber = await GenerateTransactionNumberAsync();

        var transaction = new StockTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.OrderIssue,
            ProductId = productId,
            LocationId = locationId,
            Quantity = -quantity, // Negative for issue
            ReferenceType = "StockOrder",
            ReferenceId = orderId,
            Notes = $"Issued via {orderNumber}"
        };

        _context.StockTransactions.Add(transaction);

        return Result.Ok();
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

    private static StockOrderDto MapToDto(StockOrder so)
    {
        return new StockOrderDto(
            so.Id,
            so.OrderNumber,
            so.SiteId,
            so.SiteName,
            so.OrderDate,
            so.RequiredDate,
            so.Status.ToString(),
            so.OrderTotal,
            so.RequestedBy,
            so.ApprovedBy,
            so.ApprovedDate,
            so.CollectedDate,
            so.Notes,
            so.SourceLocationId,
            so.SourceLocation.LocationName,
            so.Lines.Select(l => new StockOrderLineDto(
                l.Id,
                l.ProductId,
                l.Product.ProductCode,
                l.Product.ProductName,
                l.QuantityRequested,
                l.QuantityIssued,
                l.UnitPrice,
                l.QuantityRequested * l.UnitPrice
            )).ToList(),
            so.SourceProposalId,
            so.SourceProposalNumber
        );
    }
}
