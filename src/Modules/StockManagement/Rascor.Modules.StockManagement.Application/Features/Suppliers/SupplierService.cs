using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Suppliers.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Features.Suppliers;

public class SupplierService : ISupplierService
{
    private readonly IStockManagementDbContext _context;

    public SupplierService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SupplierDto>>> GetAllAsync()
    {
        try
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .Select(s => new SupplierDto(
                    s.Id,
                    s.SupplierCode,
                    s.SupplierName,
                    s.ContactName,
                    s.Email,
                    s.Phone,
                    s.Address,
                    s.PaymentTerms,
                    s.IsActive
                ))
                .ToListAsync();

            return Result.Ok(suppliers);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<SupplierDto>>($"Error retrieving suppliers: {ex.Message}");
        }
    }

    public async Task<Result<SupplierDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var supplier = await _context.Suppliers
                .Where(s => s.Id == id)
                .Select(s => new SupplierDto(
                    s.Id,
                    s.SupplierCode,
                    s.SupplierName,
                    s.ContactName,
                    s.Email,
                    s.Phone,
                    s.Address,
                    s.PaymentTerms,
                    s.IsActive
                ))
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                return Result.Fail<SupplierDto>($"Supplier with ID {id} not found");
            }

            return Result.Ok(supplier);
        }
        catch (Exception ex)
        {
            return Result.Fail<SupplierDto>($"Error retrieving supplier: {ex.Message}");
        }
    }

    public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierDto dto)
    {
        try
        {
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                SupplierCode = dto.SupplierCode,
                SupplierName = dto.SupplierName,
                ContactName = dto.ContactName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                PaymentTerms = dto.PaymentTerms,
                IsActive = dto.IsActive
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            var supplierDto = new SupplierDto(
                supplier.Id,
                supplier.SupplierCode,
                supplier.SupplierName,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address,
                supplier.PaymentTerms,
                supplier.IsActive
            );

            return Result.Ok(supplierDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<SupplierDto>($"Error creating supplier: {ex.Message}");
        }
    }

    public async Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierDto dto)
    {
        try
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return Result.Fail<SupplierDto>($"Supplier with ID {id} not found");
            }

            supplier.SupplierCode = dto.SupplierCode;
            supplier.SupplierName = dto.SupplierName;
            supplier.ContactName = dto.ContactName;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;
            supplier.PaymentTerms = dto.PaymentTerms;
            supplier.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var supplierDto = new SupplierDto(
                supplier.Id,
                supplier.SupplierCode,
                supplier.SupplierName,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address,
                supplier.PaymentTerms,
                supplier.IsActive
            );

            return Result.Ok(supplierDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<SupplierDto>($"Error updating supplier: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return Result.Fail($"Supplier with ID {id} not found");
            }

            supplier.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting supplier: {ex.Message}");
        }
    }
}
