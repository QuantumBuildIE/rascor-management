namespace Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

public record UpdateStockLevelDto(
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityOnOrder,
    string? BinLocation,
    Guid? BayLocationId
);
