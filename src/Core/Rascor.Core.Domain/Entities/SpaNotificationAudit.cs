using Rascor.Core.Domain.Common;

namespace Rascor.Core.Domain.Entities;

/// <summary>
/// Audit record for SPA (Site Photo Attendance) reminder notifications.
/// Tracks all notifications sent for SPA compliance.
/// </summary>
public class SpaNotificationAudit : TenantEntity
{
    /// <summary>
    /// Employee who should submit the SPA
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Navigation property to Employee
    /// </summary>
    public Employee Employee { get; set; } = null!;

    /// <summary>
    /// Site where the employee was scheduled to work
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation property to Site
    /// </summary>
    public Site Site { get; set; } = null!;

    /// <summary>
    /// Float task ID that triggered this notification (if from Float schedule)
    /// </summary>
    public int? FloatTaskId { get; set; }

    /// <summary>
    /// Float person ID (denormalized from Employee for audit purposes)
    /// </summary>
    public int? FloatPersonId { get; set; }

    /// <summary>
    /// Float project ID (denormalized from Site for audit purposes)
    /// </summary>
    public int? FloatProjectId { get; set; }

    /// <summary>
    /// Date the employee was scheduled to work
    /// </summary>
    public DateOnly ScheduledDate { get; set; }

    /// <summary>
    /// Hours scheduled for this date (from Float task)
    /// </summary>
    public decimal? ScheduledHours { get; set; }

    /// <summary>
    /// Type of notification: "FloatSpaReminder", "GeofenceSpaReminder"
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>
    /// Method of notification: "Email", "Push"
    /// </summary>
    public string NotificationMethod { get; set; } = string.Empty;

    /// <summary>
    /// Email address the notification was sent to (for email notifications)
    /// </summary>
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Status of the notification: "Pending", "Sent", "Failed", "SpaSubmittedBeforeSend"
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// When the notification was sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Error message if the notification failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Email provider message ID (e.g., MailerSend message ID) for tracking
    /// </summary>
    public string? EmailProviderId { get; set; }

    /// <summary>
    /// Whether the employee eventually submitted an SPA for this date
    /// </summary>
    public bool SpaSubmitted { get; set; }

    /// <summary>
    /// Link to the SitePhotoAttendance record if submitted
    /// </summary>
    public Guid? SpaId { get; set; }

    /// <summary>
    /// When the SPA was submitted
    /// </summary>
    public DateTime? SpaSubmittedAt { get; set; }
}
