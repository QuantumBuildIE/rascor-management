namespace Rascor.Modules.StockManagement.Application.Features.Products.DTOs;

public record UpdateProductDto(
    string ProductCode,
    string ProductName,
    Guid CategoryId,
    Guid? SupplierId,
    string UnitType,
    decimal BaseRate,
    int ReorderLevel,
    int ReorderQuantity,
    int LeadTimeDays,
    bool IsActive,
    decimal? CostPrice,
    decimal? SellPrice,
    string? ProductType
);
