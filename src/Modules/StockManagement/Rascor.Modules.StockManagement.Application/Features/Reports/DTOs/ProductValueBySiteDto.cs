namespace Rascor.Modules.StockManagement.Application.Features.Reports.DTOs;

/// <summary>
/// Product value data grouped by site
/// </summary>
public record ProductValueBySiteDto(
    string SiteName,
    string ProductName,
    decimal Value
);
