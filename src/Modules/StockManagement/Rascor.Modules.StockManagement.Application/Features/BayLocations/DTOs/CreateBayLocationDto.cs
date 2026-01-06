namespace Rascor.Modules.StockManagement.Application.Features.BayLocations.DTOs;

public record CreateBayLocationDto(
    string BayCode,
    string? BayName,
    Guid StockLocationId,
    int? Capacity,
    bool IsActive = true,
    string? Notes = null
);
