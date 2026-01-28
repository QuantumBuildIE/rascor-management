using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Abstractions.Email;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services.Email;

/// <summary>
/// Stub email provider that logs emails but doesn't actually send them.
/// Used when no email provider is configured or for development/testing.
/// </summary>
public class StubEmailProvider : IEmailProvider
{
    private readonly ILogger<StubEmailProvider> _logger;

    public StubEmailProvider(ILogger<StubEmailProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Always returns false - this provider is not configured for actual sending
    /// </summary>
    public bool IsConfigured => false;

    /// <summary>
    /// Logs the email details but does not send. Returns NotConfigured result.
    /// </summary>
    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Email provider not configured. Email NOT sent - To: {ToEmail} ({ToName}), Subject: {Subject}",
            message.ToEmail,
            message.ToName ?? "No Name",
            message.Subject);

        _logger.LogDebug(
            "Unsent email content: {HtmlBody}",
            message.HtmlBody);

        return Task.FromResult(EmailSendResult.NotConfigured());
    }
}
