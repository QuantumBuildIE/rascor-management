namespace Rascor.Modules.StockManagement.Application.Features.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    string CategoryName,
    int SortOrder,
    bool IsActive
);
