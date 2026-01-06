namespace Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;

/// <summary>
/// Stock valuation report with items and summary totals
/// </summary>
public record StockValuationReportDto(
    List<StockValuationItemDto> Items,
    int TotalProducts,
    decimal TotalQuantity,
    decimal TotalValue,
    DateTime GeneratedAt
);
