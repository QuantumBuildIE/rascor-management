using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service implementation for sending RAMS workflow notifications
/// </summary>
public class RamsNotificationService : IRamsNotificationService
{
    private readonly IRamsDbContext _ramsContext;
    private readonly ICoreDbContext _coreContext;
    private readonly IEmailService _emailService;
    private readonly IRamsEmailTemplateService _templateService;
    private readonly IRamsDashboardService _dashboardService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RamsNotificationService> _logger;
    private readonly string _baseUrl;

    public RamsNotificationService(
        IRamsDbContext ramsContext,
        ICoreDbContext coreContext,
        IEmailService emailService,
        IRamsEmailTemplateService templateService,
        IRamsDashboardService dashboardService,
        IConfiguration configuration,
        ILogger<RamsNotificationService> logger)
    {
        _ramsContext = ramsContext;
        _coreContext = coreContext;
        _emailService = emailService;
        _templateService = templateService;
        _dashboardService = dashboardService;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://rascorweb-production.up.railway.app";
    }

    public async Task SendSubmitNotificationAsync(
        Guid documentId,
        string submittedByUserId,
        string submittedByUserName,
        CancellationToken cancellationToken = default)
    {
        if (!IsNotificationEnabled("SendOnSubmit"))
        {
            _logger.LogDebug("Submit notifications are disabled");
            return;
        }

        var document = await _ramsContext.RamsDocuments
            .Include(d => d.RiskAssessments)
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Cannot send submit notification: Document {DocumentId} not found", documentId);
            return;
        }

        // Get safety officer email if assigned
        string? recipientEmail = null;
        string recipientName = "Safety Officer";

        if (document.SafetyOfficerId.HasValue)
        {
            var employee = await _coreContext.Employees
                .Where(e => e.Id == document.SafetyOfficerId.Value)
                .Select(e => new { e.FirstName, e.LastName, e.Email })
                .FirstOrDefaultAsync(cancellationToken);

            if (employee != null && !string.IsNullOrEmpty(employee.Email))
            {
                recipientEmail = employee.Email;
                recipientName = $"{employee.FirstName} {employee.LastName}";
            }
        }

        // Fall back to configured approvers if no safety officer
        if (string.IsNullOrEmpty(recipientEmail))
        {
            var approverEmails = _configuration.GetSection("RamsNotifications:ApproverEmails")
                .Get<List<string>>() ?? [];

            if (!approverEmails.Any())
            {
                _logger.LogWarning(
                    "Cannot send submit notification for {DocumentId}: No safety officer assigned and no approver emails configured",
                    documentId);
                return;
            }

            // Send to first configured approver
            recipientEmail = approverEmails.First();
            recipientName = "RAMS Approver";
        }

        var documentUrl = $"{_baseUrl}/rams/{documentId}";
        var highRiskCount = document.RiskAssessments.Count(r => r.ResidualRiskLevel == RiskLevel.High);

        var template = _templateService.GetSubmitTemplate(
            document.ProjectReference,
            document.ProjectName,
            submittedByUserName,
            documentUrl,
            document.RiskAssessments.Count,
            highRiskCount);

        await SendAndLogNotificationAsync(
            documentId,
            "Submit",
            recipientEmail,
            recipientName,
            template.Subject,
            template.HtmlBody,
            submittedByUserId,
            submittedByUserName,
            cancellationToken);
    }

    public async Task SendApprovalNotificationAsync(
        Guid documentId,
        string approvedByUserId,
        string approvedByUserName,
        CancellationToken cancellationToken = default)
    {
        if (!IsNotificationEnabled("SendOnApprove"))
        {
            _logger.LogDebug("Approval notifications are disabled");
            return;
        }

        var document = await _ramsContext.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Cannot send approval notification: Document {DocumentId} not found", documentId);
            return;
        }

        // Get document creator's email
        var creatorEmail = await GetDocumentCreatorEmailAsync(document.CreatedBy, cancellationToken);

        if (string.IsNullOrEmpty(creatorEmail))
        {
            _logger.LogWarning(
                "Cannot send approval notification for {DocumentId}: Creator email not found",
                documentId);
            return;
        }

        var documentUrl = $"{_baseUrl}/rams/{documentId}";

        var template = _templateService.GetApprovalTemplate(
            document.ProjectReference,
            document.ProjectName,
            approvedByUserName,
            documentUrl);

        await SendAndLogNotificationAsync(
            documentId,
            "Approve",
            creatorEmail,
            "Document Creator",
            template.Subject,
            template.HtmlBody,
            approvedByUserId,
            approvedByUserName,
            cancellationToken);
    }

    public async Task SendRejectionNotificationAsync(
        Guid documentId,
        string rejectedByUserId,
        string rejectedByUserName,
        string? rejectionComments,
        CancellationToken cancellationToken = default)
    {
        if (!IsNotificationEnabled("SendOnReject"))
        {
            _logger.LogDebug("Rejection notifications are disabled");
            return;
        }

        var document = await _ramsContext.RamsDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Cannot send rejection notification: Document {DocumentId} not found", documentId);
            return;
        }

        // Get document creator's email
        var creatorEmail = await GetDocumentCreatorEmailAsync(document.CreatedBy, cancellationToken);

        if (string.IsNullOrEmpty(creatorEmail))
        {
            _logger.LogWarning(
                "Cannot send rejection notification for {DocumentId}: Creator email not found",
                documentId);
            return;
        }

        var documentUrl = $"{_baseUrl}/rams/{documentId}";

        var template = _templateService.GetRejectionTemplate(
            document.ProjectReference,
            document.ProjectName,
            rejectedByUserName,
            rejectionComments,
            documentUrl);

        await SendAndLogNotificationAsync(
            documentId,
            "Reject",
            creatorEmail,
            "Document Creator",
            template.Subject,
            template.HtmlBody,
            rejectedByUserId,
            rejectedByUserName,
            cancellationToken);
    }

    public async Task SendDailyDigestAsync(CancellationToken cancellationToken = default)
    {
        if (!IsNotificationEnabled("DailyDigestEnabled"))
        {
            _logger.LogDebug("Daily digest is disabled");
            return;
        }

        var recipients = _configuration.GetSection("RamsNotifications:DigestRecipients")
            .Get<List<string>>() ?? [];

        if (!recipients.Any())
        {
            _logger.LogInformation("No digest recipients configured, skipping daily digest");
            return;
        }

        var pendingApprovals = await _dashboardService.GetPendingApprovalsAsync(cancellationToken);
        var overdueDocuments = await _dashboardService.GetOverdueDocumentsAsync(cancellationToken);

        if (!pendingApprovals.Any() && !overdueDocuments.Any())
        {
            _logger.LogInformation("No pending or overdue documents, skipping daily digest");
            return;
        }

        var dashboardUrl = $"{_baseUrl}/rams/dashboard";

        var template = _templateService.GetDailyDigestTemplate(
            pendingApprovals.Count,
            overdueDocuments.Count,
            pendingApprovals,
            overdueDocuments,
            dashboardUrl);

        foreach (var recipientEmail in recipients)
        {
            await SendAndLogNotificationAsync(
                null,
                "Digest",
                recipientEmail,
                "Digest Recipient",
                template.Subject,
                template.HtmlBody,
                null,
                "System",
                cancellationToken);
        }

        _logger.LogInformation(
            "Daily digest sent to {RecipientCount} recipients with {PendingCount} pending and {OverdueCount} overdue items",
            recipients.Count, pendingApprovals.Count, overdueDocuments.Count);
    }

    public async Task<List<RamsNotificationDto>> GetNotificationHistoryAsync(
        Guid? documentId = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _ramsContext.RamsNotificationLogs
            .AsNoTracking()
            .OrderByDescending(n => n.AttemptedAt);

        if (documentId.HasValue)
        {
            query = (IOrderedQueryable<RamsNotificationLog>)query
                .Where(n => n.RamsDocumentId == documentId);
        }

        var notifications = await query
            .Take(limit)
            .ToListAsync(cancellationToken);

        // Get document details for the notifications
        var docIds = notifications
            .Where(n => n.RamsDocumentId.HasValue)
            .Select(n => n.RamsDocumentId!.Value)
            .Distinct()
            .ToList();

        var documents = await _ramsContext.RamsDocuments
            .Where(d => docIds.Contains(d.Id))
            .Select(d => new { d.Id, d.ProjectReference, d.ProjectName })
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        return notifications.Select(n => new RamsNotificationDto
        {
            Id = n.Id,
            DocumentId = n.RamsDocumentId,
            ProjectReference = n.RamsDocumentId.HasValue && documents.TryGetValue(n.RamsDocumentId.Value, out var doc)
                ? doc.ProjectReference
                : string.Empty,
            ProjectName = n.RamsDocumentId.HasValue && documents.TryGetValue(n.RamsDocumentId.Value, out var doc2)
                ? doc2.ProjectName
                : string.Empty,
            NotificationType = n.NotificationType,
            RecipientEmail = n.RecipientEmail,
            RecipientName = n.RecipientName,
            Subject = n.Subject,
            BodyPreview = n.BodyPreview,
            AttemptedAt = n.AttemptedAt,
            WasSent = n.WasSent,
            ErrorMessage = n.ErrorMessage,
            RetryCount = n.RetryCount,
            TriggeredByUserName = n.TriggeredByUserName
        }).ToList();
    }

    public async Task RetryFailedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var failedNotifications = await _ramsContext.RamsNotificationLogs
            .Where(n => !n.WasSent && n.RetryCount < 3)
            .OrderBy(n => n.AttemptedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (!failedNotifications.Any())
        {
            _logger.LogDebug("No failed notifications to retry");
            return;
        }

        foreach (var notification in failedNotifications)
        {
            notification.RetryCount++;
            notification.AttemptedAt = DateTime.UtcNow;

            try
            {
                // We don't have the original HTML body stored, so we can only log the retry
                // In a production system, you might want to store the full body or regenerate it
                _logger.LogInformation(
                    "Retry {RetryCount} for notification {NotificationId} to {Email}",
                    notification.RetryCount, notification.Id, notification.RecipientEmail);

                // For now, just update the retry count
                // A real implementation would re-send the email
            }
            catch (Exception ex)
            {
                notification.ErrorMessage = ex.Message;
                _logger.LogError(ex,
                    "Failed retry {RetryCount} for notification {NotificationId}",
                    notification.RetryCount, notification.Id);
            }
        }

        await _ramsContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SendTestNotificationAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var template = _templateService.GetTestTemplate();

        try
        {
            await _emailService.SendEmailAsync(email, template.Subject, template.HtmlBody, cancellationToken);

            await LogNotificationAsync(
                null,
                "Test",
                email,
                "Test Recipient",
                template.Subject,
                true,
                null,
                null,
                "System",
                cancellationToken);

            _logger.LogInformation("Test notification sent successfully to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            await LogNotificationAsync(
                null,
                "Test",
                email,
                "Test Recipient",
                template.Subject,
                false,
                ex.Message,
                null,
                "System",
                cancellationToken);

            _logger.LogError(ex, "Failed to send test notification to {Email}", email);
            return false;
        }
    }

    private bool IsNotificationEnabled(string settingName)
    {
        // Check if notifications are globally enabled
        var globalEnabled = _configuration.GetValue<bool>("RamsNotifications:Enabled", true);
        if (!globalEnabled)
            return false;

        // Check specific setting
        return _configuration.GetValue<bool>($"RamsNotifications:{settingName}", true);
    }

    private async Task<string?> GetDocumentCreatorEmailAsync(string createdBy, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(createdBy) || !Guid.TryParse(createdBy, out var userId))
            return null;

        return await _coreContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task SendAndLogNotificationAsync(
        Guid? documentId,
        string notificationType,
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        string? triggeredByUserId,
        string? triggeredByUserName,
        CancellationToken cancellationToken)
    {
        bool success = false;
        string? errorMessage = null;

        try
        {
            await _emailService.SendEmailAsync(recipientEmail, subject, htmlBody, cancellationToken);
            success = true;

            _logger.LogInformation(
                "Sent {NotificationType} notification for document {DocumentId} to {Email}",
                notificationType, documentId, recipientEmail);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            _logger.LogError(ex,
                "Failed to send {NotificationType} notification for document {DocumentId} to {Email}",
                notificationType, documentId, recipientEmail);
        }

        await LogNotificationAsync(
            documentId,
            notificationType,
            recipientEmail,
            recipientName,
            subject,
            success,
            errorMessage,
            triggeredByUserId,
            triggeredByUserName,
            cancellationToken);
    }

    private async Task LogNotificationAsync(
        Guid? documentId,
        string notificationType,
        string recipientEmail,
        string recipientName,
        string subject,
        bool wasSent,
        string? errorMessage,
        string? triggeredByUserId,
        string? triggeredByUserName,
        CancellationToken cancellationToken)
    {
        var log = new RamsNotificationLog
        {
            RamsDocumentId = documentId,
            NotificationType = notificationType,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            Subject = subject,
            BodyPreview = $"[{notificationType}] {subject}",
            AttemptedAt = DateTime.UtcNow,
            WasSent = wasSent,
            ErrorMessage = errorMessage,
            RetryCount = 0,
            TriggeredByUserId = triggeredByUserId,
            TriggeredByUserName = triggeredByUserName
        };

        _ramsContext.RamsNotificationLogs.Add(log);
        await _ramsContext.SaveChangesAsync(cancellationToken);
    }
}
