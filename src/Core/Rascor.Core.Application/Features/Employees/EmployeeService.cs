using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Features.Employees.DTOs;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Application.Features.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly ICoreDbContext _context;

    public EmployeeService(ICoreDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<EmployeeDto>>> GetAllAsync()
    {
        try
        {
            var employees = await _context.Employees
                .Include(e => e.PrimarySite)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.FirstName + " " + e.LastName,
                    e.Email,
                    e.Phone,
                    e.Mobile,
                    e.JobTitle,
                    e.Department,
                    e.PrimarySiteId,
                    e.PrimarySite != null ? e.PrimarySite.SiteName : null,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    e.Notes
                ))
                .ToListAsync();

            return Result.Ok(employees);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<EmployeeDto>>($"Error retrieving employees: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<EmployeeDto>>> GetPaginatedAsync(GetEmployeesQueryDto query)
    {
        try
        {
            var employeesQuery = _context.Employees
                .Include(e => e.PrimarySite)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchLower = query.Search.ToLower();
                employeesQuery = employeesQuery.Where(e =>
                    e.EmployeeCode.ToLower().Contains(searchLower) ||
                    e.FirstName.ToLower().Contains(searchLower) ||
                    e.LastName.ToLower().Contains(searchLower) ||
                    (e.FirstName + " " + e.LastName).ToLower().Contains(searchLower) ||
                    (e.Email != null && e.Email.ToLower().Contains(searchLower)) ||
                    (e.JobTitle != null && e.JobTitle.ToLower().Contains(searchLower))
                );
            }

            // Apply sorting
            employeesQuery = ApplySorting(employeesQuery, query.SortColumn, query.SortDirection);

            // Get total count before pagination
            var totalCount = await employeesQuery.CountAsync();

            // Apply pagination
            var employees = await employeesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.FirstName + " " + e.LastName,
                    e.Email,
                    e.Phone,
                    e.Mobile,
                    e.JobTitle,
                    e.Department,
                    e.PrimarySiteId,
                    e.PrimarySite != null ? e.PrimarySite.SiteName : null,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    e.Notes
                ))
                .ToListAsync();

            var result = new PaginatedList<EmployeeDto>(employees, totalCount, query.PageNumber, query.PageSize);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<PaginatedList<EmployeeDto>>($"Error retrieving employees: {ex.Message}");
        }
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortColumn, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn?.ToLower() switch
        {
            "employeecode" => isDescending ? query.OrderByDescending(e => e.EmployeeCode) : query.OrderBy(e => e.EmployeeCode),
            "firstname" => isDescending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
            "fullname" or "name" => isDescending
                ? query.OrderByDescending(e => e.FirstName + " " + e.LastName)
                : query.OrderBy(e => e.FirstName + " " + e.LastName),
            "email" => isDescending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
            "jobtitle" => isDescending ? query.OrderByDescending(e => e.JobTitle) : query.OrderBy(e => e.JobTitle),
            "primarysitename" => isDescending
                ? query.OrderByDescending(e => e.PrimarySite != null ? e.PrimarySite.SiteName : "")
                : query.OrderBy(e => e.PrimarySite != null ? e.PrimarySite.SiteName : ""),
            "isactive" => isDescending ? query.OrderByDescending(e => e.IsActive) : query.OrderBy(e => e.IsActive),
            _ => query.OrderBy(e => e.EmployeeCode)
        };
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.PrimarySite)
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDto(
                    e.Id,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.FirstName + " " + e.LastName,
                    e.Email,
                    e.Phone,
                    e.Mobile,
                    e.JobTitle,
                    e.Department,
                    e.PrimarySiteId,
                    e.PrimarySite != null ? e.PrimarySite.SiteName : null,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    e.Notes
                ))
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return Result.Fail<EmployeeDto>($"Employee with ID {id} not found");
            }

            return Result.Ok(employee);
        }
        catch (Exception ex)
        {
            return Result.Fail<EmployeeDto>($"Error retrieving employee: {ex.Message}");
        }
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        try
        {
            // Validate that PrimarySiteId exists if provided
            if (dto.PrimarySiteId.HasValue)
            {
                var siteExists = await _context.Sites
                    .AnyAsync(s => s.Id == dto.PrimarySiteId.Value);

                if (!siteExists)
                {
                    return Result.Fail<EmployeeDto>($"Site with ID {dto.PrimarySiteId} not found");
                }
            }

            // Check for duplicate EmployeeCode within the same tenant
            var duplicateCode = await _context.Employees
                .AnyAsync(e => e.EmployeeCode == dto.EmployeeCode);

            if (duplicateCode)
            {
                return Result.Fail<EmployeeDto>($"Employee with code '{dto.EmployeeCode}' already exists");
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = dto.EmployeeCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                JobTitle = dto.JobTitle,
                Department = dto.Department,
                PrimarySiteId = dto.PrimarySiteId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                Notes = dto.Notes
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Reload with related entities to get site name
            var createdEmployee = await _context.Employees
                .Include(e => e.PrimarySite)
                .FirstAsync(e => e.Id == employee.Id);

            var employeeDto = new EmployeeDto(
                createdEmployee.Id,
                createdEmployee.EmployeeCode,
                createdEmployee.FirstName,
                createdEmployee.LastName,
                createdEmployee.FirstName + " " + createdEmployee.LastName,
                createdEmployee.Email,
                createdEmployee.Phone,
                createdEmployee.Mobile,
                createdEmployee.JobTitle,
                createdEmployee.Department,
                createdEmployee.PrimarySiteId,
                createdEmployee.PrimarySite?.SiteName,
                createdEmployee.StartDate,
                createdEmployee.EndDate,
                createdEmployee.IsActive,
                createdEmployee.Notes
            );

            return Result.Ok(employeeDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<EmployeeDto>($"Error creating employee: {ex.Message}");
        }
    }

    public async Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.PrimarySite)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return Result.Fail<EmployeeDto>($"Employee with ID {id} not found");
            }

            // Validate that PrimarySiteId exists if provided
            if (dto.PrimarySiteId.HasValue)
            {
                var siteExists = await _context.Sites
                    .AnyAsync(s => s.Id == dto.PrimarySiteId.Value);

                if (!siteExists)
                {
                    return Result.Fail<EmployeeDto>($"Site with ID {dto.PrimarySiteId} not found");
                }
            }

            // Check for duplicate EmployeeCode (excluding current employee)
            var duplicateCode = await _context.Employees
                .AnyAsync(e => e.EmployeeCode == dto.EmployeeCode && e.Id != id);

            if (duplicateCode)
            {
                return Result.Fail<EmployeeDto>($"Employee with code '{dto.EmployeeCode}' already exists");
            }

            employee.EmployeeCode = dto.EmployeeCode;
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.Mobile = dto.Mobile;
            employee.JobTitle = dto.JobTitle;
            employee.Department = dto.Department;
            employee.PrimarySiteId = dto.PrimarySiteId;
            employee.StartDate = dto.StartDate;
            employee.EndDate = dto.EndDate;
            employee.IsActive = dto.IsActive;
            employee.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            // Reload to get updated related entity name
            await _context.Employees
                .Entry(employee)
                .Reference(e => e.PrimarySite)
                .LoadAsync();

            var employeeDto = new EmployeeDto(
                employee.Id,
                employee.EmployeeCode,
                employee.FirstName,
                employee.LastName,
                employee.FirstName + " " + employee.LastName,
                employee.Email,
                employee.Phone,
                employee.Mobile,
                employee.JobTitle,
                employee.Department,
                employee.PrimarySiteId,
                employee.PrimarySite?.SiteName,
                employee.StartDate,
                employee.EndDate,
                employee.IsActive,
                employee.Notes
            );

            return Result.Ok(employeeDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<EmployeeDto>($"Error updating employee: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return Result.Fail($"Employee with ID {id} not found");
            }

            employee.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting employee: {ex.Message}");
        }
    }
}
