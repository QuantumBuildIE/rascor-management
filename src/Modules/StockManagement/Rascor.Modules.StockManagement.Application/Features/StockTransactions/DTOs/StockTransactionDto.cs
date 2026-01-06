namespace Rascor.Modules.StockManagement.Application.Features.StockTransactions.DTOs;

public record StockTransactionDto(
    Guid Id,
    string TransactionNumber,
    DateTime TransactionDate,
    string TransactionType,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid LocationId,
    string LocationCode,
    string LocationName,
    int Quantity,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes
);
