namespace Rascor.Modules.StockManagement.Application.Features.Categories.DTOs;

public record CreateCategoryDto(
    string CategoryName,
    int SortOrder,
    bool IsActive
);
