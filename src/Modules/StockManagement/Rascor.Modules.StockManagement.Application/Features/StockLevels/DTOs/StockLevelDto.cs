namespace Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

public record StockLevelDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid LocationId,
    string LocationCode,
    string LocationName,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable,
    int QuantityOnOrder,
    string? BinLocation,
    Guid? BayLocationId,
    string? BayCode,
    string? BayName,
    DateTime? LastMovementDate,
    DateTime? LastCountDate,
    int ReorderLevel
);
