using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Features.StockLevels;

public class StockLevelService : IStockLevelService
{
    private readonly IStockManagementDbContext _context;

    public StockLevelService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StockLevelDto>>> GetAllAsync()
    {
        try
        {
            var stockLevels = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .OrderBy(sl => sl.Location.LocationCode)
                .ThenBy(sl => sl.BayLocation != null ? sl.BayLocation.BayCode : "zzz")
                .ThenBy(sl => sl.Product.ProductCode)
                .Select(sl => new StockLevelDto(
                    sl.Id,
                    sl.ProductId,
                    sl.Product.ProductCode,
                    sl.Product.ProductName,
                    sl.LocationId,
                    sl.Location.LocationCode,
                    sl.Location.LocationName,
                    sl.QuantityOnHand,
                    sl.QuantityReserved,
                    sl.QuantityOnHand - sl.QuantityReserved,
                    sl.QuantityOnOrder,
                    sl.BinLocation,
                    sl.BayLocationId,
                    sl.BayLocation != null ? sl.BayLocation.BayCode : null,
                    sl.BayLocation != null ? sl.BayLocation.BayName : null,
                    sl.LastMovementDate,
                    sl.LastCountDate,
                    sl.Product.ReorderLevel
                ))
                .ToListAsync();

            return Result.Ok(stockLevels);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockLevelDto>>($"Error retrieving stock levels: {ex.Message}");
        }
    }

    public async Task<Result<StockLevelDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var stockLevel = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.Id == id)
                .Select(sl => new StockLevelDto(
                    sl.Id,
                    sl.ProductId,
                    sl.Product.ProductCode,
                    sl.Product.ProductName,
                    sl.LocationId,
                    sl.Location.LocationCode,
                    sl.Location.LocationName,
                    sl.QuantityOnHand,
                    sl.QuantityReserved,
                    sl.QuantityOnHand - sl.QuantityReserved,
                    sl.QuantityOnOrder,
                    sl.BinLocation,
                    sl.BayLocationId,
                    sl.BayLocation != null ? sl.BayLocation.BayCode : null,
                    sl.BayLocation != null ? sl.BayLocation.BayName : null,
                    sl.LastMovementDate,
                    sl.LastCountDate,
                    sl.Product.ReorderLevel
                ))
                .FirstOrDefaultAsync();

            if (stockLevel == null)
            {
                return Result.Fail<StockLevelDto>($"Stock level with ID {id} not found");
            }

            return Result.Ok(stockLevel);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLevelDto>($"Error retrieving stock level: {ex.Message}");
        }
    }

    public async Task<Result<StockLevelDto>> GetByProductAndLocationAsync(Guid productId, Guid locationId)
    {
        try
        {
            var stockLevel = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.ProductId == productId && sl.LocationId == locationId)
                .Select(sl => new StockLevelDto(
                    sl.Id,
                    sl.ProductId,
                    sl.Product.ProductCode,
                    sl.Product.ProductName,
                    sl.LocationId,
                    sl.Location.LocationCode,
                    sl.Location.LocationName,
                    sl.QuantityOnHand,
                    sl.QuantityReserved,
                    sl.QuantityOnHand - sl.QuantityReserved,
                    sl.QuantityOnOrder,
                    sl.BinLocation,
                    sl.BayLocationId,
                    sl.BayLocation != null ? sl.BayLocation.BayCode : null,
                    sl.BayLocation != null ? sl.BayLocation.BayName : null,
                    sl.LastMovementDate,
                    sl.LastCountDate,
                    sl.Product.ReorderLevel
                ))
                .FirstOrDefaultAsync();

            if (stockLevel == null)
            {
                return Result.Fail<StockLevelDto>($"Stock level for product {productId} at location {locationId} not found");
            }

            return Result.Ok(stockLevel);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLevelDto>($"Error retrieving stock level: {ex.Message}");
        }
    }

    public async Task<Result<List<StockLevelDto>>> GetByLocationAsync(Guid locationId)
    {
        try
        {
            var stockLevels = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.LocationId == locationId)
                .OrderBy(sl => sl.BayLocation != null ? sl.BayLocation.BayCode : "zzz")
                .ThenBy(sl => sl.Product.ProductCode)
                .Select(sl => new StockLevelDto(
                    sl.Id,
                    sl.ProductId,
                    sl.Product.ProductCode,
                    sl.Product.ProductName,
                    sl.LocationId,
                    sl.Location.LocationCode,
                    sl.Location.LocationName,
                    sl.QuantityOnHand,
                    sl.QuantityReserved,
                    sl.QuantityOnHand - sl.QuantityReserved,
                    sl.QuantityOnOrder,
                    sl.BinLocation,
                    sl.BayLocationId,
                    sl.BayLocation != null ? sl.BayLocation.BayCode : null,
                    sl.BayLocation != null ? sl.BayLocation.BayName : null,
                    sl.LastMovementDate,
                    sl.LastCountDate,
                    sl.Product.ReorderLevel
                ))
                .ToListAsync();

            return Result.Ok(stockLevels);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockLevelDto>>($"Error retrieving stock levels by location: {ex.Message}");
        }
    }

    public async Task<Result<List<StockLevelDto>>> GetLowStockAsync()
    {
        try
        {
            var stockLevels = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .Where(sl => sl.QuantityOnHand <= sl.Product.ReorderLevel)
                .OrderBy(sl => sl.QuantityOnHand)
                .ThenBy(sl => sl.Product.ProductCode)
                .Select(sl => new StockLevelDto(
                    sl.Id,
                    sl.ProductId,
                    sl.Product.ProductCode,
                    sl.Product.ProductName,
                    sl.LocationId,
                    sl.Location.LocationCode,
                    sl.Location.LocationName,
                    sl.QuantityOnHand,
                    sl.QuantityReserved,
                    sl.QuantityOnHand - sl.QuantityReserved,
                    sl.QuantityOnOrder,
                    sl.BinLocation,
                    sl.BayLocationId,
                    sl.BayLocation != null ? sl.BayLocation.BayCode : null,
                    sl.BayLocation != null ? sl.BayLocation.BayName : null,
                    sl.LastMovementDate,
                    sl.LastCountDate,
                    sl.Product.ReorderLevel
                ))
                .ToListAsync();

            return Result.Ok(stockLevels);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockLevelDto>>($"Error retrieving low stock levels: {ex.Message}");
        }
    }

    public async Task<Result<StockLevelDto>> CreateAsync(CreateStockLevelDto dto)
    {
        try
        {
            // Validate that ProductId exists
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                return Result.Fail<StockLevelDto>($"Product with ID {dto.ProductId} not found");
            }

            // Validate that LocationId exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == dto.LocationId);

            if (!locationExists)
            {
                return Result.Fail<StockLevelDto>($"Stock location with ID {dto.LocationId} not found");
            }

            // Check if stock level already exists for this product/location combination
            var existingLevel = await _context.StockLevels
                .AnyAsync(sl => sl.ProductId == dto.ProductId && sl.LocationId == dto.LocationId);

            if (existingLevel)
            {
                return Result.Fail<StockLevelDto>($"Stock level already exists for this product at this location");
            }

            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                LocationId = dto.LocationId,
                QuantityOnHand = dto.QuantityOnHand,
                QuantityReserved = 0,
                QuantityOnOrder = 0,
                BinLocation = dto.BinLocation,
                LastMovementDate = DateTime.UtcNow
            };

            _context.StockLevels.Add(stockLevel);
            await _context.SaveChangesAsync();

            // Reload with related entities
            var createdStockLevel = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .FirstAsync(sl => sl.Id == stockLevel.Id);

            var stockLevelDto = new StockLevelDto(
                createdStockLevel.Id,
                createdStockLevel.ProductId,
                createdStockLevel.Product.ProductCode,
                createdStockLevel.Product.ProductName,
                createdStockLevel.LocationId,
                createdStockLevel.Location.LocationCode,
                createdStockLevel.Location.LocationName,
                createdStockLevel.QuantityOnHand,
                createdStockLevel.QuantityReserved,
                createdStockLevel.QuantityOnHand - createdStockLevel.QuantityReserved,
                createdStockLevel.QuantityOnOrder,
                createdStockLevel.BinLocation,
                createdStockLevel.BayLocationId,
                createdStockLevel.BayLocation?.BayCode,
                createdStockLevel.BayLocation?.BayName,
                createdStockLevel.LastMovementDate,
                createdStockLevel.LastCountDate,
                createdStockLevel.Product.ReorderLevel
            );

            return Result.Ok(stockLevelDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLevelDto>($"Error creating stock level: {ex.Message}");
        }
    }

    public async Task<Result<StockLevelDto>> UpdateAsync(Guid id, UpdateStockLevelDto dto)
    {
        try
        {
            var stockLevel = await _context.StockLevels
                .Include(sl => sl.Product)
                .Include(sl => sl.Location)
                .Include(sl => sl.BayLocation)
                .FirstOrDefaultAsync(sl => sl.Id == id);

            if (stockLevel == null)
            {
                return Result.Fail<StockLevelDto>($"Stock level with ID {id} not found");
            }

            stockLevel.QuantityOnHand = dto.QuantityOnHand;
            stockLevel.QuantityReserved = dto.QuantityReserved;
            stockLevel.QuantityOnOrder = dto.QuantityOnOrder;
            stockLevel.BinLocation = dto.BinLocation;
            stockLevel.BayLocationId = dto.BayLocationId;
            stockLevel.LastMovementDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload to get bay location info
            await _context.StockLevels.Entry(stockLevel).Reference(sl => sl.BayLocation).LoadAsync();

            var stockLevelDto = new StockLevelDto(
                stockLevel.Id,
                stockLevel.ProductId,
                stockLevel.Product.ProductCode,
                stockLevel.Product.ProductName,
                stockLevel.LocationId,
                stockLevel.Location.LocationCode,
                stockLevel.Location.LocationName,
                stockLevel.QuantityOnHand,
                stockLevel.QuantityReserved,
                stockLevel.QuantityOnHand - stockLevel.QuantityReserved,
                stockLevel.QuantityOnOrder,
                stockLevel.BinLocation,
                stockLevel.BayLocationId,
                stockLevel.BayLocation?.BayCode,
                stockLevel.BayLocation?.BayName,
                stockLevel.LastMovementDate,
                stockLevel.LastCountDate,
                stockLevel.Product.ReorderLevel
            );

            return Result.Ok(stockLevelDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLevelDto>($"Error updating stock level: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(sl => sl.Id == id);

            if (stockLevel == null)
            {
                return Result.Fail($"Stock level with ID {id} not found");
            }

            stockLevel.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting stock level: {ex.Message}");
        }
    }
}
