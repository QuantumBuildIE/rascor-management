namespace Rascor.Core.Application.Features.Employees.DTOs;

public record UpdateEmployeeDto(
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
    /// Geo tracker device ID for mobile geofence app integration (format: EVT####, e.g., "EVT0011")
    /// </summary>
    string? GeoTrackerID = null
);
