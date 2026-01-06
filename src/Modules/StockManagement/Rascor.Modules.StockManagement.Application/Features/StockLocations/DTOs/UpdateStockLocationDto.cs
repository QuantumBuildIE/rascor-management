namespace Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;

public record UpdateStockLocationDto(
    string LocationCode,
    string LocationName,
    string LocationType,
    string? Address,
    bool IsActive
);
