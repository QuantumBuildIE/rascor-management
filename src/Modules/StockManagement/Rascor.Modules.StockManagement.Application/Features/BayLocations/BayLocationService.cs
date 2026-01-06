using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.BayLocations.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Features.BayLocations;

public class BayLocationService : IBayLocationService
{
    private readonly IStockManagementDbContext _context;

    public BayLocationService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BayLocationDto>>> GetAllAsync(Guid? stockLocationId = null)
    {
        try
        {
            var query = _context.BayLocations
                .Include(bl => bl.StockLocation)
                .AsQueryable();

            if (stockLocationId.HasValue)
            {
                query = query.Where(bl => bl.StockLocationId == stockLocationId.Value);
            }

            var bayLocations = await query
                .OrderBy(bl => bl.StockLocation.LocationCode)
                .ThenBy(bl => bl.BayCode)
                .Select(bl => new BayLocationDto(
                    bl.Id,
                    bl.BayCode,
                    bl.BayName,
                    bl.StockLocationId,
                    bl.StockLocation.LocationCode,
                    bl.StockLocation.LocationName,
                    bl.Capacity,
                    bl.IsActive,
                    bl.Notes
                ))
                .ToListAsync();

            return Result.Ok(bayLocations);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<BayLocationDto>>($"Error retrieving bay locations: {ex.Message}");
        }
    }

    public async Task<Result<BayLocationDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var bayLocation = await _context.BayLocations
                .Include(bl => bl.StockLocation)
                .Where(bl => bl.Id == id)
                .Select(bl => new BayLocationDto(
                    bl.Id,
                    bl.BayCode,
                    bl.BayName,
                    bl.StockLocationId,
                    bl.StockLocation.LocationCode,
                    bl.StockLocation.LocationName,
                    bl.Capacity,
                    bl.IsActive,
                    bl.Notes
                ))
                .FirstOrDefaultAsync();

            if (bayLocation == null)
            {
                return Result.Fail<BayLocationDto>($"Bay location with ID {id} not found");
            }

            return Result.Ok(bayLocation);
        }
        catch (Exception ex)
        {
            return Result.Fail<BayLocationDto>($"Error retrieving bay location: {ex.Message}");
        }
    }

    public async Task<Result<List<BayLocationDto>>> GetByLocationAsync(Guid stockLocationId)
    {
        try
        {
            var bayLocations = await _context.BayLocations
                .Include(bl => bl.StockLocation)
                .Where(bl => bl.StockLocationId == stockLocationId)
                .OrderBy(bl => bl.BayCode)
                .Select(bl => new BayLocationDto(
                    bl.Id,
                    bl.BayCode,
                    bl.BayName,
                    bl.StockLocationId,
                    bl.StockLocation.LocationCode,
                    bl.StockLocation.LocationName,
                    bl.Capacity,
                    bl.IsActive,
                    bl.Notes
                ))
                .ToListAsync();

            return Result.Ok(bayLocations);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<BayLocationDto>>($"Error retrieving bay locations by stock location: {ex.Message}");
        }
    }

    public async Task<Result<BayLocationDto>> CreateAsync(CreateBayLocationDto dto)
    {
        try
        {
            // Validate that StockLocationId exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == dto.StockLocationId);

            if (!locationExists)
            {
                return Result.Fail<BayLocationDto>($"Stock location with ID {dto.StockLocationId} not found");
            }

            // Check for duplicate BayCode within the same stock location
            var duplicateCode = await _context.BayLocations
                .AnyAsync(bl => bl.StockLocationId == dto.StockLocationId && bl.BayCode == dto.BayCode);

            if (duplicateCode)
            {
                return Result.Fail<BayLocationDto>($"Bay location with code '{dto.BayCode}' already exists at this location");
            }

            var bayLocation = new BayLocation
            {
                Id = Guid.NewGuid(),
                BayCode = dto.BayCode,
                BayName = dto.BayName,
                StockLocationId = dto.StockLocationId,
                Capacity = dto.Capacity,
                IsActive = dto.IsActive,
                Notes = dto.Notes
            };

            _context.BayLocations.Add(bayLocation);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(bayLocation.Id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<BayLocationDto>($"Error creating bay location: {innerMessage}");
        }
    }

    public async Task<Result<BayLocationDto>> UpdateAsync(Guid id, UpdateBayLocationDto dto)
    {
        try
        {
            var bayLocation = await _context.BayLocations
                .FirstOrDefaultAsync(bl => bl.Id == id);

            if (bayLocation == null)
            {
                return Result.Fail<BayLocationDto>($"Bay location with ID {id} not found");
            }

            // Validate that StockLocationId exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == dto.StockLocationId);

            if (!locationExists)
            {
                return Result.Fail<BayLocationDto>($"Stock location with ID {dto.StockLocationId} not found");
            }

            // Check for duplicate BayCode within the same stock location (excluding current)
            var duplicateCode = await _context.BayLocations
                .AnyAsync(bl => bl.StockLocationId == dto.StockLocationId && bl.BayCode == dto.BayCode && bl.Id != id);

            if (duplicateCode)
            {
                return Result.Fail<BayLocationDto>($"Bay location with code '{dto.BayCode}' already exists at this location");
            }

            bayLocation.BayCode = dto.BayCode;
            bayLocation.BayName = dto.BayName;
            bayLocation.StockLocationId = dto.StockLocationId;
            bayLocation.Capacity = dto.Capacity;
            bayLocation.IsActive = dto.IsActive;
            bayLocation.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result.Fail<BayLocationDto>($"Error updating bay location: {innerMessage}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var bayLocation = await _context.BayLocations
                .FirstOrDefaultAsync(bl => bl.Id == id);

            if (bayLocation == null)
            {
                return Result.Fail($"Bay location with ID {id} not found");
            }

            // Check if bay is in use by any stock levels
            var inUse = await _context.StockLevels
                .AnyAsync(sl => sl.BayLocationId == id);

            if (inUse)
            {
                return Result.Fail("Cannot delete bay location that is assigned to stock levels. Please reassign or remove the bay assignment first.");
            }

            bayLocation.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting bay location: {ex.Message}");
        }
    }
}
