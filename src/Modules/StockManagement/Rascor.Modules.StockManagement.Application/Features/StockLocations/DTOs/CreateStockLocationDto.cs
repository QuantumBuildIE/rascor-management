namespace Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;

public record CreateStockLocationDto(
    string LocationCode,
    string LocationName,
    string LocationType,
    string? Address,
    bool IsActive
);
