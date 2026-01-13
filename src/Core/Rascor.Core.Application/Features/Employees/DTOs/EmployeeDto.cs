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
    string? Notes,
    /// <summary>
    /// Geo tracker device ID for mobile geofence app integration (format: EVT####)
    /// </summary>
    string? GeoTrackerID = null,
    /// <summary>
    /// Indicates whether this employee has a linked User account
    /// </summary>
    bool HasUserAccount = false,
    /// <summary>
    /// The linked User ID if a user account exists
    /// </summary>
    Guid? LinkedUserId = null
);
