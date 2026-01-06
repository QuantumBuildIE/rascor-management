namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

public record UpdatePurchaseOrderLineDto(
    int QuantityOrdered,
    decimal UnitPrice
);
