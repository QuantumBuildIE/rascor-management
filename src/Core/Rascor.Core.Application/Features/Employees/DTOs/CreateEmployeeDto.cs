namespace Rascor.Core.Application.Features.Employees.DTOs;

public record CreateEmployeeDto(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? JobTitle,
    string? Department,
    Guid? PrimarySiteId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive,
    string? Notes,
    /// <summary>
    /// If true (default), creates a linked User account when Email is provided.
    /// The user will receive a password setup email.
    /// </summary>
    bool CreateUserAccount = true,
    /// <summary>
    /// Optional role name to assign to the created user.
    /// Defaults to "SiteManager" if not specified.
    /// Valid roles: Admin, Finance, OfficeStaff, SiteManager, WarehouseStaff
    /// </summary>
    string? UserRole = null
);
