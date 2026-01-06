using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Suppliers.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.Suppliers;

public interface ISupplierService
{
    Task<Result<List<SupplierDto>>> GetAllAsync();
    Task<Result<SupplierDto>> GetByIdAsync(Guid id);
    Task<Result<SupplierDto>> CreateAsync(CreateSupplierDto dto);
    Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierDto dto);
    Task<Result> DeleteAsync(Guid id);
}
