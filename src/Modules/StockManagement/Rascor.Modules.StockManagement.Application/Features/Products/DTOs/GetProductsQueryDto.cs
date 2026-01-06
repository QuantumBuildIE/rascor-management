namespace Rascor.Modules.StockManagement.Application.Features.Products.DTOs;

public record GetProductsQueryDto(
    int PageNumber = 1,
    int PageSize = 20,
    string? SortColumn = null,
    string? SortDirection = null,
    string? Search = null
);
