namespace Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;

/// <summary>
/// Product value data grouped by month
/// </summary>
public record ProductValueByMonthDto(
    string Month,
    string ProductName,
    decimal Value
);
