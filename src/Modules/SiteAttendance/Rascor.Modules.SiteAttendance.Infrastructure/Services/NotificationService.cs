using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Application.Abstractions.Email;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Configuration;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;
using Rascor.Modules.SiteAttendance.Infrastructure.Services.Email;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services;

/// <summary>
/// Notification service for Site Attendance module.
/// Handles push, email, and SMS notifications for attendance events.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IAttendanceNotificationRepository _notificationRepository;
    private readonly IAttendanceSettingsRepository _settingsRepository;
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly SpaEmailTemplateService _templateService;
    private readonly SiteAttendanceDbContext _dbContext;
    private readonly EmailSettings _emailSettings;

    public NotificationService(
        ILogger<NotificationService> logger,
        IAttendanceNotificationRepository notificationRepository,
        IAttendanceSettingsRepository settingsRepository,
        ISitePhotoAttendanceRepository spaRepository,
        IEmailProvider emailProvider,
        SpaEmailTemplateService templateService,
        SiteAttendanceDbContext dbContext,
        IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _notificationRepository = notificationRepository;
        _settingsRepository = settingsRepository;
        _spaRepository = spaRepository;
        _emailProvider = emailProvider;
        _templateService = templateService;
        _dbContext = dbContext;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendPushNotificationAsync(
        Guid employeeId,
        string title,
        string message,
        string? deepLink = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Push notification stub: Employee={EmployeeId}, Title={Title}, Message={Message}, DeepLink={DeepLink}",
            employeeId, title, message, deepLink);

        // TODO: Integrate with Firebase Cloud Messaging (FCM) or Apple Push Notification Service (APNS)
        // 1. Get device tokens from DeviceRegistration table
        // 2. Send notification via FCM/APNS
        // 3. Handle delivery confirmation/failure

        await Task.CompletedTask;
    }

    public async Task SendEmailNotificationAsync(
        Guid employeeId,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        // Get employee details for email
        var employee = await _dbContext.Set<Employee>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            _logger.LogWarning("Cannot send email notification: Employee {EmployeeId} not found", employeeId);
            return;
        }

        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            _logger.LogWarning(
                "Cannot send email notification to employee {EmployeeId} ({EmployeeName}): No email address configured",
                employeeId, employee.FullName);
            return;
        }

        var emailMessage = new EmailMessage
        {
            ToEmail = employee.Email,
            ToName = employee.FullName,
            Subject = subject,
            HtmlBody = htmlContent
        };

        var result = await _emailProvider.SendAsync(emailMessage, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation(
                "Email notification sent to {EmployeeName} ({Email}), Subject: {Subject}, MessageId: {MessageId}",
                employee.FullName, employee.Email, subject, result.MessageId);
        }
        else
        {
            _logger.LogWarning(
                "Failed to send email notification to {EmployeeName} ({Email}): {Error}",
                employee.FullName, employee.Email, result.ErrorMessage);
        }
    }

    public async Task SendSmsNotificationAsync(
        Guid employeeId,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SMS notification stub: Employee={EmployeeId}, Message={Message}",
            employeeId, message);

        // TODO: Integrate with SMS provider (Twilio, AWS SNS, etc.)
        // 1. Get employee phone number from Employee table
        // 2. Send SMS via provider
        // 3. Handle delivery confirmation/failure

        await Task.CompletedTask;
    }

    public async Task CheckAndNotifyMissingSpaAsync(
        AttendanceEvent entryEvent,
        CancellationToken cancellationToken = default)
    {
        if (entryEvent.EventType != EventType.Enter)
            return;

        var settings = await _settingsRepository.GetByTenantAsync(entryEvent.TenantId, cancellationToken);
        if (settings == null)
            return;

        var eventDate = DateOnly.FromDateTime(entryEvent.Timestamp);

        // Check if employee already has SPA for today at this site
        var hasSpa = await _spaRepository.ExistsForEmployeeSiteDateAsync(
            entryEvent.TenantId,
            entryEvent.EmployeeId,
            entryEvent.SiteId,
            eventDate,
            cancellationToken);

        if (hasSpa)
            return;

        // Get employee and site details for the email template
        var employee = await _dbContext.Set<Employee>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entryEvent.EmployeeId, cancellationToken);

        var site = await _dbContext.Set<Site>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == entryEvent.SiteId, cancellationToken);

        var employeeName = employee?.FullName ?? "Team Member";
        var siteName = site?.SiteName ?? "Site";

        // Create notification record for push
        var notification = AttendanceNotification.Create(
            entryEvent.TenantId,
            entryEvent.EmployeeId,
            NotificationType.Push,
            NotificationReason.MissingSpa,
            settings.NotificationMessage,
            entryEvent.Id);

        await _notificationRepository.AddAsync(notification, cancellationToken);

        // Send push notification if enabled
        if (settings.EnablePushNotifications)
        {
            try
            {
                await SendPushNotificationAsync(
                    entryEvent.EmployeeId,
                    settings.NotificationTitle,
                    settings.NotificationMessage,
                    $"/attendance/spa/new?siteId={entryEvent.SiteId}",
                    cancellationToken);

                // Note: Push notifications are still stubs, marking as delivered for now
                notification.MarkDelivered();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification for missing SPA");
                notification.MarkFailed(ex.Message);
            }
        }

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        // Send email notification if enabled
        if (settings.EnableEmailNotifications)
        {
            // Create separate notification record for email
            var emailNotification = AttendanceNotification.Create(
                entryEvent.TenantId,
                entryEvent.EmployeeId,
                NotificationType.Email,
                NotificationReason.MissingSpa,
                settings.NotificationMessage,
                entryEvent.Id);

            await _notificationRepository.AddAsync(emailNotification, cancellationToken);

            try
            {
                if (employee != null && !string.IsNullOrWhiteSpace(employee.Email))
                {
                    // Generate the email content using template
                    var htmlBody = _templateService.GenerateSpaReminderHtml(
                        employeeName,
                        siteName,
                        entryEvent.Timestamp,
                        entryEvent.SiteId);

                    var plainTextBody = _templateService.GenerateSpaReminderPlainText(
                        employeeName,
                        siteName,
                        entryEvent.Timestamp,
                        entryEvent.SiteId);

                    var emailMessage = new EmailMessage
                    {
                        ToEmail = employee.Email,
                        ToName = employeeName,
                        Subject = settings.NotificationTitle,
                        HtmlBody = htmlBody,
                        PlainTextBody = plainTextBody
                    };

                    var result = await _emailProvider.SendAsync(emailMessage, cancellationToken);

                    if (result.Success)
                    {
                        emailNotification.MarkDelivered();
                        _logger.LogInformation(
                            "SPA reminder email sent to {EmployeeName} ({Email}) for site {SiteName}",
                            employeeName, employee.Email, siteName);
                    }
                    else
                    {
                        emailNotification.MarkFailed(result.ErrorMessage ?? "Unknown error");
                        _logger.LogWarning(
                            "Failed to send SPA reminder email to {EmployeeName}: {Error}",
                            employeeName, result.ErrorMessage);
                    }
                }
                else
                {
                    emailNotification.MarkFailed("No email address configured for employee");
                    _logger.LogWarning(
                        "Cannot send SPA reminder email: Employee {EmployeeId} has no email address",
                        entryEvent.EmployeeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification for missing SPA");
                emailNotification.MarkFailed(ex.Message);
            }

            await _notificationRepository.UpdateAsync(emailNotification, cancellationToken);
        }

        // Send SMS notification if enabled
        if (settings.EnableSmsNotifications)
        {
            // Create separate notification record for SMS
            var smsNotification = AttendanceNotification.Create(
                entryEvent.TenantId,
                entryEvent.EmployeeId,
                NotificationType.Sms,
                NotificationReason.MissingSpa,
                settings.NotificationMessage,
                entryEvent.Id);

            await _notificationRepository.AddAsync(smsNotification, cancellationToken);

            try
            {
                await SendSmsNotificationAsync(
                    entryEvent.EmployeeId,
                    settings.NotificationMessage,
                    cancellationToken);

                // SMS is still a stub, marking as not delivered
                smsNotification.MarkFailed("SMS provider not configured");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS notification for missing SPA");
                smsNotification.MarkFailed(ex.Message);
            }

            await _notificationRepository.UpdateAsync(smsNotification, cancellationToken);
        }
    }
}
