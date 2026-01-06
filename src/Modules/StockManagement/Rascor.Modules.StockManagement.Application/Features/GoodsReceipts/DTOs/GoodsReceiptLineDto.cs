namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

public record GoodsReceiptLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid? PurchaseOrderLineId,
    int QuantityReceived,
    string? Notes,
    decimal QuantityRejected,
    string? RejectionReason,
    string? BatchNumber,
    DateTime? ExpiryDate,
    Guid? BayLocationId,
    string? BayCode
);
