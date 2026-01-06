namespace Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;

public record StockLocationDto(
    Guid Id,
    string LocationCode,
    string LocationName,
    string LocationType,
    string? Address,
    bool IsActive
);
