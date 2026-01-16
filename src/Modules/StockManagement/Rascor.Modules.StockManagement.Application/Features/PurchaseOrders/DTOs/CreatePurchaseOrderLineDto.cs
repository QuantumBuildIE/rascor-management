namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

public record CreatePurchaseOrderLineDto(
    Guid ProductId,
    int QuantityOrdered,
    decimal UnitPrice,
    string? UnitType = null
);
