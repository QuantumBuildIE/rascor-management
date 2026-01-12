using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service interface for sending RAMS workflow notifications
/// </summary>
public interface IRamsNotificationService
{
    /// <summary>
    /// Sends notification when a RAMS document is submitted for review
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="submittedByUserId">User ID who submitted the document</param>
    /// <param name="submittedByUserName">Display name of the user who submitted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendSubmitNotificationAsync(
        Guid documentId,
        string submittedByUserId,
        string submittedByUserName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notification when a RAMS document is approved
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="approvedByUserId">User ID who approved the document</param>
    /// <param name="approvedByUserName">Display name of the user who approved</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendApprovalNotificationAsync(
        Guid documentId,
        string approvedByUserId,
        string approvedByUserName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notification when a RAMS document is rejected
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="rejectedByUserId">User ID who rejected the document</param>
    /// <param name="rejectedByUserName">Display name of the user who rejected</param>
    /// <param name="rejectionComments">Rejection reason/comments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendRejectionNotificationAsync(
        Guid documentId,
        string rejectedByUserId,
        string rejectedByUserName,
        string? rejectionComments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends daily digest email to configured recipients
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendDailyDigestAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notification history for a document or all notifications
    /// </summary>
    /// <param name="documentId">Optional document ID to filter by</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of notification records</returns>
    Task<List<RamsNotificationDto>> GetNotificationHistoryAsync(
        Guid? documentId = null,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries failed notifications (up to 3 retry attempts)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RetryFailedNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a test notification to verify email configuration
    /// </summary>
    /// <param name="email">Email address to send test to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendTestNotificationAsync(
        string email,
        CancellationToken cancellationToken = default);
}
