using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.API.Controllers;

/// <summary>
/// API controller for managing RAMS email notifications
/// </summary>
[ApiController]
[Route("api/rams/notifications")]
[Authorize]
public class RamsNotificationsController : ControllerBase
{
    private readonly IRamsNotificationService _notificationService;
    private readonly ILogger<RamsNotificationsController> _logger;

    public RamsNotificationsController(
        IRamsNotificationService notificationService,
        ILogger<RamsNotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets notification history for a specific document or all notifications
    /// </summary>
    /// <param name="documentId">Optional document ID to filter by</param>
    /// <param name="limit">Maximum number of records (default 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of notification history records</returns>
    [HttpGet]
    [Authorize(Policy = "Rams.View")]
    [ProducesResponseType(typeof(List<RamsNotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RamsNotificationDto>>> GetNotificationHistory(
        [FromQuery] Guid? documentId = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationService.GetNotificationHistoryAsync(documentId, limit, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Gets notification history for a specific document
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of notification history records for the document</returns>
    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = "Rams.View")]
    [ProducesResponseType(typeof(List<RamsNotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RamsNotificationDto>>> GetDocumentNotificationHistory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationService.GetNotificationHistoryAsync(id, 100, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Sends a test notification to verify email configuration
    /// </summary>
    /// <param name="request">Test notification request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("test")]
    [Authorize(Policy = "Rams.Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendTestNotification(
        [FromBody] SendTestNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email address is required" });
        }

        _logger.LogInformation("Sending test notification to {Email}", request.Email);

        var success = await _notificationService.SendTestNotificationAsync(request.Email, cancellationToken);

        if (success)
        {
            return Ok(new { message = "Test notification sent successfully", email = request.Email });
        }

        return BadRequest(new { message = "Failed to send test notification. Check the logs for details." });
    }

    /// <summary>
    /// Manually triggers the daily digest (admin only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("digest/send")]
    [Authorize(Policy = "Rams.Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendDailyDigest(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Manually triggering RAMS daily digest");
            await _notificationService.SendDailyDigestAsync(cancellationToken);
            return Ok(new { message = "Daily digest sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily digest");
            return StatusCode(500, new { message = "Failed to send daily digest", error = ex.Message });
        }
    }

    /// <summary>
    /// Retries failed notifications (admin only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("retry")]
    [Authorize(Policy = "Rams.Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RetryFailedNotifications(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrying failed RAMS notifications");
            await _notificationService.RetryFailedNotificationsAsync(cancellationToken);
            return Ok(new { message = "Failed notifications retry initiated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry notifications");
            return StatusCode(500, new { message = "Failed to retry notifications", error = ex.Message });
        }
    }
}
