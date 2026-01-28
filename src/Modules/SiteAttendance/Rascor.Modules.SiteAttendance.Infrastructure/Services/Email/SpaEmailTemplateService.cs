using Microsoft.Extensions.Options;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services.Email;

/// <summary>
/// Generates email templates for Site Photo Attendance (SPA) reminders.
/// Templates are mobile-friendly and include RASCOR branding.
/// </summary>
public class SpaEmailTemplateService
{
    private readonly EmailSettings _settings;

    public SpaEmailTemplateService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Generates the HTML email body for an SPA reminder
    /// </summary>
    /// <param name="employeeName">Employee's full name</param>
    /// <param name="siteName">Name of the site they checked into</param>
    /// <param name="entryTime">Time of site entry</param>
    /// <param name="siteId">Site ID for the submission link</param>
    /// <returns>HTML email body</returns>
    public string GenerateSpaReminderHtml(
        string employeeName,
        string siteName,
        DateTime entryTime,
        Guid siteId)
    {
        var formattedTime = entryTime.ToString("h:mm tt");
        var formattedDate = entryTime.ToString("dddd, d MMMM yyyy");
        var submissionUrl = $"{_settings.SpaSubmissionBaseUrl}?siteId={siteId}";

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>SPA Reminder - RASCOR</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f5;"">
    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #f4f4f5;"">
        <tr>
            <td style=""padding: 20px 0;"">
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""max-width: 600px; margin: 0 auto;"">

                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #1e3a5f; padding: 24px 32px; border-radius: 8px 8px 0 0;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td>
                                        <h1 style=""margin: 0; color: #ffffff; font-size: 24px; font-weight: 700;"">RASCOR</h1>
                                        <p style=""margin: 4px 0 0 0; color: #94a3b8; font-size: 14px;"">Site Safety Management</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style=""background-color: #ffffff; padding: 32px;"">

                            <!-- Greeting -->
                            <p style=""margin: 0 0 24px 0; color: #1f2937; font-size: 16px; line-height: 1.5;"">
                                Hi {HtmlEncode(employeeName)},
                            </p>

                            <!-- Alert Box -->
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""margin-bottom: 24px;"">
                                <tr>
                                    <td style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px; border-radius: 4px;"">
                                        <p style=""margin: 0; color: #92400e; font-size: 15px; line-height: 1.5;"">
                                            <strong>Site Photo Attendance Required</strong>
                                        </p>
                                        <p style=""margin: 8px 0 0 0; color: #92400e; font-size: 14px; line-height: 1.5;"">
                                            You checked into <strong>{HtmlEncode(siteName)}</strong> at <strong>{formattedTime}</strong> on {formattedDate}, but haven't submitted your Site Photo Attendance (SPA).
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Message -->
                            <p style=""margin: 0 0 24px 0; color: #4b5563; font-size: 15px; line-height: 1.6;"">
                                Please submit your SPA to remain compliant with site safety requirements. This is a mandatory requirement for all site personnel.
                            </p>

                            <!-- CTA Button -->
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""margin: 0 auto 24px auto;"">
                                <tr>
                                    <td style=""background-color: #2563eb; border-radius: 6px;"">
                                        <a href=""{submissionUrl}"" target=""_blank"" style=""display: inline-block; padding: 14px 32px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600;"">
                                            Submit SPA Now
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Alternative Link -->
                            <p style=""margin: 0; color: #6b7280; font-size: 13px; line-height: 1.5; text-align: center;"">
                                Or copy this link: <a href=""{submissionUrl}"" style=""color: #2563eb;"">{submissionUrl}</a>
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8fafc; padding: 24px 32px; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0 0 8px 0; color: #6b7280; font-size: 13px; line-height: 1.5; text-align: center;"">
                                This is an automated message from the RASCOR Safety Management System.
                            </p>
                            <p style=""margin: 0; color: #9ca3af; font-size: 12px; line-height: 1.5; text-align: center;"">
                                RASCOR Construction Ltd. | Dublin, Ireland
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    /// <summary>
    /// Generates the plain text version of the SPA reminder email
    /// </summary>
    public string GenerateSpaReminderPlainText(
        string employeeName,
        string siteName,
        DateTime entryTime,
        Guid siteId)
    {
        var formattedTime = entryTime.ToString("h:mm tt");
        var formattedDate = entryTime.ToString("dddd, d MMMM yyyy");
        var submissionUrl = $"{_settings.SpaSubmissionBaseUrl}?siteId={siteId}";

        return $@"RASCOR - Site Safety Management

Hi {employeeName},

SITE PHOTO ATTENDANCE REQUIRED

You checked into {siteName} at {formattedTime} on {formattedDate}, but haven't submitted your Site Photo Attendance (SPA).

Please submit your SPA to remain compliant with site safety requirements. This is a mandatory requirement for all site personnel.

Submit SPA Now: {submissionUrl}

---
This is an automated message from the RASCOR Safety Management System.
RASCOR Construction Ltd. | Dublin, Ireland";
    }

    /// <summary>
    /// HTML encodes a string to prevent XSS in email content
    /// </summary>
    private static string HtmlEncode(string value)
    {
        return System.Net.WebUtility.HtmlEncode(value);
    }
}
