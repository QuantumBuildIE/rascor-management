namespace Rascor.Modules.StockManagement.Application.Features.BayLocations.DTOs;

public record BayLocationDto(
    Guid Id,
    string BayCode,
    string? BayName,
    Guid StockLocationId,
    string StockLocationCode,
    string StockLocationName,
    int? Capacity,
    bool IsActive,
    string? Notes
);
