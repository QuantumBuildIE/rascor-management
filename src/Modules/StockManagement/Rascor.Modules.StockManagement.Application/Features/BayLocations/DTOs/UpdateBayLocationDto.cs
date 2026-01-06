namespace Rascor.Modules.StockManagement.Application.Features.BayLocations.DTOs;

public record UpdateBayLocationDto(
    string BayCode,
    string? BayName,
    Guid StockLocationId,
    int? Capacity,
    bool IsActive,
    string? Notes
);
