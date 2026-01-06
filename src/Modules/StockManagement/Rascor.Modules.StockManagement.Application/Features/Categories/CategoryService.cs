using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Categories.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly IStockManagementDbContext _context;

    public CategoryService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync()
    {
        try
        {
            var categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new CategoryDto(
                    c.Id,
                    c.CategoryName,
                    c.SortOrder,
                    c.IsActive
                ))
                .ToListAsync();

            return Result.Ok(categories);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<CategoryDto>>($"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto(
                    c.Id,
                    c.CategoryName,
                    c.SortOrder,
                    c.IsActive
                ))
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return Result.Fail<CategoryDto>($"Category with ID {id} not found");
            }

            return Result.Ok(category);
        }
        catch (Exception ex)
        {
            return Result.Fail<CategoryDto>($"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                CategoryName = dto.CategoryName,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDto(
                category.Id,
                category.CategoryName,
                category.SortOrder,
                category.IsActive
            );

            return Result.Ok(categoryDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<CategoryDto>($"Error creating category: {ex.Message}");
        }
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Result.Fail<CategoryDto>($"Category with ID {id} not found");
            }

            category.CategoryName = dto.CategoryName;
            category.SortOrder = dto.SortOrder;
            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDto(
                category.Id,
                category.CategoryName,
                category.SortOrder,
                category.IsActive
            );

            return Result.Ok(categoryDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<CategoryDto>($"Error updating category: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Result.Fail($"Category with ID {id} not found");
            }

            category.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting category: {ex.Message}");
        }
    }
}
