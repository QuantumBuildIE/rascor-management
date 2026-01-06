namespace Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

public record StockOrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int QuantityRequested,
    int QuantityIssued,
    decimal UnitPrice,
    decimal LineTotal,
    string? BayCode = null
);
