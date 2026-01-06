namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

public record CreateStocktakeDto(
    Guid LocationId,
    string CountedBy,
    string? Notes
);
