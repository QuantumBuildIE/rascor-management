namespace Rascor.Modules.SiteAttendance.Application.DTOs;

/// <summary>
/// Device information for admin listing view
/// </summary>
public record AdminDeviceListDto
{
    /// <summary>
    /// Unique identifier (Guid)
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Device identifier string (e.g., EVT0001)
    /// </summary>
    public string DeviceIdentifier { get; init; } = null!;

    /// <summary>
    /// Platform: iOS, Android
    /// </summary>
    public string? Platform { get; init; }

    /// <summary>
    /// Device name or model
    /// </summary>
    public string? DeviceName { get; init; }

    /// <summary>
    /// When the device was first registered
    /// </summary>
    public DateTime RegisteredAt { get; init; }

    /// <summary>
    /// Last time the device was active (sent a request)
    /// </summary>
    public DateTime? LastActiveAt { get; init; }

    /// <summary>
    /// Whether the device is currently active
    /// </summary>
    public bool IsActive { get; init; }

    // Linked employee info
    public Guid? EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? EmployeeEmail { get; init; }
    public DateTime? LinkedAt { get; init; }
    public string? LinkedBy { get; init; }

    /// <summary>
    /// Whether the device is linked to an employee
    /// </summary>
    public bool IsLinked => EmployeeId.HasValue;
}

/// <summary>
/// Device detail information including unlinking history
/// </summary>
public record AdminDeviceDetailDto : AdminDeviceListDto
{
    /// <summary>
    /// Push notification token
    /// </summary>
    public string? PushToken { get; init; }

    /// <summary>
    /// When the device was last unlinked
    /// </summary>
    public DateTime? UnlinkedAt { get; init; }

    /// <summary>
    /// Reason provided when device was unlinked
    /// </summary>
    public string? UnlinkedReason { get; init; }
}

/// <summary>
/// Request to link a device to an employee
/// </summary>
public record LinkDeviceRequest
{
    /// <summary>
    /// The employee to link the device to
    /// </summary>
    public Guid EmployeeId { get; init; }
}

/// <summary>
/// Request to unlink a device from its employee
/// </summary>
public record UnlinkDeviceRequest
{
    /// <summary>
    /// Reason for unlinking (e.g., "Device lost", "Employee left", "Reassigned")
    /// </summary>
    public string Reason { get; init; } = null!;
}
