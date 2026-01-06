namespace Rascor.Modules.StockManagement.Application.Features.Categories.DTOs;

public record UpdateCategoryDto(
    string CategoryName,
    int SortOrder,
    bool IsActive
);
