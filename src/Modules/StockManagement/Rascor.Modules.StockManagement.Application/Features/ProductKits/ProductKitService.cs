using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.StockManagement.Application.Features.ProductKits.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Features.ProductKits;

public class ProductKitService : IProductKitService
{
    private readonly IStockManagementDbContext _context;

    public ProductKitService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ProductKitListItemDto>>> GetAllAsync(
        string? search = null,
        Guid? categoryId = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortColumn = null,
        string? sortDirection = null)
    {
        try
        {
            var query = _context.ProductKits
                .Include(pk => pk.Category)
                .Include(pk => pk.Items)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(pk =>
                    pk.KitCode.ToLower().Contains(searchLower) ||
                    pk.KitName.ToLower().Contains(searchLower) ||
                    (pk.Description != null && pk.Description.ToLower().Contains(searchLower)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(pk => pk.CategoryId == categoryId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(pk => pk.IsActive == isActive.Value);
            }

            // Apply sorting
            query = sortColumn?.ToLower() switch
            {
                "kitcode" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(pk => pk.KitCode)
                    : query.OrderBy(pk => pk.KitCode),
                "kitname" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(pk => pk.KitName)
                    : query.OrderBy(pk => pk.KitName),
                "categoryname" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(pk => pk.Category != null ? pk.Category.CategoryName : "")
                    : query.OrderBy(pk => pk.Category != null ? pk.Category.CategoryName : ""),
                "totalcost" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(pk => pk.TotalCost)
                    : query.OrderBy(pk => pk.TotalCost),
                "totalprice" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(pk => pk.TotalPrice)
                    : query.OrderBy(pk => pk.TotalPrice),
                "itemcount" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(pk => pk.Items.Count)
                    : query.OrderBy(pk => pk.Items.Count),
                _ => query.OrderBy(pk => pk.KitName)
            };

            // Project to DTO before pagination
            var projectedQuery = query.Select(pk => new ProductKitListItemDto(
                pk.Id,
                pk.KitCode,
                pk.KitName,
                pk.Category != null ? pk.Category.CategoryName : null,
                pk.IsActive,
                pk.TotalCost,
                pk.TotalPrice,
                pk.Items.Count
            ));

            var result = await PaginatedList<ProductKitListItemDto>.CreateAsync(projectedQuery, pageNumber, pageSize);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<PaginatedList<ProductKitListItemDto>>($"Error retrieving product kits: {ex.Message}");
        }
    }

    public async Task<Result<ProductKitDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var kit = await _context.ProductKits
                .Include(pk => pk.Category)
                .Include(pk => pk.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pk => pk.Id == id);

            if (kit == null)
            {
                return Result.Fail<ProductKitDto>($"Product kit with ID {id} not found");
            }

            var dto = MapToDto(kit);
            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductKitDto>($"Error retrieving product kit: {ex.Message}");
        }
    }

    public async Task<Result<ProductKitDto>> CreateAsync(CreateProductKitDto dto)
    {
        try
        {
            // Check for duplicate kit code
            var existingKit = await _context.ProductKits
                .FirstOrDefaultAsync(pk => pk.KitCode == dto.KitCode);

            if (existingKit != null)
            {
                return Result.Fail<ProductKitDto>($"A product kit with code '{dto.KitCode}' already exists");
            }

            var kit = new ProductKit
            {
                Id = Guid.NewGuid(),
                KitCode = dto.KitCode,
                KitName = dto.KitName,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive,
                Notes = dto.Notes,
                TotalCost = 0,
                TotalPrice = 0
            };

            _context.ProductKits.Add(kit);

            // Add items if provided
            if (dto.Items != null && dto.Items.Count > 0)
            {
                foreach (var itemDto in dto.Items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        return Result.Fail<ProductKitDto>($"Product with ID {itemDto.ProductId} not found");
                    }

                    var item = new ProductKitItem
                    {
                        Id = Guid.NewGuid(),
                        ProductKitId = kit.Id,
                        ProductId = itemDto.ProductId,
                        DefaultQuantity = itemDto.DefaultQuantity,
                        SortOrder = itemDto.SortOrder,
                        Notes = itemDto.Notes
                    };

                    _context.ProductKitItems.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            // Recalculate totals
            await RecalculateTotalsAsync(kit.Id);

            // Reload with navigation properties
            var createdKit = await _context.ProductKits
                .Include(pk => pk.Category)
                .Include(pk => pk.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pk => pk.Id == kit.Id);

            return Result.Ok(MapToDto(createdKit!));
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductKitDto>($"Error creating product kit: {ex.Message}");
        }
    }

    public async Task<Result<ProductKitDto>> UpdateAsync(Guid id, UpdateProductKitDto dto)
    {
        try
        {
            var kit = await _context.ProductKits
                .Include(pk => pk.Category)
                .Include(pk => pk.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pk => pk.Id == id);

            if (kit == null)
            {
                return Result.Fail<ProductKitDto>($"Product kit with ID {id} not found");
            }

            // Check for duplicate kit code (excluding current kit)
            var existingKit = await _context.ProductKits
                .FirstOrDefaultAsync(pk => pk.KitCode == dto.KitCode && pk.Id != id);

            if (existingKit != null)
            {
                return Result.Fail<ProductKitDto>($"A product kit with code '{dto.KitCode}' already exists");
            }

            kit.KitCode = dto.KitCode;
            kit.KitName = dto.KitName;
            kit.Description = dto.Description;
            kit.CategoryId = dto.CategoryId;
            kit.IsActive = dto.IsActive;
            kit.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var updatedKit = await _context.ProductKits
                .Include(pk => pk.Category)
                .Include(pk => pk.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pk => pk.Id == id);

            return Result.Ok(MapToDto(updatedKit!));
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductKitDto>($"Error updating product kit: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var kit = await _context.ProductKits
                .FirstOrDefaultAsync(pk => pk.Id == id);

            if (kit == null)
            {
                return Result.Fail($"Product kit with ID {id} not found");
            }

            kit.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting product kit: {ex.Message}");
        }
    }

    public async Task<Result<ProductKitItemDto>> AddItemAsync(Guid kitId, CreateProductKitItemDto dto)
    {
        try
        {
            var kit = await _context.ProductKits
                .FirstOrDefaultAsync(pk => pk.Id == kitId);

            if (kit == null)
            {
                return Result.Fail<ProductKitItemDto>($"Product kit with ID {kitId} not found");
            }

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return Result.Fail<ProductKitItemDto>($"Product with ID {dto.ProductId} not found");
            }

            // Check if product already exists in kit
            var existingItem = await _context.ProductKitItems
                .FirstOrDefaultAsync(i => i.ProductKitId == kitId && i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                return Result.Fail<ProductKitItemDto>($"Product '{product.ProductName}' is already in this kit");
            }

            var item = new ProductKitItem
            {
                Id = Guid.NewGuid(),
                ProductKitId = kitId,
                ProductId = dto.ProductId,
                DefaultQuantity = dto.DefaultQuantity,
                SortOrder = dto.SortOrder,
                Notes = dto.Notes
            };

            _context.ProductKitItems.Add(item);
            await _context.SaveChangesAsync();

            // Recalculate totals
            await RecalculateTotalsAsync(kitId);

            // Reload with navigation properties
            var createdItem = await _context.ProductKitItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            return Result.Ok(MapItemToDto(createdItem!));
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductKitItemDto>($"Error adding item to product kit: {ex.Message}");
        }
    }

    public async Task<Result<ProductKitItemDto>> UpdateItemAsync(Guid itemId, UpdateProductKitItemDto dto)
    {
        try
        {
            var item = await _context.ProductKitItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return Result.Fail<ProductKitItemDto>($"Product kit item with ID {itemId} not found");
            }

            // If product is changing, check it exists and isn't already in kit
            if (dto.ProductId != item.ProductId)
            {
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                {
                    return Result.Fail<ProductKitItemDto>($"Product with ID {dto.ProductId} not found");
                }

                var existingItem = await _context.ProductKitItems
                    .FirstOrDefaultAsync(i => i.ProductKitId == item.ProductKitId && i.ProductId == dto.ProductId && i.Id != itemId);

                if (existingItem != null)
                {
                    return Result.Fail<ProductKitItemDto>($"Product '{product.ProductName}' is already in this kit");
                }

                item.ProductId = dto.ProductId;
            }

            item.DefaultQuantity = dto.DefaultQuantity;
            item.SortOrder = dto.SortOrder;
            item.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            // Recalculate totals
            await RecalculateTotalsAsync(item.ProductKitId);

            // Reload with navigation properties
            var updatedItem = await _context.ProductKitItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            return Result.Ok(MapItemToDto(updatedItem!));
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductKitItemDto>($"Error updating product kit item: {ex.Message}");
        }
    }

    public async Task<Result> DeleteItemAsync(Guid itemId)
    {
        try
        {
            var item = await _context.ProductKitItems
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return Result.Fail($"Product kit item with ID {itemId} not found");
            }

            var kitId = item.ProductKitId;

            _context.ProductKitItems.Remove(item);
            await _context.SaveChangesAsync();

            // Recalculate totals
            await RecalculateTotalsAsync(kitId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting product kit item: {ex.Message}");
        }
    }

    private async Task RecalculateTotalsAsync(Guid kitId)
    {
        var kit = await _context.ProductKits
            .Include(pk => pk.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(pk => pk.Id == kitId);

        if (kit == null) return;

        decimal totalCost = 0;
        decimal totalPrice = 0;

        foreach (var item in kit.Items)
        {
            var unitCost = item.Product.CostPrice ?? 0;
            var unitPrice = item.Product.SellPrice ?? item.Product.CostPrice ?? 0;
            totalCost += unitCost * item.DefaultQuantity;
            totalPrice += unitPrice * item.DefaultQuantity;
        }

        kit.TotalCost = totalCost;
        kit.TotalPrice = totalPrice;

        await _context.SaveChangesAsync();
    }

    private static ProductKitDto MapToDto(ProductKit kit)
    {
        return new ProductKitDto(
            kit.Id,
            kit.KitCode,
            kit.KitName,
            kit.Description,
            kit.CategoryId,
            kit.Category?.CategoryName,
            kit.IsActive,
            kit.Notes,
            kit.TotalCost,
            kit.TotalPrice,
            kit.Items.Count,
            kit.Items.OrderBy(i => i.SortOrder).Select(MapItemToDto).ToList(),
            kit.CreatedAt,
            kit.UpdatedAt
        );
    }

    private static ProductKitItemDto MapItemToDto(ProductKitItem item)
    {
        var unitCost = item.Product.CostPrice ?? 0;
        var unitPrice = item.Product.SellPrice ?? item.Product.CostPrice ?? 0;

        return new ProductKitItemDto(
            item.Id,
            item.ProductKitId,
            item.ProductId,
            item.Product.ProductCode,
            item.Product.ProductName,
            item.Product.UnitType,
            item.DefaultQuantity,
            unitCost,
            unitPrice,
            unitCost * item.DefaultQuantity,
            unitPrice * item.DefaultQuantity,
            item.SortOrder,
            item.Notes
        );
    }
}
