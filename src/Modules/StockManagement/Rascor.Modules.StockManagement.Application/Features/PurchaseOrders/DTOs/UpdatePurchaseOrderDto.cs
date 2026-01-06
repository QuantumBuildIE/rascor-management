namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

public record UpdatePurchaseOrderDto(
    DateTime? ExpectedDate,
    string? Notes
);
