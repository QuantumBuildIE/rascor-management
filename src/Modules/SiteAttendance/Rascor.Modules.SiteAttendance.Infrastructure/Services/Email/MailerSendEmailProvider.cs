using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Core.Application.Abstractions.Email;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services.Email;

/// <summary>
/// MailerSend email provider implementation.
/// Uses MailerSend's REST API to send emails.
/// </summary>
/// <remarks>
/// To enable:
/// 1. Add MailerSend API key to configuration (Email:MailerSend:ApiKey)
/// 2. Set Email:Provider to "MailerSend"
/// </remarks>
public class MailerSendEmailProvider : IEmailProvider
{
    private readonly ILogger<MailerSendEmailProvider> _logger;
    private readonly EmailSettings _settings;
    private readonly HttpClient _httpClient;

    public MailerSendEmailProvider(
        ILogger<MailerSendEmailProvider> logger,
        IOptions<EmailSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient("MailerSend");
    }

    /// <summary>
    /// Returns true if MailerSend API key is configured
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.MailerSend.ApiKey);

    /// <summary>
    /// Sends an email via MailerSend REST API
    /// </summary>
    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning(
                "MailerSend provider not configured. Email NOT sent - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);
            return EmailSendResult.NotConfigured();
        }

        try
        {
            var request = new MailerSendRequest
            {
                From = new MailerSendAddress
                {
                    Email = _settings.FromEmail,
                    Name = _settings.FromName
                },
                To =
                [
                    new MailerSendAddress
                    {
                        Email = message.ToEmail,
                        Name = message.ToName ?? message.ToEmail
                    }
                ],
                Subject = message.Subject,
                Html = message.HtmlBody,
                Text = message.PlainTextBody
            };

            if (!string.IsNullOrWhiteSpace(message.ReplyToEmail))
            {
                request.ReplyTo = new MailerSendAddress
                {
                    Email = message.ReplyToEmail,
                    Name = message.ReplyToName
                };
            }

            var baseUrl = _settings.MailerSend.BaseUrl.TrimEnd('/');
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/email");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.MailerSend.ApiKey);
            httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

            _logger.LogDebug(
                "Sending email via MailerSend - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                string? messageId = null;
                if (response.Headers.TryGetValues("X-Message-Id", out var messageIds))
                {
                    messageId = messageIds.FirstOrDefault();
                }

                _logger.LogInformation(
                    "MailerSend email sent successfully - To: {ToEmail}, Subject: {Subject}, MessageId: {MessageId}",
                    message.ToEmail, message.Subject, messageId);

                return EmailSendResult.Succeeded(messageId);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var errorMessage = $"MailerSend API returned {(int)response.StatusCode}: {body}";

            _logger.LogWarning(
                "MailerSend email failed - To: {ToEmail}, Subject: {Subject}, StatusCode: {StatusCode}, Body: {Body}",
                message.ToEmail, message.Subject, (int)response.StatusCode, body);

            return EmailSendResult.Failed(errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "MailerSend email exception - To: {ToEmail}, Subject: {Subject}",
                message.ToEmail, message.Subject);

            return EmailSendResult.Failed(ex.Message);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private class MailerSendRequest
    {
        public required MailerSendAddress From { get; set; }
        public required List<MailerSendAddress> To { get; set; }
        public required string Subject { get; set; }
        public string? Html { get; set; }
        public string? Text { get; set; }
        public MailerSendAddress? ReplyTo { get; set; }
    }

    private class MailerSendAddress
    {
        public required string Email { get; set; }
        public string? Name { get; set; }
    }
}
