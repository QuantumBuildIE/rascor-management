namespace Rascor.Modules.Rams.Application.DTOs;

/// <summary>
/// DTO for RAMS notification history display
/// </summary>
public record RamsNotificationDto
{
    public Guid Id { get; init; }
    public Guid? DocumentId { get; init; }
    public string ProjectReference { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? BodyPreview { get; init; }
    public DateTime AttemptedAt { get; init; }
    public bool WasSent { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }
    public string? TriggeredByUserName { get; init; }
}

/// <summary>
/// Email template data for generating email content
/// </summary>
public record RamsEmailTemplateDto
{
    public string TemplateName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string PlainTextBody { get; init; } = string.Empty;
}

/// <summary>
/// Data for the daily digest email
/// </summary>
public record RamsDailyDigestDto
{
    public int PendingApprovalCount { get; init; }
    public int OverdueCount { get; init; }
    public int HighRiskCount { get; init; }
    public List<RamsPendingApprovalDto> PendingApprovals { get; init; } = [];
    public List<RamsOverdueDocumentDto> OverdueDocuments { get; init; } = [];
    public DateTime GeneratedAt { get; init; }
}

/// <summary>
/// User notification preferences for RAMS
/// </summary>
public record RamsNotificationPreferencesDto
{
    public bool ReceiveSubmitNotifications { get; init; } = true;
    public bool ReceiveApprovalNotifications { get; init; } = true;
    public bool ReceiveRejectionNotifications { get; init; } = true;
    public bool ReceiveDailyDigest { get; init; } = true;
}

/// <summary>
/// Request to send a test notification
/// </summary>
public record SendTestNotificationRequest
{
    public string Email { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
}

/// <summary>
/// Configuration settings for RAMS notifications
/// </summary>
public record RamsNotificationSettingsDto
{
    public bool Enabled { get; init; }
    public bool SendOnSubmit { get; init; }
    public bool SendOnApprove { get; init; }
    public bool SendOnReject { get; init; }
    public bool DailyDigestEnabled { get; init; }
    public string DailyDigestTime { get; init; } = "08:00";
    public List<string> DigestRecipients { get; init; } = [];
}
