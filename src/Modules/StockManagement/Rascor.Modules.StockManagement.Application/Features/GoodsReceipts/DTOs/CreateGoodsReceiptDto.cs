namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

public record CreateGoodsReceiptDto(
    Guid? PurchaseOrderId,
    Guid SupplierId,
    string? DeliveryNoteRef,
    Guid LocationId,
    DateTime ReceiptDate,
    string ReceivedBy,
    string? Notes,
    List<CreateGoodsReceiptLineDto> Lines
);
