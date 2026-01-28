using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Modules.SiteAttendance.Application.Abstractions.Email;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services.Email;

/// <summary>
/// SMTP email provider implementation.
/// Uses System.Net.Mail.SmtpClient to send emails via any SMTP server.
/// </summary>
/// <remarks>
/// To enable:
/// 1. Configure SMTP settings (Email:Smtp:Host, Port, Username, Password)
/// 2. Set Email:Provider to "Smtp"
/// 3. Register this provider in ServiceCollectionExtensions
/// </remarks>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly ILogger<SmtpEmailProvider> _logger;
    private readonly EmailSettings _settings;

    public SmtpEmailProvider(
        ILogger<SmtpEmailProvider> logger,
        IOptions<EmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    /// <summary>
    /// Returns true if SMTP host is configured
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Smtp.Host);

    /// <summary>
    /// Sends an email via SMTP
    /// </summary>
    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning(
                "SMTP provider not configured. Email NOT sent - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);
            return EmailSendResult.NotConfigured();
        }

        try
        {
            using var smtpClient = new SmtpClient(_settings.Smtp.Host, _settings.Smtp.Port)
            {
                EnableSsl = _settings.Smtp.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            // Add credentials if configured
            if (!string.IsNullOrWhiteSpace(_settings.Smtp.Username))
            {
                smtpClient.Credentials = new NetworkCredential(
                    _settings.Smtp.Username,
                    _settings.Smtp.Password);
            }

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);
            var to = new MailAddress(message.ToEmail, message.ToName);

            using var mailMessage = new MailMessage(from, to)
            {
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            // Add plain text alternative if provided
            if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
            {
                var plainTextView = AlternateView.CreateAlternateViewFromString(
                    message.PlainTextBody,
                    null,
                    "text/plain");
                mailMessage.AlternateViews.Add(plainTextView);

                var htmlView = AlternateView.CreateAlternateViewFromString(
                    message.HtmlBody,
                    null,
                    "text/html");
                mailMessage.AlternateViews.Add(htmlView);

                // Clear the main body since we're using alternate views
                mailMessage.Body = string.Empty;
            }

            // Add reply-to if specified
            if (!string.IsNullOrWhiteSpace(message.ReplyToEmail))
            {
                mailMessage.ReplyToList.Add(new MailAddress(
                    message.ReplyToEmail,
                    message.ReplyToName));
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation(
                "SMTP email sent successfully - To: {ToEmail}, Subject: {Subject}, Host: {Host}",
                message.ToEmail, message.Subject, _settings.Smtp.Host);

            return EmailSendResult.Succeeded();
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex,
                "SMTP email failed - To: {ToEmail}, Subject: {Subject}, StatusCode: {StatusCode}",
                message.ToEmail, message.Subject, ex.StatusCode);

            return EmailSendResult.Failed($"SMTP error ({ex.StatusCode}): {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SMTP email exception - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);

            return EmailSendResult.Failed(ex.Message);
        }
    }
}
