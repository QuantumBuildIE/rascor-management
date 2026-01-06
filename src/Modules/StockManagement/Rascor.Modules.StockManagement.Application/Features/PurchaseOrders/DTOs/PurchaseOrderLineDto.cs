namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

public record PurchaseOrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int QuantityOrdered,
    int QuantityReceived,
    decimal UnitPrice,
    decimal LineTotal,
    string LineStatus
);
