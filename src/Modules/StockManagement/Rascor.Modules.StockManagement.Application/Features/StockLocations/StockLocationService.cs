using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.StockLocations;

public class StockLocationService : IStockLocationService
{
    private readonly IStockManagementDbContext _context;

    public StockLocationService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StockLocationDto>>> GetAllAsync()
    {
        try
        {
            var locations = await _context.StockLocations
                .OrderBy(l => l.LocationCode)
                .Select(l => new StockLocationDto(
                    l.Id,
                    l.LocationCode,
                    l.LocationName,
                    l.LocationType.ToString(),
                    l.Address,
                    l.IsActive
                ))
                .ToListAsync();

            return Result.Ok(locations);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockLocationDto>>($"Error retrieving stock locations: {ex.Message}");
        }
    }

    public async Task<Result<StockLocationDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var location = await _context.StockLocations
                .Where(l => l.Id == id)
                .Select(l => new StockLocationDto(
                    l.Id,
                    l.LocationCode,
                    l.LocationName,
                    l.LocationType.ToString(),
                    l.Address,
                    l.IsActive
                ))
                .FirstOrDefaultAsync();

            if (location == null)
            {
                return Result.Fail<StockLocationDto>($"Stock location with ID {id} not found");
            }

            return Result.Ok(location);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLocationDto>($"Error retrieving stock location: {ex.Message}");
        }
    }

    public async Task<Result<StockLocationDto>> CreateAsync(CreateStockLocationDto dto)
    {
        try
        {
            // Parse the location type
            if (!Enum.TryParse<LocationType>(dto.LocationType, ignoreCase: true, out var locationType))
            {
                return Result.Fail<StockLocationDto>($"Invalid location type: {dto.LocationType}");
            }

            // Check for duplicate LocationCode within the same tenant
            var duplicateCode = await _context.StockLocations
                .AnyAsync(l => l.LocationCode == dto.LocationCode);

            if (duplicateCode)
            {
                return Result.Fail<StockLocationDto>($"Stock location with code '{dto.LocationCode}' already exists");
            }

            var location = new StockLocation
            {
                Id = Guid.NewGuid(),
                LocationCode = dto.LocationCode,
                LocationName = dto.LocationName,
                LocationType = locationType,
                Address = dto.Address,
                IsActive = dto.IsActive
            };

            _context.StockLocations.Add(location);
            await _context.SaveChangesAsync();

            var locationDto = new StockLocationDto(
                location.Id,
                location.LocationCode,
                location.LocationName,
                location.LocationType.ToString(),
                location.Address,
                location.IsActive
            );

            return Result.Ok(locationDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLocationDto>($"Error creating stock location: {ex.Message}");
        }
    }

    public async Task<Result<StockLocationDto>> UpdateAsync(Guid id, UpdateStockLocationDto dto)
    {
        try
        {
            var location = await _context.StockLocations
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null)
            {
                return Result.Fail<StockLocationDto>($"Stock location with ID {id} not found");
            }

            // Parse the location type
            if (!Enum.TryParse<LocationType>(dto.LocationType, ignoreCase: true, out var locationType))
            {
                return Result.Fail<StockLocationDto>($"Invalid location type: {dto.LocationType}");
            }

            // Check for duplicate LocationCode (excluding current location)
            var duplicateCode = await _context.StockLocations
                .AnyAsync(l => l.LocationCode == dto.LocationCode && l.Id != id);

            if (duplicateCode)
            {
                return Result.Fail<StockLocationDto>($"Stock location with code '{dto.LocationCode}' already exists");
            }

            location.LocationCode = dto.LocationCode;
            location.LocationName = dto.LocationName;
            location.LocationType = locationType;
            location.Address = dto.Address;
            location.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var locationDto = new StockLocationDto(
                location.Id,
                location.LocationCode,
                location.LocationName,
                location.LocationType.ToString(),
                location.Address,
                location.IsActive
            );

            return Result.Ok(locationDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockLocationDto>($"Error updating stock location: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var location = await _context.StockLocations
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null)
            {
                return Result.Fail($"Stock location with ID {id} not found");
            }

            location.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting stock location: {ex.Message}");
        }
    }
}
