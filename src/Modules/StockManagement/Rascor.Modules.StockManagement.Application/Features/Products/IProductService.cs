using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Products.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Products;

public interface IProductService
{
    Task<Result<List<ProductDto>>> GetAllAsync();
    Task<Result<PaginatedList<ProductDto>>> GetPaginatedAsync(GetProductsQueryDto query);
    Task<Result<ProductDto>> GetByIdAsync(Guid id);
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<Result> DeleteAsync(Guid id);
}
