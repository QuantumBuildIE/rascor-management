using System.Text;
using Rascor.Core.Domain.Entities;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Default implementation of SPA reminder email templates.
/// </summary>
public class FloatSpaEmailTemplateService : IFloatSpaEmailTemplateService
{
    /// <inheritdoc />
    public string GenerateReminderEmailSubject(Site site, DateOnly scheduledDate)
    {
        return $"Reminder: Site Photo Attendance Required - {site.SiteName}";
    }

    /// <inheritdoc />
    public string GenerateReminderEmailHtml(
        Employee employee,
        Site site,
        DateOnly scheduledDate,
        string spaSubmissionUrl)
    {
        var addressLine = !string.IsNullOrWhiteSpace(site.Address)
            ? $"<strong>Address:</strong> {EscapeHtml(site.Address)}<br>"
            : "";

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>SPA Reminder</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; border-radius: 8px; padding: 30px; margin-bottom: 20px;'>
        <h1 style='color: #0056b3; margin-top: 0;'>Site Photo Attendance Reminder</h1>

        <p>Hi {EscapeHtml(employee.FirstName)},</p>

        <p>According to our schedule, you are assigned to work at <strong>{EscapeHtml(site.SiteName)}</strong> today ({scheduledDate:dddd, d MMMM yyyy}).</p>

        <p>We haven't received your Site Photo Attendance (SPA) submission yet. Please submit your SPA to confirm your presence on site.</p>

        <div style='background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 4px; padding: 15px; margin: 20px 0;'>
            <strong>Important:</strong> SPA submissions are required for health and safety compliance and accurate site attendance records.
        </div>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='{EscapeHtml(spaSubmissionUrl)}' style='background-color: #0056b3; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Submit SPA Now</a>
        </div>

        <p style='font-size: 14px; color: #666;'>
            <strong>Site:</strong> {EscapeHtml(site.SiteName)}<br>
            <strong>Date:</strong> {scheduledDate:dddd, d MMMM yyyy}<br>
            {addressLine}
        </p>

        <p>If you are not on site today or believe this is an error, please contact your supervisor.</p>

        <p>Thank you,<br>
        <strong>RASCOR Safety Management</strong></p>
    </div>

    <div style='text-align: center; font-size: 12px; color: #999;'>
        <p>This is an automated message from RASCOR Safety Management System.</p>
    </div>
</body>
</html>";
    }

    /// <inheritdoc />
    public string GenerateReminderEmailPlainText(
        Employee employee,
        Site site,
        DateOnly scheduledDate,
        string spaSubmissionUrl)
    {
        var sb = new StringBuilder();

        sb.AppendLine("SITE PHOTO ATTENDANCE REMINDER");
        sb.AppendLine("==============================");
        sb.AppendLine();
        sb.AppendLine($"Hi {employee.FirstName},");
        sb.AppendLine();
        sb.AppendLine($"According to our schedule, you are assigned to work at {site.SiteName} today ({scheduledDate:dddd, d MMMM yyyy}).");
        sb.AppendLine();
        sb.AppendLine("We haven't received your Site Photo Attendance (SPA) submission yet. Please submit your SPA to confirm your presence on site.");
        sb.AppendLine();
        sb.AppendLine("IMPORTANT: SPA submissions are required for health and safety compliance and accurate site attendance records.");
        sb.AppendLine();
        sb.AppendLine("Submit your SPA here:");
        sb.AppendLine(spaSubmissionUrl);
        sb.AppendLine();
        sb.AppendLine("Site Details:");
        sb.AppendLine($"  Site: {site.SiteName}");
        sb.AppendLine($"  Date: {scheduledDate:dddd, d MMMM yyyy}");

        if (!string.IsNullOrWhiteSpace(site.Address))
        {
            sb.AppendLine($"  Address: {site.Address}");
        }

        sb.AppendLine();
        sb.AppendLine("If you are not on site today or believe this is an error, please contact your supervisor.");
        sb.AppendLine();
        sb.AppendLine("Thank you,");
        sb.AppendLine("RASCOR Safety Management");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine("This is an automated message from RASCOR Safety Management System.");

        return sb.ToString();
    }

    /// <summary>
    /// Escapes HTML special characters to prevent XSS.
    /// </summary>
    private static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
