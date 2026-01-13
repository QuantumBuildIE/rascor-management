using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.API.Controllers.SiteAttendance;

/// <summary>
/// Admin endpoints for manually triggering attendance processing (for testing/demo purposes)
/// </summary>
[ApiController]
[Route("api/admin/site-attendance")]
[Authorize(Policy = "SiteAttendance.Admin")]
public class AdminAttendanceProcessingController : ControllerBase
{
    private readonly ITimeCalculationService _timeCalculationService;
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminAttendanceProcessingController> _logger;

    public AdminAttendanceProcessingController(
        ITimeCalculationService timeCalculationService,
        IAttendanceEventRepository eventRepository,
        IAttendanceSummaryRepository summaryRepository,
        ICurrentUserService currentUserService,
        ILogger<AdminAttendanceProcessingController> logger)
    {
        _timeCalculationService = timeCalculationService;
        _eventRepository = eventRepository;
        _summaryRepository = summaryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Manually process attendance summaries for a date range.
    /// Useful for testing/demo when the daily Hangfire job hasn't run yet.
    /// </summary>
    [HttpPost("process-summaries")]
    [ProducesResponseType(typeof(ProcessSummariesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessSummaries(
        [FromBody] ProcessSummariesRequest? request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Default to last 30 days if no dates provided
        var fromDate = request?.FromDate ?? today.AddDays(-30);
        var toDate = request?.ToDate ?? today.AddDays(-1);

        // Validate date range
        if (fromDate > toDate)
        {
            return BadRequest(new { error = "FromDate must be before or equal to ToDate" });
        }

        if (toDate > today)
        {
            return BadRequest(new { error = "ToDate cannot be in the future" });
        }

        _logger.LogInformation(
            "Starting manual attendance processing for tenant {TenantId} from {FromDate} to {ToDate}",
            tenantId, fromDate, toDate);

        var details = new List<DateProcessingDetail>();
        var totalEventsProcessed = 0;
        var totalSummariesCreated = 0;
        var datesProcessed = 0;

        // Get all unique dates that have events in the range
        var datesWithEvents = await _eventRepository.GetUniqueDatesWithEventsAsync(
            tenantId, fromDate, toDate, cancellationToken);

        foreach (var date in datesWithEvents)
        {
            try
            {
                var (eventsProcessed, summariesCreated) = await _timeCalculationService
                    .ProcessDailyAttendanceWithCountsAsync(tenantId, date, cancellationToken);

                if (eventsProcessed > 0)
                {
                    datesProcessed++;
                    totalEventsProcessed += eventsProcessed;
                    totalSummariesCreated += summariesCreated;

                    details.Add(new DateProcessingDetail
                    {
                        Date = date,
                        EventsProcessed = eventsProcessed,
                        SummariesCreated = summariesCreated
                    });

                    _logger.LogInformation(
                        "Processed {Events} events for date {Date}, created/updated {Summaries} summaries",
                        eventsProcessed, date, summariesCreated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing attendance for date {Date}", date);
            }
        }

        var result = new ProcessSummariesResult
        {
            DatesProcessed = datesProcessed,
            SummariesCreated = totalSummariesCreated,
            EventsProcessed = totalEventsProcessed,
            FromDate = fromDate,
            ToDate = toDate,
            Details = details
        };

        _logger.LogInformation(
            "Manual attendance processing completed: {DatesProcessed} dates, {EventsProcessed} events, {SummariesCreated} summaries",
            datesProcessed, totalEventsProcessed, totalSummariesCreated);

        return Ok(result);
    }

    /// <summary>
    /// Get processing status for attendance events.
    /// Shows counts of processed vs unprocessed events and summary counts by date.
    /// </summary>
    [HttpGet("processing-status")]
    [ProducesResponseType(typeof(ProcessingStatusResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessingStatus(
        [FromQuery] int daysBack = 30,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = today.AddDays(-daysBack);

        // Get event processing stats
        var (total, processed, unprocessed) = await _eventRepository
            .GetProcessingStatsAsync(tenantId, cancellationToken);

        // Get total summary count
        var totalSummaries = await _summaryRepository.GetTotalCountAsync(tenantId, cancellationToken);

        // Get summaries by date
        var summariesByDate = await _summaryRepository
            .GetCountsByDateAsync(tenantId, fromDate, today, cancellationToken);

        // Get events by date
        var eventsByDate = await _eventRepository
            .GetEventCountsByDateAsync(tenantId, fromDate, today, cancellationToken);

        var result = new ProcessingStatusResult
        {
            TotalEvents = total,
            ProcessedEvents = processed,
            UnprocessedEvents = unprocessed,
            TotalSummaries = totalSummaries,
            SummariesByDate = summariesByDate.Select(x => new DateSummaryCount
            {
                Date = x.Date,
                Count = x.Count
            }),
            EventsByDate = eventsByDate.Select(x => new DateEventCount
            {
                Date = x.Date,
                TotalEvents = x.Total,
                ProcessedEvents = x.Processed,
                UnprocessedEvents = x.Unprocessed
            })
        };

        return Ok(result);
    }
}
