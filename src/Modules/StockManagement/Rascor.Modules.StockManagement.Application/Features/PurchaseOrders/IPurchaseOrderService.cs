using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;

public interface IPurchaseOrderService
{
    Task<Result<List<PurchaseOrderDto>>> GetAllAsync();
    Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id);
    Task<Result<List<PurchaseOrderDto>>> GetBySupplierAsync(Guid supplierId);
    Task<Result<List<PurchaseOrderDto>>> GetByStatusAsync(string status);
    Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto dto);
    Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto);
    Task<Result<PurchaseOrderDto>> ConfirmAsync(Guid id);
    Task<Result<PurchaseOrderDto>> CancelAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}
