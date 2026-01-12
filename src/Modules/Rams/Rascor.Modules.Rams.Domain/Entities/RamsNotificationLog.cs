using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Rams.Domain.Entities;

/// <summary>
/// Entity for tracking RAMS notification history and delivery status
/// </summary>
public class RamsNotificationLog : TenantEntity
{
    /// <summary>
    /// Optional reference to the RAMS document this notification relates to
    /// Null for digest emails
    /// </summary>
    public Guid? RamsDocumentId { get; set; }

    /// <summary>
    /// Type of notification: Submit, Approve, Reject, Digest
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the recipient
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the recipient
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Preview of the email body (first 1000 characters)
    /// </summary>
    public string? BodyPreview { get; set; }

    /// <summary>
    /// When the notification was attempted
    /// </summary>
    public DateTime AttemptedAt { get; set; }

    /// <summary>
    /// Whether the email was successfully sent
    /// </summary>
    public bool WasSent { get; set; }

    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// User ID who triggered this notification
    /// </summary>
    public string? TriggeredByUserId { get; set; }

    /// <summary>
    /// Display name of the user who triggered this notification
    /// </summary>
    public string? TriggeredByUserName { get; set; }

    // Navigation property
    public virtual RamsDocument? RamsDocument { get; set; }
}
