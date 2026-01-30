using Microsoft.Extensions.Configuration;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service for generating professional HTML email templates for RAMS notifications
/// </summary>
public class RamsEmailTemplateService : IRamsEmailTemplateService
{
    private readonly string _baseUrl;
    private readonly string _companyName;

    public RamsEmailTemplateService(IConfiguration configuration)
    {
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://rascorweb-production.up.railway.app";
        _companyName = configuration["AppSettings:CompanyName"] ?? "RASCOR";
    }

    public RamsEmailTemplateDto GetSubmitTemplate(
        string projectReference,
        string projectName,
        string submittedBy,
        string documentUrl,
        int riskCount,
        int highRiskCount)
    {
        var subject = $"RAMS Submitted for Review: {projectReference} - {projectName}";

        var warningSection = highRiskCount > 0
            ? $@"<div style='background-color: #fff3cd; border: 1px solid #ffc107; padding: 10px; border-radius: 5px; margin-top: 15px;'>
                <strong>Attention:</strong> This document contains {highRiskCount} high-risk item(s) that require careful review.
            </div>"
            : "";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0d6efd; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .detail-row:last-child {{ border-bottom: none; }}
        .btn {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>RAMS Submitted for Review</h1>
        </div>
        <div class='content'>
            <p>A Risk Assessment and Method Statement has been submitted for your review.</p>

            <div class='details'>
                <div class='detail-row'>
                    <strong>Reference:</strong>
                    <span>{EscapeHtml(projectReference)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Project:</strong>
                    <span>{EscapeHtml(projectName)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Submitted By:</strong>
                    <span>{EscapeHtml(submittedBy)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Risk Assessments:</strong>
                    <span>{riskCount}</span>
                </div>
            </div>

            {warningSection}

            <p style='text-align: center;'>
                <a href='{documentUrl}' class='btn'>Review Document</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from the {_companyName} RAMS System.</p>
            <p>Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        var plainText = $@"
RAMS Submitted for Review

A Risk Assessment and Method Statement has been submitted for your review.

Reference: {projectReference}
Project: {projectName}
Submitted By: {submittedBy}
Risk Assessments: {riskCount}
{(highRiskCount > 0 ? $"\nATTENTION: This document contains {highRiskCount} high-risk item(s).\n" : "")}

Review the document at: {documentUrl}

---
This is an automated message from the {_companyName} RAMS System.
";

        return new RamsEmailTemplateDto
        {
            TemplateName = "RamsSubmit",
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainText
        };
    }

    public RamsEmailTemplateDto GetApprovalTemplate(
        string projectReference,
        string projectName,
        string approvedBy,
        string documentUrl)
    {
        var subject = $"RAMS Approved: {projectReference} - {projectName}";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #198754; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .detail-row:last-child {{ border-bottom: none; }}
        .btn {{ display: inline-block; padding: 12px 24px; background-color: #198754; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .success {{ background-color: #d1e7dd; border: 1px solid #198754; padding: 15px; border-radius: 5px; text-align: center; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>RAMS Approved</h1>
        </div>
        <div class='content'>
            <div class='success'>
                <h2 style='color: #198754; margin: 0;'>Your RAMS has been approved!</h2>
            </div>

            <div class='details'>
                <div class='detail-row'>
                    <strong>Reference:</strong>
                    <span>{EscapeHtml(projectReference)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Project:</strong>
                    <span>{EscapeHtml(projectName)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Approved By:</strong>
                    <span>{EscapeHtml(approvedBy)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Approved At:</strong>
                    <span>{DateTime.UtcNow:dd MMM yyyy HH:mm} UTC</span>
                </div>
            </div>

            <p>The document is now ready for use. You can download the PDF from the document page.</p>

            <p style='text-align: center;'>
                <a href='{documentUrl}' class='btn'>View Document</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from the {_companyName} RAMS System.</p>
        </div>
    </div>
</body>
</html>";

        var plainText = $@"
RAMS Approved

Your Risk Assessment and Method Statement has been approved!

Reference: {projectReference}
Project: {projectName}
Approved By: {approvedBy}
Approved At: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC

The document is now ready for use. You can download the PDF from the document page.

View the document at: {documentUrl}

---
This is an automated message from the {_companyName} RAMS System.
";

        return new RamsEmailTemplateDto
        {
            TemplateName = "RamsApproval",
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainText
        };
    }

    public RamsEmailTemplateDto GetRejectionTemplate(
        string projectReference,
        string projectName,
        string rejectedBy,
        string? rejectionComments,
        string documentUrl)
    {
        var subject = $"RAMS Rejected: {projectReference} - {projectName}";

        var commentsHtml = string.IsNullOrEmpty(rejectionComments)
            ? "<p><em>No comments provided.</em></p>"
            : $"<div style='background-color: white; padding: 15px; border-left: 4px solid #dc3545; margin: 15px 0;'>{EscapeHtml(rejectionComments)}</div>";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .detail-row:last-child {{ border-bottom: none; }}
        .btn {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>RAMS Rejected</h1>
        </div>
        <div class='content'>
            <p>Your Risk Assessment and Method Statement has been rejected and requires amendments.</p>

            <div class='details'>
                <div class='detail-row'>
                    <strong>Reference:</strong>
                    <span>{EscapeHtml(projectReference)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Project:</strong>
                    <span>{EscapeHtml(projectName)}</span>
                </div>
                <div class='detail-row'>
                    <strong>Rejected By:</strong>
                    <span>{EscapeHtml(rejectedBy)}</span>
                </div>
            </div>

            <h3>Rejection Comments:</h3>
            {commentsHtml}

            <p>Please review the comments, make the necessary amendments, and resubmit the document for approval.</p>

            <p style='text-align: center;'>
                <a href='{documentUrl}/edit' class='btn'>Edit Document</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from the {_companyName} RAMS System.</p>
        </div>
    </div>
</body>
</html>";

        var plainText = $@"
RAMS Rejected

Your Risk Assessment and Method Statement has been rejected and requires amendments.

Reference: {projectReference}
Project: {projectName}
Rejected By: {rejectedBy}

Rejection Comments:
{(string.IsNullOrEmpty(rejectionComments) ? "No comments provided." : rejectionComments)}

Please review the comments, make the necessary amendments, and resubmit the document for approval.

Edit the document at: {documentUrl}/edit

---
This is an automated message from the {_companyName} RAMS System.
";

        return new RamsEmailTemplateDto
        {
            TemplateName = "RamsRejection",
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainText
        };
    }

    public RamsEmailTemplateDto GetDailyDigestTemplate(
        int pendingCount,
        int overdueCount,
        List<RamsPendingApprovalDto> pendingItems,
        List<RamsOverdueDocumentDto> overdueItems,
        string dashboardUrl)
    {
        var subject = $"RAMS Daily Digest: {pendingCount} Pending, {overdueCount} Overdue";

        var pendingTableRows = pendingItems.Any()
            ? string.Join("", pendingItems.Select(p =>
            {
                var badgeColor = p.DaysPending > 7 ? "#dc3545" : p.DaysPending > 3 ? "#ffc107" : "#6c757d";
                var textColor = p.DaysPending > 3 && p.DaysPending <= 7 ? "#000" : "#fff";
                return $@"
                <tr>
                    <td style='padding: 8px; border-bottom: 1px solid #eee;'><a href='{_baseUrl}/rams/{p.Id}'>{EscapeHtml(p.ProjectReference)}</a></td>
                    <td style='padding: 8px; border-bottom: 1px solid #eee;'>{EscapeHtml(p.ProjectName)}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: center;'>
                        <span style='background-color: {badgeColor}; color: {textColor}; padding: 2px 8px; border-radius: 10px; font-size: 12px;'>{p.DaysPending}d</span>
                    </td>
                </tr>";
            }))
            : "<tr><td colspan='3' style='padding: 20px; text-align: center; color: #666;'>No documents pending approval</td></tr>";

        var overdueSection = overdueItems.Any()
            ? $@"
            <div style='margin-top: 25px;'>
                <h2 style='font-size: 18px; margin-bottom: 10px; padding-bottom: 5px; border-bottom: 2px solid #dc3545;'>Overdue Documents</h2>
                <table style='width: 100%; border-collapse: collapse; background-color: white; border-radius: 5px; overflow: hidden;'>
                    <thead>
                        <tr>
                            <th style='background-color: #f1f3f4; padding: 12px 8px; text-align: left; font-weight: 600;'>Reference</th>
                            <th style='background-color: #f1f3f4; padding: 12px 8px; text-align: left; font-weight: 600;'>Project</th>
                            <th style='background-color: #f1f3f4; padding: 12px 8px; text-align: center; font-weight: 600;'>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", overdueItems.Select(o => $@"
                        <tr>
                            <td style='padding: 8px; border-bottom: 1px solid #eee;'><a href='{_baseUrl}/rams/{o.Id}'>{EscapeHtml(o.ProjectReference)}</a></td>
                            <td style='padding: 8px; border-bottom: 1px solid #eee;'>{EscapeHtml(o.ProjectName)}</td>
                            <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: center;'>
                                <span style='background-color: #dc3545; color: #fff; padding: 2px 8px; border-radius: 10px; font-size: 12px;'>{o.DaysOverdue}d overdue</span>
                            </td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>"
            : "";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0d6efd; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .stats {{ display: flex; justify-content: space-around; margin: 20px 0; }}
        .stat-box {{ background-color: white; padding: 15px 25px; border-radius: 10px; text-align: center; }}
        .stat-number {{ font-size: 32px; font-weight: bold; }}
        .stat-label {{ font-size: 12px; color: #666; }}
        .btn {{ display: inline-block; padding: 10px 20px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>RAMS Daily Digest</h1>
            <p style='margin: 0; opacity: 0.9;'>{DateTime.UtcNow:dddd, dd MMMM yyyy}</p>
        </div>
        <div class='content'>
            <table style='width: 100%; margin: 20px 0;'>
                <tr>
                    <td style='text-align: center; padding: 15px; background-color: white; border-radius: 10px; margin: 5px;'>
                        <div style='font-size: 32px; font-weight: bold; color: #ffc107;'>{pendingCount}</div>
                        <div style='font-size: 12px; color: #666;'>PENDING APPROVAL</div>
                    </td>
                    <td style='width: 20px;'></td>
                    <td style='text-align: center; padding: 15px; background-color: white; border-radius: 10px; margin: 5px;'>
                        <div style='font-size: 32px; font-weight: bold; color: #dc3545;'>{overdueCount}</div>
                        <div style='font-size: 12px; color: #666;'>OVERDUE</div>
                    </td>
                </tr>
            </table>

            <div style='margin-top: 25px;'>
                <h2 style='font-size: 18px; margin-bottom: 10px; padding-bottom: 5px; border-bottom: 2px solid #0d6efd;'>Pending Approvals</h2>
                <table style='width: 100%; border-collapse: collapse; background-color: white; border-radius: 5px; overflow: hidden;'>
                    <thead>
                        <tr>
                            <th style='background-color: #f1f3f4; padding: 12px 8px; text-align: left; font-weight: 600;'>Reference</th>
                            <th style='background-color: #f1f3f4; padding: 12px 8px; text-align: left; font-weight: 600;'>Project</th>
                            <th style='background-color: #f1f3f4; padding: 12px 8px; text-align: center; font-weight: 600;'>Waiting</th>
                        </tr>
                    </thead>
                    <tbody>
                        {pendingTableRows}
                    </tbody>
                </table>
            </div>

            {overdueSection}

            <p style='text-align: center; margin-top: 25px;'>
                <a href='{dashboardUrl}' class='btn'>View Dashboard</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is your daily RAMS digest from the {_companyName} RAMS System.</p>
            <p>To unsubscribe from these emails, update your notification preferences in the system.</p>
        </div>
    </div>
</body>
</html>";

        var pendingText = pendingItems.Any()
            ? string.Join("\n", pendingItems.Select(p => $"  - {p.ProjectReference}: {p.ProjectName} ({p.DaysPending} days pending)"))
            : "  No documents pending approval";

        var overdueText = overdueItems.Any()
            ? "\n\nOVERDUE DOCUMENTS:\n" + string.Join("\n", overdueItems.Select(o => $"  - {o.ProjectReference}: {o.ProjectName} ({o.DaysOverdue} days overdue)"))
            : "";

        var plainText = $@"
RAMS Daily Digest - {DateTime.UtcNow:dddd, dd MMMM yyyy}

SUMMARY:
- Pending Approval: {pendingCount}
- Overdue: {overdueCount}

PENDING APPROVALS:
{pendingText}
{overdueText}

View Dashboard: {dashboardUrl}

---
This is your daily RAMS digest from the {_companyName} RAMS System.
";

        return new RamsEmailTemplateDto
        {
            TemplateName = "RamsDailyDigest",
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainText
        };
    }

    public RamsEmailTemplateDto GetTestTemplate()
    {
        var subject = "RAMS Notification Test - Configuration Verified";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .success {{ background-color: #d1e7dd; border: 1px solid #198754; padding: 15px; border-radius: 5px; text-align: center; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>RAMS Notification Test</h1>
        </div>
        <div class='content'>
            <div class='success'>
                <h2 style='color: #198754; margin: 0;'>Email configuration is working correctly!</h2>
            </div>

            <p>This is a test notification from the RAMS System. If you received this email, your notification settings are configured correctly.</p>

            <p><strong>Test Details:</strong></p>
            <ul>
                <li>Sent at: {DateTime.UtcNow:dd MMM yyyy HH:mm:ss} UTC</li>
                <li>System: {_companyName} RAMS</li>
                <li>Base URL: {_baseUrl}</li>
            </ul>
        </div>
        <div class='footer'>
            <p>This is a test message from the {_companyName} RAMS System.</p>
        </div>
    </div>
</body>
</html>";

        var plainText = $@"
RAMS Notification Test

Email configuration is working correctly!

This is a test notification from the RAMS System. If you received this email, your notification settings are configured correctly.

Test Details:
- Sent at: {DateTime.UtcNow:dd MMM yyyy HH:mm:ss} UTC
- System: {_companyName} RAMS
- Base URL: {_baseUrl}

---
This is a test message from the {_companyName} RAMS System.
";

        return new RamsEmailTemplateDto
        {
            TemplateName = "RamsTest",
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainText
        };
    }

    private static string EscapeHtml(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return System.Net.WebUtility.HtmlEncode(text);
    }
}
