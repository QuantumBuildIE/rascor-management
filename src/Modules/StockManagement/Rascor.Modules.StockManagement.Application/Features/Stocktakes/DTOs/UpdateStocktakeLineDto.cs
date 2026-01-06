namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

public record UpdateStocktakeLineDto(
    int? CountedQuantity,
    string? VarianceReason = null
);
