namespace Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

public record CreateStockOrderLineDto(
    Guid ProductId,
    int QuantityRequested
);
