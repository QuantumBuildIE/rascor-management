using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes;

public class StocktakeService : IStocktakeService
{
    private readonly IStockManagementDbContext _context;

    public StocktakeService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StocktakeDto>>> GetAllAsync()
    {
        try
        {
            var stocktakes = await _context.Stocktakes
                .Include(s => s.Location)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Product)
                .OrderByDescending(s => s.CountDate)
                .ThenByDescending(s => s.StocktakeNumber)
                .Select(s => MapToDto(s))
                .ToListAsync();

            return Result.Ok(stocktakes);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StocktakeDto>>($"Error retrieving stocktakes: {ex.Message}");
        }
    }

    public async Task<Result<StocktakeDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.Location)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Product)
                .Where(s => s.Id == id)
                .Select(s => MapToDto(s))
                .FirstOrDefaultAsync();

            if (stocktake == null)
            {
                return Result.Fail<StocktakeDto>($"Stocktake with ID {id} not found");
            }

            return Result.Ok(stocktake);
        }
        catch (Exception ex)
        {
            return Result.Fail<StocktakeDto>($"Error retrieving stocktake: {ex.Message}");
        }
    }

    public async Task<Result<List<StocktakeDto>>> GetByLocationAsync(Guid locationId)
    {
        try
        {
            var stocktakes = await _context.Stocktakes
                .Include(s => s.Location)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Product)
                .Where(s => s.LocationId == locationId)
                .OrderByDescending(s => s.CountDate)
                .ThenByDescending(s => s.StocktakeNumber)
                .Select(s => MapToDto(s))
                .ToListAsync();

            return Result.Ok(stocktakes);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StocktakeDto>>($"Error retrieving stocktakes by location: {ex.Message}");
        }
    }

    public async Task<Result<StocktakeDto>> CreateAsync(CreateStocktakeDto dto)
    {
        try
        {
            // Validate that LocationId exists
            var location = await _context.StockLocations
                .FirstOrDefaultAsync(l => l.Id == dto.LocationId);

            if (location == null)
            {
                return Result.Fail<StocktakeDto>($"Stock location with ID {dto.LocationId} not found");
            }

            // Get all stock levels at this location (including bay location info)
            var stockLevels = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.LocationId == dto.LocationId && sl.QuantityOnHand > 0)
                .ToListAsync();

            if (!stockLevels.Any())
            {
                return Result.Fail<StocktakeDto>($"No products with stock found at location {location.LocationName}");
            }

            // Generate stocktake number
            var stocktakeNumber = await GenerateStocktakeNumberAsync();

            var stocktake = new Stocktake
            {
                Id = Guid.NewGuid(),
                StocktakeNumber = stocktakeNumber,
                LocationId = dto.LocationId,
                CountDate = DateTime.UtcNow,
                Status = StocktakeStatus.Draft,
                CountedBy = dto.CountedBy,
                Notes = dto.Notes
            };

            _context.Stocktakes.Add(stocktake);

            // Create lines for all products with stock at this location
            foreach (var stockLevel in stockLevels)
            {
                var line = new StocktakeLine
                {
                    Id = Guid.NewGuid(),
                    StocktakeId = stocktake.Id,
                    ProductId = stockLevel.ProductId,
                    SystemQuantity = stockLevel.QuantityOnHand,
                    CountedQuantity = null,
                    AdjustmentCreated = false,
                    // Capture bay location at time of count
                    BayLocationId = stockLevel.BayLocationId,
                    BayCode = stockLevel.BayLocation?.BayCode
                };
                _context.StocktakeLines.Add(line);
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(stocktake.Id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StocktakeDto>($"Error creating stocktake: {innerMessage}");
        }
    }

    public async Task<Result<StocktakeDto>> StartAsync(Guid id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
            {
                return Result.Fail<StocktakeDto>($"Stocktake with ID {id} not found");
            }

            if (stocktake.Status != StocktakeStatus.Draft)
            {
                return Result.Fail<StocktakeDto>($"Stocktake can only be started when in Draft status. Current status: {stocktake.Status}");
            }

            stocktake.Status = StocktakeStatus.InProgress;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StocktakeDto>($"Error starting stocktake: {innerMessage}");
        }
    }

    public async Task<Result<StocktakeDto>> UpdateLineAsync(Guid stocktakeId, Guid lineId, UpdateStocktakeLineDto dto)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == stocktakeId);

            if (stocktake == null)
            {
                return Result.Fail<StocktakeDto>($"Stocktake with ID {stocktakeId} not found");
            }

            if (stocktake.Status != StocktakeStatus.InProgress)
            {
                return Result.Fail<StocktakeDto>($"Stocktake lines can only be updated when in InProgress status. Current status: {stocktake.Status}");
            }

            var line = stocktake.Lines.FirstOrDefault(l => l.Id == lineId);
            if (line == null)
            {
                return Result.Fail<StocktakeDto>($"Stocktake line with ID {lineId} not found");
            }

            line.CountedQuantity = dto.CountedQuantity;
            line.VarianceReason = dto.VarianceReason;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(stocktakeId);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StocktakeDto>($"Error updating stocktake line: {innerMessage}");
        }
    }

    public async Task<Result<StocktakeDto>> CompleteAsync(Guid id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
            {
                return Result.Fail<StocktakeDto>($"Stocktake with ID {id} not found");
            }

            if (stocktake.Status != StocktakeStatus.InProgress)
            {
                return Result.Fail<StocktakeDto>($"Stocktake can only be completed when in InProgress status. Current status: {stocktake.Status}");
            }

            // Check if all lines have been counted
            var uncountedLines = stocktake.Lines.Where(l => l.CountedQuantity == null).ToList();
            if (uncountedLines.Any())
            {
                return Result.Fail<StocktakeDto>($"{uncountedLines.Count} line(s) have not been counted yet");
            }

            // Process each line with variance
            foreach (var line in stocktake.Lines)
            {
                var variance = line.CountedQuantity!.Value - line.SystemQuantity;

                if (variance != 0)
                {
                    // Create stock adjustment transaction
                    await CreateAdjustmentTransactionAsync(
                        line.ProductId,
                        stocktake.LocationId,
                        variance,
                        stocktake.Id,
                        stocktake.StocktakeNumber);

                    // Update stock level
                    await UpdateStockLevelAsync(line.ProductId, stocktake.LocationId, variance);

                    line.AdjustmentCreated = true;
                }

                // Update LastCountDate regardless of variance
                await UpdateLastCountDateAsync(line.ProductId, stocktake.LocationId);
            }

            stocktake.Status = StocktakeStatus.Completed;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StocktakeDto>($"Error completing stocktake: {innerMessage}");
        }
    }

    public async Task<Result<StocktakeDto>> CancelAsync(Guid id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
            {
                return Result.Fail<StocktakeDto>($"Stocktake with ID {id} not found");
            }

            if (stocktake.Status == StocktakeStatus.Completed)
            {
                return Result.Fail<StocktakeDto>("Cannot cancel a completed stocktake");
            }

            if (stocktake.Status == StocktakeStatus.Cancelled)
            {
                return Result.Fail<StocktakeDto>("Stocktake is already cancelled");
            }

            stocktake.Status = StocktakeStatus.Cancelled;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<StocktakeDto>($"Error cancelling stocktake: {innerMessage}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var stocktake = await _context.Stocktakes
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
            {
                return Result.Fail($"Stocktake with ID {id} not found");
            }

            if (stocktake.Status == StocktakeStatus.Completed)
            {
                return Result.Fail("Cannot delete a completed stocktake");
            }

            stocktake.IsDeleted = true;
            foreach (var line in stocktake.Lines)
            {
                line.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting stocktake: {ex.Message}");
        }
    }

    private async Task<string> GenerateStocktakeNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");
        var prefix = $"ST-{datePrefix}-";

        var lastStocktake = await _context.Stocktakes
            .Where(s => s.StocktakeNumber.StartsWith(prefix))
            .OrderByDescending(s => s.StocktakeNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastStocktake != null)
        {
            var lastNumber = lastStocktake.StocktakeNumber;
            var sequencePart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D3}";
    }

    private async Task CreateAdjustmentTransactionAsync(
        Guid productId,
        Guid locationId,
        int variance,
        Guid stocktakeId,
        string stocktakeNumber)
    {
        var transactionNumber = await GenerateTransactionNumberAsync();

        var transactionType = variance > 0
            ? TransactionType.AdjustmentIn
            : TransactionType.AdjustmentOut;

        var transaction = new StockTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.StocktakeAdjustment,
            ProductId = productId,
            LocationId = locationId,
            Quantity = variance,
            ReferenceType = "Stocktake",
            ReferenceId = stocktakeId,
            Notes = $"Stocktake adjustment via {stocktakeNumber}"
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

    private async Task UpdateStockLevelAsync(Guid productId, Guid locationId, int variance)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.LocationId == locationId);

        if (stockLevel != null)
        {
            stockLevel.QuantityOnHand += variance;
            stockLevel.LastMovementDate = DateTime.UtcNow;
        }
    }

    private async Task UpdateLastCountDateAsync(Guid productId, Guid locationId)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.LocationId == locationId);

        if (stockLevel != null)
        {
            stockLevel.LastCountDate = DateTime.UtcNow;
        }
    }

    private static StocktakeDto MapToDto(Stocktake s)
    {
        return new StocktakeDto(
            s.Id,
            s.StocktakeNumber,
            s.LocationId,
            s.Location.LocationCode,
            s.Location.LocationName,
            s.CountDate,
            s.Status.ToString(),
            s.CountedBy,
            s.Notes,
            s.Lines.Select(l => new StocktakeLineDto(
                l.Id,
                l.ProductId,
                l.Product.ProductCode,
                l.Product.ProductName,
                l.SystemQuantity,
                l.CountedQuantity,
                l.CountedQuantity.HasValue ? l.CountedQuantity.Value - l.SystemQuantity : null,
                l.AdjustmentCreated,
                l.VarianceReason,
                l.BayLocationId,
                l.BayCode
            )).OrderBy(l => l.BayCode ?? "zzz").ThenBy(l => l.ProductCode).ToList()
        );
    }
}
