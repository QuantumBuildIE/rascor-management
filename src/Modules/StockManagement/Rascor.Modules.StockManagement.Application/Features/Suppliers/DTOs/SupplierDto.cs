namespace Rascor.Modules.StockManagement.Application.Features.Suppliers.DTOs;

public record SupplierDto(
    Guid Id,
    string SupplierCode,
    string SupplierName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? PaymentTerms,
    bool IsActive
);
