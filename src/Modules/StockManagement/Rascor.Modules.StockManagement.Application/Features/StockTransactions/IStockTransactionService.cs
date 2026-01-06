using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockTransactions.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockTransactions;

public interface IStockTransactionService
{
    Task<Result<List<StockTransactionDto>>> GetAllAsync();
    Task<Result<StockTransactionDto>> GetByIdAsync(Guid id);
    Task<Result<List<StockTransactionDto>>> GetByProductAsync(Guid productId);
    Task<Result<List<StockTransactionDto>>> GetByLocationAsync(Guid locationId);
    Task<Result<List<StockTransactionDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Result<StockTransactionDto>> CreateAsync(CreateStockTransactionDto dto);
    Task<Result> DeleteAsync(Guid id);
}
