namespace Rascor.Modules.StockManagement.Application.Features.ProductKits.DTOs;

public record ProductKitDto(
    Guid Id,
    string KitCode,
    string KitName,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    bool IsActive,
    string? Notes,
    decimal TotalCost,
    decimal TotalPrice,
    int ItemCount,
    List<ProductKitItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProductKitListItemDto(
    Guid Id,
    string KitCode,
    string KitName,
    string? CategoryName,
    bool IsActive,
    decimal TotalCost,
    decimal TotalPrice,
    int ItemCount
);

public record ProductKitItemDto(
    Guid Id,
    Guid ProductKitId,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    decimal DefaultQuantity,
    decimal UnitCost,
    decimal UnitPrice,
    decimal LineCost,
    decimal LinePrice,
    int SortOrder,
    string? Notes
);

public record CreateProductKitDto(
    string KitCode,
    string KitName,
    string? Description,
    Guid? CategoryId,
    bool IsActive,
    string? Notes,
    List<CreateProductKitItemDto>? Items
);

public record UpdateProductKitDto(
    string KitCode,
    string KitName,
    string? Description,
    Guid? CategoryId,
    bool IsActive,
    string? Notes
);

public record CreateProductKitItemDto(
    Guid ProductId,
    decimal DefaultQuantity,
    int SortOrder,
    string? Notes
);

public record UpdateProductKitItemDto(
    Guid ProductId,
    decimal DefaultQuantity,
    int SortOrder,
    string? Notes
);
