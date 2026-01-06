using Rascor.Core.Application.Features.Companies.DTOs;
using Rascor.Core.Application.Models;

namespace Rascor.Core.Application.Features.Companies;

public interface ICompanyService
{
    Task<Result<List<CompanyDto>>> GetAllAsync();
    Task<Result<PaginatedList<CompanyDto>>> GetPaginatedAsync(GetCompaniesQueryDto query);
    Task<Result<CompanyDto>> GetByIdAsync(Guid id);
    Task<Result<CompanyDto>> CreateAsync(CreateCompanyDto dto);
    Task<Result<CompanyDto>> UpdateAsync(Guid id, UpdateCompanyDto dto);
    Task<Result> DeleteAsync(Guid id);
}
