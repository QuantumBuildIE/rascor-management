namespace Rascor.Modules.StockManagement.Application.Features.StockTransactions.DTOs;

public record CreateStockTransactionDto(
    string TransactionType,
    Guid ProductId,
    Guid LocationId,
    int Quantity,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes
);
