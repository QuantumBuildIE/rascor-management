namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

public record CreatePurchaseOrderDto(
    Guid SupplierId,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    string? Notes,
    List<CreatePurchaseOrderLineDto> Lines
);
