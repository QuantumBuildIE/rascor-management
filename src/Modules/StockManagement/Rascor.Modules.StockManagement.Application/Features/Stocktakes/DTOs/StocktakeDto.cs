namespace Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

public record StocktakeDto(
    Guid Id,
    string StocktakeNumber,
    Guid LocationId,
    string LocationCode,
    string LocationName,
    DateTime CountDate,
    string Status,
    string CountedBy,
    string? Notes,
    List<StocktakeLineDto> Lines
);
