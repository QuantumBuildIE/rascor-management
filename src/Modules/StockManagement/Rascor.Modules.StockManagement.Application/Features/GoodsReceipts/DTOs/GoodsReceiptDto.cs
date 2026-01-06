namespace Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

public record GoodsReceiptDto(
    Guid Id,
    string GrnNumber,
    Guid? PurchaseOrderId,
    string? PoNumber,
    Guid SupplierId,
    string SupplierName,
    string? DeliveryNoteRef,
    Guid LocationId,
    string LocationName,
    DateTime ReceiptDate,
    string ReceivedBy,
    string? Notes,
    List<GoodsReceiptLineDto> Lines
);
