using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.StockOrders;

public interface IStockOrderService
{
    Task<Result<List<StockOrderDto>>> GetAllAsync();
    Task<Result<StockOrderDto>> GetByIdAsync(Guid id);
    Task<Result<StockOrderDto>> GetForDocketAsync(Guid id, Guid warehouseLocationId);
    Task<Result<List<StockOrderDto>>> GetBySiteAsync(Guid siteId);
    Task<Result<List<StockOrderDto>>> GetByStatusAsync(string status);
    Task<Result<StockOrderDto>> CreateAsync(CreateStockOrderDto dto);
    Task<Result<StockOrderDto>> SubmitAsync(Guid id);
    Task<Result<StockOrderDto>> ApproveAsync(Guid id, string approvedBy, Guid warehouseLocationId);
    Task<Result<StockOrderDto>> RejectAsync(Guid id, string rejectedBy, string reason);
    Task<Result<StockOrderDto>> ReadyForCollectionAsync(Guid id);
    Task<Result<StockOrderDto>> CollectAsync(Guid id, Guid warehouseLocationId);
    Task<Result<StockOrderDto>> CancelAsync(Guid id, Guid? warehouseLocationId);
    Task<Result> DeleteAsync(Guid id);
}
