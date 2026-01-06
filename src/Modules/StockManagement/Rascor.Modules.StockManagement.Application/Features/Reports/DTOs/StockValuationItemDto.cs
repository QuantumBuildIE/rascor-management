namespace Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;

/// <summary>
/// Individual stock valuation item
/// </summary>
public record StockValuationItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid? CategoryId,
    string? CategoryName,
    Guid LocationId,
    string LocationName,
    string? BayCode,
    decimal QuantityOnHand,
    decimal? CostPrice,
    decimal TotalValue
);
