using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Registered mobile devices for GPS attendance tracking and push notifications
/// </summary>
public class DeviceRegistration : TenantEntity
{
    public Guid? EmployeeId { get; private set; }
    public string DeviceIdentifier { get; private set; } = string.Empty;
    public string? DeviceName { get; private set; }
    public string? Platform { get; private set; } // iOS, Android
    public string? PushToken { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? LastActiveAt { get; private set; }
    public bool IsActive { get; private set; }

    // Device linking tracking (for Zoho migration and audit)
    public DateTime? LinkedAt { get; private set; }
    public string? LinkedBy { get; private set; } // "Admin", "QRCode", "ManualCode", "EmailCode"
    public DateTime? UnlinkedAt { get; private set; }
    public string? UnlinkedReason { get; private set; }

    // Navigation properties
    public virtual Employee? Employee { get; private set; }
    public virtual ICollection<AttendanceEvent> AttendanceEvents { get; private set; } = new List<AttendanceEvent>();

    private DeviceRegistration() { } // EF Core

    public static DeviceRegistration Create(
        Guid tenantId,
        string deviceIdentifier,
        string? deviceName = null,
        string? platform = null,
        Guid? employeeId = null)
    {
        return new DeviceRegistration
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DeviceIdentifier = deviceIdentifier,
            DeviceName = deviceName,
            Platform = platform,
            EmployeeId = employeeId,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignToEmployee(Guid employeeId)
    {
        EmployeeId = employeeId;
    }

    /// <summary>
    /// Link the device to an employee with tracking metadata.
    /// Use this for admin linking and soft migration from Zoho.
    /// </summary>
    /// <param name="employeeId">The employee to link to</param>
    /// <param name="linkedBy">How the link was established: Admin, QRCode, ManualCode, EmailCode</param>
    public void LinkToEmployee(Guid employeeId, string linkedBy)
    {
        EmployeeId = employeeId;
        LinkedAt = DateTime.UtcNow;
        LinkedBy = linkedBy;
        UnlinkedAt = null;
        UnlinkedReason = null;
    }

    /// <summary>
    /// Unlink the device from its current employee.
    /// </summary>
    /// <param name="reason">Reason for unlinking (e.g., "Device lost", "Employee left", "Reassigned")</param>
    public void Unlink(string reason)
    {
        EmployeeId = null;
        UnlinkedAt = DateTime.UtcNow;
        UnlinkedReason = reason;
    }

    /// <summary>
    /// Reactivate a previously deactivated device.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        LastActiveAt = DateTime.UtcNow;
    }

    public void UpdatePushToken(string pushToken)
    {
        PushToken = pushToken;
        LastActiveAt = DateTime.UtcNow;
    }

    public void MarkActive()
    {
        LastActiveAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
