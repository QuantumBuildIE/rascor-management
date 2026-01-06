using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes;

public interface IStocktakeService
{
    Task<Result<List<StocktakeDto>>> GetAllAsync();
    Task<Result<StocktakeDto>> GetByIdAsync(Guid id);
    Task<Result<List<StocktakeDto>>> GetByLocationAsync(Guid locationId);
    Task<Result<StocktakeDto>> CreateAsync(CreateStocktakeDto dto);
    Task<Result<StocktakeDto>> StartAsync(Guid id);
    Task<Result<StocktakeDto>> UpdateLineAsync(Guid stocktakeId, Guid lineId, UpdateStocktakeLineDto dto);
    Task<Result<StocktakeDto>> CompleteAsync(Guid id);
    Task<Result<StocktakeDto>> CancelAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}
