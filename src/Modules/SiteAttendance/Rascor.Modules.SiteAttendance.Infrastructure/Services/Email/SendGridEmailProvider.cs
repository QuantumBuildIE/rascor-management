using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Modules.SiteAttendance.Application.Abstractions.Email;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services.Email;

/// <summary>
/// SendGrid email provider implementation.
/// Uses SendGrid's Web API v3 to send emails.
/// </summary>
/// <remarks>
/// To enable:
/// 1. Add SendGrid API key to configuration (Email:SendGrid:ApiKey)
/// 2. Set Email:Provider to "SendGrid"
/// 3. Register this provider in ServiceCollectionExtensions
/// </remarks>
public class SendGridEmailProvider : IEmailProvider
{
    private readonly ILogger<SendGridEmailProvider> _logger;
    private readonly EmailSettings _settings;
    private readonly SendGridClient? _client;

    public SendGridEmailProvider(
        ILogger<SendGridEmailProvider> logger,
        IOptions<EmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;

        // Initialize SendGrid client if API key is configured
        if (!string.IsNullOrWhiteSpace(_settings.SendGrid.ApiKey))
        {
            _client = new SendGridClient(_settings.SendGrid.ApiKey);
        }
    }

    /// <summary>
    /// Returns true if SendGrid API key is configured
    /// </summary>
    public bool IsConfigured => _client != null;

    /// <summary>
    /// Sends an email via SendGrid
    /// </summary>
    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning(
                "SendGrid provider not configured. Email NOT sent - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);
            return EmailSendResult.NotConfigured();
        }

        try
        {
            var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
            var to = new EmailAddress(message.ToEmail, message.ToName);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                message.Subject,
                message.PlainTextBody,
                message.HtmlBody);

            // Add reply-to if specified
            if (!string.IsNullOrWhiteSpace(message.ReplyToEmail))
            {
                msg.ReplyTo = new EmailAddress(message.ReplyToEmail, message.ReplyToName);
            }

            var response = await _client!.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // Extract message ID from headers if available
                string? messageId = null;
                if (response.Headers.TryGetValues("X-Message-Id", out var messageIds))
                {
                    messageId = messageIds.FirstOrDefault();
                }

                _logger.LogInformation(
                    "SendGrid email sent successfully - To: {ToEmail}, Subject: {Subject}, MessageId: {MessageId}",
                    message.ToEmail, message.Subject, messageId);

                return EmailSendResult.Succeeded(messageId);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                var errorMessage = $"SendGrid API returned {(int)response.StatusCode}: {body}";

                _logger.LogWarning(
                    "SendGrid email failed - To: {ToEmail}, Subject: {Subject}, StatusCode: {StatusCode}, Body: {Body}",
                    message.ToEmail, message.Subject, (int)response.StatusCode, body);

                return EmailSendResult.Failed(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SendGrid email exception - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);

            return EmailSendResult.Failed(ex.Message);
        }
    }
}
