using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;

namespace Rascor.Core.Infrastructure.Services;

/// <summary>
/// Email service for sending notifications.
/// Stub implementation - replace with actual email provider integration (SendGrid, AWS SES, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordSetupEmailAsync(
        string email,
        string firstName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://app.rascor.ie";
        var encodedToken = WebUtility.UrlEncode(resetToken);
        var encodedEmail = WebUtility.UrlEncode(email);
        var resetUrl = $"{baseUrl}/auth/set-password?email={encodedEmail}&token={encodedToken}";

        var subject = "Welcome to RASCOR - Set Up Your Account";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 10px; border-radius: 5px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to RASCOR</h1>
        </div>
        <div class='content'>
            <p>Dear {firstName},</p>
            <p>An account has been created for you in the RASCOR Business Suite. Please click the link below to set your password and activate your account:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Set Up My Password</a>
            </p>
            <div class='warning'>
                <strong>Important:</strong> This link will expire in 24 hours. If you did not expect this email, please contact your administrator.
            </div>
            <p style='margin-top: 20px;'>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; font-size: 12px; color: #666;'>{resetUrl}</p>
        </div>
        <div class='footer'>
            <p>Thank you,<br>RASCOR Team</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        // TODO: Integrate with actual email provider (SendGrid, AWS SES, SMTP, etc.)
        // For now, log the email details as a stub implementation

        _logger.LogInformation(
            "Email notification stub - To={To}, Subject={Subject}",
            to,
            subject);

        _logger.LogDebug(
            "Email body: {Body}",
            htmlBody);

        // Simulate async operation
        await Task.CompletedTask;
    }
}
