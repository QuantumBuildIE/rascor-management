using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Categories.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Categories;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetAllAsync();
    Task<Result<CategoryDto>> GetByIdAsync(Guid id);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto);
    Task<Result> DeleteAsync(Guid id);
}
