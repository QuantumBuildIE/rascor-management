namespace Rascor.Modules.StockManagement.Application.Features.Suppliers.DTOs;

public record UpdateSupplierDto(
    string SupplierCode,
    string SupplierName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? PaymentTerms,
    bool IsActive
);
