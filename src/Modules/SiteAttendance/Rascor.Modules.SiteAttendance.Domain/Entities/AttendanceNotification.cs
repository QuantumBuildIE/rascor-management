using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Domain.Entities;

/// <summary>
/// Notification log for attendance-related notifications
/// </summary>
public class AttendanceNotification : TenantEntity
{
    public Guid EmployeeId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public NotificationReason Reason { get; private set; }
    public string? Message { get; private set; }
    public DateTime SentAt { get; private set; }
    public bool Delivered { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Guid? RelatedEventId { get; private set; }

    // Navigation properties
    public virtual Employee Employee { get; private set; } = null!;
    public virtual AttendanceEvent? RelatedEvent { get; private set; }

    private AttendanceNotification() { } // EF Core

    public static AttendanceNotification Create(
        Guid tenantId,
        Guid employeeId,
        NotificationType notificationType,
        NotificationReason reason,
        string? message = null,
        Guid? relatedEventId = null)
    {
        return new AttendanceNotification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            NotificationType = notificationType,
            Reason = reason,
            Message = message,
            SentAt = DateTime.UtcNow,
            Delivered = false,
            RelatedEventId = relatedEventId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkDelivered()
    {
        Delivered = true;
    }

    public void MarkFailed(string errorMessage)
    {
        Delivered = false;
        ErrorMessage = errorMessage;
    }
}
