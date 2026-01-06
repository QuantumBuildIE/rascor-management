namespace Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

public record PurchaseOrderDto(
    Guid Id,
    string PoNumber,
    Guid SupplierId,
    string SupplierName,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    string Status,
    decimal TotalValue,
    string? Notes,
    List<PurchaseOrderLineDto> Lines
);
