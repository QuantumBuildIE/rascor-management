namespace Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

public record CreateStockLevelDto(
    Guid ProductId,
    Guid LocationId,
    int QuantityOnHand,
    string? BinLocation
);
