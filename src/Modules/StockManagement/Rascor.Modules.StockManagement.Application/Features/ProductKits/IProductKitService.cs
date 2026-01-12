using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.ProductKits.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.ProductKits;

public interface IProductKitService
{
    Task<Result<PaginatedList<ProductKitListItemDto>>> GetAllAsync(
        string? search = null,
        Guid? categoryId = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortColumn = null,
        string? sortDirection = null);

    Task<Result<ProductKitDto>> GetByIdAsync(Guid id);
    Task<Result<ProductKitDto>> CreateAsync(CreateProductKitDto dto);
    Task<Result<ProductKitDto>> UpdateAsync(Guid id, UpdateProductKitDto dto);
    Task<Result> DeleteAsync(Guid id);

    // Kit Items
    Task<Result<ProductKitItemDto>> AddItemAsync(Guid kitId, CreateProductKitItemDto dto);
    Task<Result<ProductKitItemDto>> UpdateItemAsync(Guid itemId, UpdateProductKitItemDto dto);
    Task<Result> DeleteItemAsync(Guid itemId);
}
