using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.StockTransactions.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Modules.StockManagement.Application.Features.StockTransactions;

public class StockTransactionService : IStockTransactionService
{
    private readonly IStockManagementDbContext _context;

    public StockTransactionService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StockTransactionDto>>> GetAllAsync()
    {
        try
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Location)
                .OrderByDescending(st => st.TransactionDate)
                .ThenByDescending(st => st.TransactionNumber)
                .Select(st => new StockTransactionDto(
                    st.Id,
                    st.TransactionNumber,
                    st.TransactionDate,
                    st.TransactionType.ToString(),
                    st.ProductId,
                    st.Product.ProductCode,
                    st.Product.ProductName,
                    st.LocationId,
                    st.Location.LocationCode,
                    st.Location.LocationName,
                    st.Quantity,
                    st.ReferenceType,
                    st.ReferenceId,
                    st.Notes
                ))
                .ToListAsync();

            return Result.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockTransactionDto>>($"Error retrieving stock transactions: {ex.Message}");
        }
    }

    public async Task<Result<StockTransactionDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var transaction = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Location)
                .Where(st => st.Id == id)
                .Select(st => new StockTransactionDto(
                    st.Id,
                    st.TransactionNumber,
                    st.TransactionDate,
                    st.TransactionType.ToString(),
                    st.ProductId,
                    st.Product.ProductCode,
                    st.Product.ProductName,
                    st.LocationId,
                    st.Location.LocationCode,
                    st.Location.LocationName,
                    st.Quantity,
                    st.ReferenceType,
                    st.ReferenceId,
                    st.Notes
                ))
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return Result.Fail<StockTransactionDto>($"Stock transaction with ID {id} not found");
            }

            return Result.Ok(transaction);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockTransactionDto>($"Error retrieving stock transaction: {ex.Message}");
        }
    }

    public async Task<Result<List<StockTransactionDto>>> GetByProductAsync(Guid productId)
    {
        try
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Location)
                .Where(st => st.ProductId == productId)
                .OrderByDescending(st => st.TransactionDate)
                .ThenByDescending(st => st.TransactionNumber)
                .Select(st => new StockTransactionDto(
                    st.Id,
                    st.TransactionNumber,
                    st.TransactionDate,
                    st.TransactionType.ToString(),
                    st.ProductId,
                    st.Product.ProductCode,
                    st.Product.ProductName,
                    st.LocationId,
                    st.Location.LocationCode,
                    st.Location.LocationName,
                    st.Quantity,
                    st.ReferenceType,
                    st.ReferenceId,
                    st.Notes
                ))
                .ToListAsync();

            return Result.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockTransactionDto>>($"Error retrieving stock transactions by product: {ex.Message}");
        }
    }

    public async Task<Result<List<StockTransactionDto>>> GetByLocationAsync(Guid locationId)
    {
        try
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Location)
                .Where(st => st.LocationId == locationId)
                .OrderByDescending(st => st.TransactionDate)
                .ThenByDescending(st => st.TransactionNumber)
                .Select(st => new StockTransactionDto(
                    st.Id,
                    st.TransactionNumber,
                    st.TransactionDate,
                    st.TransactionType.ToString(),
                    st.ProductId,
                    st.Product.ProductCode,
                    st.Product.ProductName,
                    st.LocationId,
                    st.Location.LocationCode,
                    st.Location.LocationName,
                    st.Quantity,
                    st.ReferenceType,
                    st.ReferenceId,
                    st.Notes
                ))
                .ToListAsync();

            return Result.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockTransactionDto>>($"Error retrieving stock transactions by location: {ex.Message}");
        }
    }

    public async Task<Result<List<StockTransactionDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var transactions = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Location)
                .Where(st => st.TransactionDate >= startDate && st.TransactionDate <= endDate)
                .OrderByDescending(st => st.TransactionDate)
                .ThenByDescending(st => st.TransactionNumber)
                .Select(st => new StockTransactionDto(
                    st.Id,
                    st.TransactionNumber,
                    st.TransactionDate,
                    st.TransactionType.ToString(),
                    st.ProductId,
                    st.Product.ProductCode,
                    st.Product.ProductName,
                    st.LocationId,
                    st.Location.LocationCode,
                    st.Location.LocationName,
                    st.Quantity,
                    st.ReferenceType,
                    st.ReferenceId,
                    st.Notes
                ))
                .ToListAsync();

            return Result.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<StockTransactionDto>>($"Error retrieving stock transactions by date range: {ex.Message}");
        }
    }

    public async Task<Result<StockTransactionDto>> CreateAsync(CreateStockTransactionDto dto)
    {
        try
        {
            // Parse the transaction type
            if (!Enum.TryParse<TransactionType>(dto.TransactionType, ignoreCase: true, out var transactionType))
            {
                return Result.Fail<StockTransactionDto>($"Invalid transaction type: {dto.TransactionType}");
            }

            // Validate that ProductId exists
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                return Result.Fail<StockTransactionDto>($"Product with ID {dto.ProductId} not found");
            }

            // Validate that LocationId exists
            var locationExists = await _context.StockLocations
                .AnyAsync(l => l.Id == dto.LocationId);

            if (!locationExists)
            {
                return Result.Fail<StockTransactionDto>($"Stock location with ID {dto.LocationId} not found");
            }

            // Generate transaction number (format: TXN-YYYYMMDD-001)
            var transactionNumber = await GenerateTransactionNumberAsync();

            var transaction = new StockTransaction
            {
                Id = Guid.NewGuid(),
                TransactionNumber = transactionNumber,
                TransactionDate = DateTime.UtcNow,
                TransactionType = transactionType,
                ProductId = dto.ProductId,
                LocationId = dto.LocationId,
                Quantity = dto.Quantity,
                ReferenceType = dto.ReferenceType,
                ReferenceId = dto.ReferenceId,
                Notes = dto.Notes
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Reload with related entities
            var createdTransaction = await _context.StockTransactions
                .Include(st => st.Product)
                .Include(st => st.Location)
                .FirstAsync(st => st.Id == transaction.Id);

            var transactionDto = new StockTransactionDto(
                createdTransaction.Id,
                createdTransaction.TransactionNumber,
                createdTransaction.TransactionDate,
                createdTransaction.TransactionType.ToString(),
                createdTransaction.ProductId,
                createdTransaction.Product.ProductCode,
                createdTransaction.Product.ProductName,
                createdTransaction.LocationId,
                createdTransaction.Location.LocationCode,
                createdTransaction.Location.LocationName,
                createdTransaction.Quantity,
                createdTransaction.ReferenceType,
                createdTransaction.ReferenceId,
                createdTransaction.Notes
            );

            return Result.Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<StockTransactionDto>($"Error creating stock transaction: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var transaction = await _context.StockTransactions
                .FirstOrDefaultAsync(st => st.Id == id);

            if (transaction == null)
            {
                return Result.Fail($"Stock transaction with ID {id} not found");
            }

            transaction.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting stock transaction: {ex.Message}");
        }
    }

    private async Task<string> GenerateTransactionNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var datePrefix = today.ToString("yyyyMMdd");
        var prefix = $"TXN-{datePrefix}-";

        // Find the highest sequence number for today
        var lastTransaction = await _context.StockTransactions
            .Where(st => st.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(st => st.TransactionNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastTransaction != null)
        {
            var lastNumber = lastTransaction.TransactionNumber;
            var sequencePart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(sequencePart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D3}";
    }
}
