using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts;

public interface IGoodsReceiptService
{
    Task<Result<List<GoodsReceiptDto>>> GetAllAsync();
    Task<Result<GoodsReceiptDto>> GetByIdAsync(Guid id);
    Task<Result<List<GoodsReceiptDto>>> GetBySupplierAsync(Guid supplierId);
    Task<Result<List<GoodsReceiptDto>>> GetByPurchaseOrderAsync(Guid purchaseOrderId);
    Task<Result<GoodsReceiptDto>> CreateAsync(CreateGoodsReceiptDto dto);
    Task<Result> DeleteAsync(Guid id);
}
