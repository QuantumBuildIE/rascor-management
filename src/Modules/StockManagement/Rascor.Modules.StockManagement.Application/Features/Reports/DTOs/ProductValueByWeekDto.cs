namespace Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;

/// <summary>
/// Product value data grouped by week
/// </summary>
public record ProductValueByWeekDto(
    DateTime WeekStartDate,
    string ProductName,
    decimal Value
);
