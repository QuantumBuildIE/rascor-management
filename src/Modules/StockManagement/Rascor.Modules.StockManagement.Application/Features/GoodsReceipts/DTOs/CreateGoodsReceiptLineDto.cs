namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

public record CreateGoodsReceiptLineDto(
    Guid ProductId,
    Guid? PurchaseOrderLineId,
    int QuantityReceived,
    string? Notes,
    decimal QuantityRejected = 0,
    string? RejectionReason = null,
    string? BatchNumber = null,
    DateTime? ExpiryDate = null,
    Guid? BayLocationId = null
);
