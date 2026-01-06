namespace Rascor.Core.Application.Features.Employees.DTOs;

public record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? JobTitle,
    string? Department,
    Guid? PrimarySiteId,
    string? PrimarySiteName,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive,
    string? Notes
);
