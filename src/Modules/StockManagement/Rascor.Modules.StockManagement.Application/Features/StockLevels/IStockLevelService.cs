using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockLevels;

public interface IStockLevelService
{
    Task<Result<List<StockLevelDto>>> GetAllAsync();
    Task<Result<StockLevelDto>> GetByIdAsync(Guid id);
    Task<Result<StockLevelDto>> GetByProductAndLocationAsync(Guid productId, Guid locationId);
    Task<Result<List<StockLevelDto>>> GetByLocationAsync(Guid locationId);
    Task<Result<List<StockLevelDto>>> GetLowStockAsync();
    Task<Result<StockLevelDto>> CreateAsync(CreateStockLevelDto dto);
    Task<Result<StockLevelDto>> UpdateAsync(Guid id, UpdateStockLevelDto dto);
    Task<Result> DeleteAsync(Guid id);
}
