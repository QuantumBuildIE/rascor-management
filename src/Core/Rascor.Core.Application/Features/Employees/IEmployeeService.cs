using Rascor.Core.Application.Features.Employees.DTOs;
using Rascor.Core.Application.Models;

namespace Rascor.Core.Application.Features.Employees;

public interface IEmployeeService
{
    Task<Result<List<EmployeeDto>>> GetAllAsync();
    Task<Result<PaginatedList<EmployeeDto>>> GetPaginatedAsync(GetEmployeesQueryDto query);
    Task<Result<EmployeeDto>> GetByIdAsync(Guid id);
    Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto dto);
    Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<List<EmployeeDto>>> GetUnlinkedAsync();

    /// <summary>
    /// Resends the welcome/password setup email to an employee with a linked user account
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> ResendInviteAsync(Guid employeeId);
}
