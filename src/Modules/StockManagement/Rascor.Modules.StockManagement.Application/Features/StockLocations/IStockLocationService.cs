using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockLocations;

public interface IStockLocationService
{
    Task<Result<List<StockLocationDto>>> GetAllAsync();
    Task<Result<StockLocationDto>> GetByIdAsync(Guid id);
    Task<Result<StockLocationDto>> CreateAsync(CreateStockLocationDto dto);
    Task<Result<StockLocationDto>> UpdateAsync(Guid id, UpdateStockLocationDto dto);
    Task<Result> DeleteAsync(Guid id);
}
