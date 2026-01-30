namespace Rascor.Modules.SiteAttendance.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for email notifications.
/// Supports multiple providers: SendGrid, SMTP, or custom implementations.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// The email provider to use: "None", "SendGrid", "Smtp", "MailerSend"
    /// Default is "None" (stub provider - logs but doesn't send)
    /// </summary>
    public string Provider { get; set; } = "None";

    /// <summary>
    /// Sender email address for all outgoing emails
    /// </summary>
    public string FromEmail { get; set; } = "noreply@rascor.ie";

    /// <summary>
    /// Sender display name for all outgoing emails
    /// </summary>
    public string FromName { get; set; } = "RASCOR Safety";

    /// <summary>
    /// Base URL for SPA submission links in emails
    /// </summary>
    public string SpaSubmissionBaseUrl { get; set; } = "https://rascorweb-production.up.railway.app/site-attendance/spa/submit";

    /// <summary>
    /// SendGrid-specific settings
    /// </summary>
    public SendGridSettings SendGrid { get; set; } = new();

    /// <summary>
    /// SMTP-specific settings
    /// </summary>
    public SmtpSettings Smtp { get; set; } = new();

    /// <summary>
    /// MailerSend-specific settings
    /// </summary>
    public MailerSendSettings MailerSend { get; set; } = new();
}

/// <summary>
/// SendGrid email provider settings
/// </summary>
public class SendGridSettings
{
    /// <summary>
    /// SendGrid API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// MailerSend email provider settings
/// </summary>
public class MailerSendSettings
{
    /// <summary>
    /// MailerSend API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// MailerSend API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.mailersend.com/v1";
}

/// <summary>
/// SMTP email provider settings
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// SMTP server hostname
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (587 for TLS, 465 for SSL, 25 for unencrypted)
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// SMTP authentication username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP authentication password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use SSL/TLS
    /// </summary>
    public bool UseSsl { get; set; } = true;
}
