using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Domain.Entities;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Services;

/// <summary>
/// Email service for Toolbox Talk notifications.
/// Stub implementation - replace with actual email provider integration (SendGrid, AWS SES, etc.)
/// </summary>
public class ToolboxTalkEmailService : IToolboxTalkEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ToolboxTalkEmailService> _logger;

    public ToolboxTalkEmailService(
        IConfiguration configuration,
        ILogger<ToolboxTalkEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTalkAssignmentEmailAsync(
        ScheduledTalk scheduledTalk,
        Employee employee,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(employee.Email))
        {
            _logger.LogWarning(
                "Cannot send assignment email: Employee {EmployeeId} has no email address",
                employee.Id);
            return;
        }

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://app.rascor.ie";
        var talkUrl = $"{baseUrl}/my/toolbox-talks/{scheduledTalk.Id}";

        var subject = $"New Toolbox Talk Assigned: {scheduledTalk.ToolboxTalk.Title}";
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
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Toolbox Talk Assigned</h1>
        </div>
        <div class='content'>
            <p>Dear {employee.FirstName},</p>
            <p>A new toolbox talk has been assigned to you:</p>
            <p><strong>{scheduledTalk.ToolboxTalk.Title}</strong></p>
            {(string.IsNullOrEmpty(scheduledTalk.ToolboxTalk.Description) ? "" : $"<p>{scheduledTalk.ToolboxTalk.Description}</p>")}
            <p><strong>Due Date:</strong> {scheduledTalk.DueDate:dd MMM yyyy}</p>
            <p>
                <a href='{talkUrl}' class='button'>Start Talk</a>
            </p>
        </div>
        <div class='footer'>
            <p>Thank you,<br>RASCOR Safety Team</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        // TODO: Integrate with actual email provider (SendGrid, AWS SES, etc.)
        // await _emailSender.SendEmailAsync(employee.Email, subject, body, cancellationToken);

        _logger.LogInformation(
            "Email notification stub - Assignment: To={Email}, Subject={Subject}, TalkId={TalkId}",
            employee.Email,
            subject,
            scheduledTalk.Id);

        await Task.CompletedTask;
    }

    public async Task SendReminderEmailAsync(
        ScheduledTalk scheduledTalk,
        Employee employee,
        int reminderNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(employee.Email))
        {
            _logger.LogWarning(
                "Cannot send reminder email: Employee {EmployeeId} has no email address",
                employee.Id);
            return;
        }

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://app.rascor.ie";
        var talkUrl = $"{baseUrl}/my/toolbox-talks/{scheduledTalk.Id}";

        var urgency = reminderNumber >= 3 ? "URGENT: " : "";
        var subject = $"{urgency}Reminder {reminderNumber}: {scheduledTalk.ToolboxTalk.Title}";

        var daysOverdue = (DateTime.Today - scheduledTalk.DueDate.Date).Days;
        var overdueText = daysOverdue > 0
            ? $"<p style='color: #dc3545; font-weight: bold;'>This talk is {daysOverdue} day{(daysOverdue == 1 ? "" : "s")} overdue.</p>"
            : $"<p style='color: #ffc107; font-weight: bold;'>This talk is due today.</p>";

        var buttonColor = reminderNumber >= 3 ? "#dc3545" : "#ffc107";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {buttonColor}; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; background-color: {buttonColor}; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Reminder #{reminderNumber}</h1>
        </div>
        <div class='content'>
            <p>Dear {employee.FirstName},</p>
            <p>This is reminder #{reminderNumber} for an overdue toolbox talk:</p>
            <p><strong>{scheduledTalk.ToolboxTalk.Title}</strong></p>
            {overdueText}
            <p><strong>Original Due Date:</strong> {scheduledTalk.DueDate:dd MMM yyyy}</p>
            <p>
                <a href='{talkUrl}' class='button'>Complete Now</a>
            </p>
        </div>
        <div class='footer'>
            <p>Thank you,<br>RASCOR Safety Team</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        // TODO: Integrate with actual email provider
        _logger.LogInformation(
            "Email notification stub - Reminder: To={Email}, Subject={Subject}, TalkId={TalkId}, ReminderNumber={ReminderNumber}",
            employee.Email,
            subject,
            scheduledTalk.Id,
            reminderNumber);

        await Task.CompletedTask;
    }

    public async Task SendCompletionConfirmationEmailAsync(
        ScheduledTalkCompletion completion,
        Employee employee,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(employee.Email))
        {
            _logger.LogWarning(
                "Cannot send completion email: Employee {EmployeeId} has no email address",
                employee.Id);
            return;
        }

        var scheduledTalk = completion.ScheduledTalk;
        var subject = $"Toolbox Talk Completed: {scheduledTalk.ToolboxTalk.Title}";

        var quizSection = "";
        if (completion.QuizScore.HasValue && completion.QuizMaxScore.HasValue)
        {
            var passStatus = completion.QuizPassed == true ? "Passed" : "Failed";
            var passColor = completion.QuizPassed == true ? "#28a745" : "#dc3545";
            quizSection = $@"
            <p><strong>Quiz Results:</strong></p>
            <p>Score: {completion.QuizScore}/{completion.QuizMaxScore} (<span style='color: {passColor};'>{passStatus}</span>)</p>";
        }

        var certificateSection = "";
        if (!string.IsNullOrEmpty(completion.CertificateUrl))
        {
            certificateSection = $@"
            <p><a href='{completion.CertificateUrl}' style='color: #007bff;'>Download your completion certificate</a></p>";
        }

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
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
        .success-icon {{ font-size: 48px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>&#10004;</div>
            <h1>Toolbox Talk Completed</h1>
        </div>
        <div class='content'>
            <p>Dear {employee.FirstName},</p>
            <p>Thank you for completing the toolbox talk:</p>
            <p><strong>{scheduledTalk.ToolboxTalk.Title}</strong></p>
            <p><strong>Completed:</strong> {completion.CompletedAt:dd MMM yyyy HH:mm}</p>
            <p><strong>Time Spent:</strong> {FormatTimeSpent(completion.TotalTimeSpentSeconds)}</p>
            {quizSection}
            {certificateSection}
            <p>Your completion has been recorded.</p>
        </div>
        <div class='footer'>
            <p>Thank you,<br>RASCOR Safety Team</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        // TODO: Integrate with actual email provider
        _logger.LogInformation(
            "Email notification stub - Completion: To={Email}, Subject={Subject}, CompletionId={CompletionId}",
            employee.Email,
            subject,
            completion.Id);

        await Task.CompletedTask;
    }

    public async Task SendEscalationEmailAsync(
        ScheduledTalk scheduledTalk,
        Employee employee,
        Employee manager,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(manager.Email))
        {
            _logger.LogWarning(
                "Cannot send escalation email: Manager {ManagerId} has no email address",
                manager.Id);
            return;
        }

        var daysOverdue = (DateTime.Today - scheduledTalk.DueDate.Date).Days;
        var subject = $"Escalation: Overdue Toolbox Talk - {employee.FullName}";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
        .details-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        .details-table td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
        .details-table td:first-child {{ font-weight: bold; width: 40%; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Escalation Notice</h1>
        </div>
        <div class='content'>
            <p>Dear {manager.FirstName},</p>
            <p>The following toolbox talk remains incomplete after multiple reminders and requires your attention:</p>
            <table class='details-table'>
                <tr>
                    <td>Employee:</td>
                    <td>{employee.FullName}</td>
                </tr>
                <tr>
                    <td>Talk:</td>
                    <td>{scheduledTalk.ToolboxTalk.Title}</td>
                </tr>
                <tr>
                    <td>Original Due Date:</td>
                    <td>{scheduledTalk.DueDate:dd MMM yyyy}</td>
                </tr>
                <tr>
                    <td>Days Overdue:</td>
                    <td style='color: #dc3545; font-weight: bold;'>{daysOverdue}</td>
                </tr>
                <tr>
                    <td>Reminders Sent:</td>
                    <td>{scheduledTalk.RemindersSent}</td>
                </tr>
            </table>
            <p>Please follow up with {employee.FirstName} to ensure compliance with safety training requirements.</p>
        </div>
        <div class='footer'>
            <p>Thank you,<br>RASCOR Safety Team</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        // TODO: Integrate with actual email provider
        // Send to manager
        _logger.LogInformation(
            "Email notification stub - Escalation to Manager: To={Email}, Subject={Subject}, TalkId={TalkId}",
            manager.Email,
            subject,
            scheduledTalk.Id);

        // Also CC the employee if they have an email
        if (!string.IsNullOrEmpty(employee.Email))
        {
            _logger.LogInformation(
                "Email notification stub - Escalation CC to Employee: To={Email}, Subject={Subject}, TalkId={TalkId}",
                employee.Email,
                subject,
                scheduledTalk.Id);
        }

        await Task.CompletedTask;
    }

    private static string FormatTimeSpent(int totalSeconds)
    {
        if (totalSeconds < 60)
            return $"{totalSeconds} seconds";

        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        if (minutes < 60)
            return seconds > 0 ? $"{minutes} min {seconds} sec" : $"{minutes} min";

        var hours = minutes / 60;
        minutes %= 60;

        return minutes > 0 ? $"{hours} hr {minutes} min" : $"{hours} hr";
    }
}
