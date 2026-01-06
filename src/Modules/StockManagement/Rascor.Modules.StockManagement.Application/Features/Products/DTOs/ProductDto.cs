namespace Rascor.Modules.StockManagement.Application.Features.Products.DTOs;

public record ProductDto(
    Guid Id,
    string ProductCode,
    string ProductName,
    Guid CategoryId,
    string CategoryName,
    Guid? SupplierId,
    string? SupplierName,
    string UnitType,
    decimal BaseRate,
    int ReorderLevel,
    int ReorderQuantity,
    int LeadTimeDays,
    bool IsActive,
    string? QrCodeData,
    decimal? CostPrice,
    decimal? SellPrice,
    string? ProductType,
    decimal? MarginAmount,
    decimal? MarginPercent,
    string? ImageFileName,
    string? ImageUrl
);
