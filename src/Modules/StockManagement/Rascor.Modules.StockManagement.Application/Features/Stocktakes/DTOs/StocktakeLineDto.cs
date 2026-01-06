namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

public record StocktakeLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int SystemQuantity,
    int? CountedQuantity,
    int? Variance,
    bool AdjustmentCreated,
    string? VarianceReason,
    Guid? BayLocationId,
    string? BayCode
);
