using MediatR;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Commands.ProcessDailyAttendance;

/// <summary>
/// Handler for processing daily attendance - typically run as a background job
/// </summary>
public class ProcessDailyAttendanceHandler : IRequestHandler<ProcessDailyAttendanceCommand, ProcessDailyAttendanceResult>
{
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly IAttendanceSettingsRepository _settingsRepository;
    private readonly ITimeCalculationService _timeCalculationService;

    public ProcessDailyAttendanceHandler(
        IAttendanceEventRepository eventRepository,
        IAttendanceSummaryRepository summaryRepository,
        ISitePhotoAttendanceRepository spaRepository,
        IAttendanceSettingsRepository settingsRepository,
        ITimeCalculationService timeCalculationService)
    {
        _eventRepository = eventRepository;
        _summaryRepository = summaryRepository;
        _spaRepository = spaRepository;
        _settingsRepository = settingsRepository;
        _timeCalculationService = timeCalculationService;
    }

    public async Task<ProcessDailyAttendanceResult> Handle(ProcessDailyAttendanceCommand request, CancellationToken cancellationToken)
    {
        var result = new ProcessDailyAttendanceResult();
        var errors = new List<string>();

        try
        {
            // Get settings for expected hours
            var settings = await _settingsRepository.GetByTenantAsync(request.TenantId, cancellationToken);
            var expectedHours = settings?.ExpectedHoursPerDay ?? 7.5m;

            // Get all unprocessed events for the date
            var unprocessedEvents = await _eventRepository.GetUnprocessedAsync(
                request.TenantId,
                request.Date,
                cancellationToken);

            var eventsList = unprocessedEvents.ToList();
            result = result with { EventsProcessed = eventsList.Count };

            // Group events by employee and site
            var groupedEvents = eventsList
                .Where(e => !e.IsNoise)
                .GroupBy(e => new { e.EmployeeId, e.SiteId });

            int summariesCreated = 0;
            int summariesUpdated = 0;

            foreach (var group in groupedEvents)
            {
                try
                {
                    var employeeId = group.Key.EmployeeId;
                    var siteId = group.Key.SiteId;
                    var events = group.OrderBy(e => e.Timestamp).ToList();

                    // Get or create summary
                    var summary = await _summaryRepository.GetByEmployeeSiteDateAsync(
                        request.TenantId,
                        employeeId,
                        siteId,
                        request.Date,
                        cancellationToken);

                    bool isNew = false;
                    if (summary == null)
                    {
                        summary = AttendanceSummary.Create(
                            request.TenantId,
                            employeeId,
                            siteId,
                            request.Date,
                            expectedHours);
                        isNew = true;
                    }

                    // Calculate time on site
                    var timeOnSite = _timeCalculationService.CalculateTimeOnSite(events);

                    // Get first entry and last exit
                    var firstEntry = events
                        .Where(e => e.EventType == EventType.Enter)
                        .OrderBy(e => e.Timestamp)
                        .FirstOrDefault()?.Timestamp;

                    var lastExit = events
                        .Where(e => e.EventType == EventType.Exit)
                        .OrderByDescending(e => e.Timestamp)
                        .FirstOrDefault()?.Timestamp;

                    var entryCount = events.Count(e => e.EventType == EventType.Enter);
                    var exitCount = events.Count(e => e.EventType == EventType.Exit);

                    summary.UpdateFromEvents(
                        firstEntry,
                        lastExit,
                        timeOnSite.TotalMinutes,
                        entryCount,
                        exitCount);

                    // Check for SPA
                    var hasSpa = await _spaRepository.ExistsForEmployeeSiteDateAsync(
                        request.TenantId,
                        employeeId,
                        siteId,
                        request.Date,
                        cancellationToken);

                    summary.MarkHasSpa(hasSpa);

                    if (isNew)
                    {
                        await _summaryRepository.AddAsync(summary, cancellationToken);
                        summariesCreated++;
                    }
                    else
                    {
                        await _summaryRepository.UpdateAsync(summary, cancellationToken);
                        summariesUpdated++;
                    }

                    // Mark events as processed
                    foreach (var evt in events)
                    {
                        evt.MarkAsProcessed();
                        await _eventRepository.UpdateAsync(evt, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing events for employee {group.Key.EmployeeId} at site {group.Key.SiteId}: {ex.Message}");
                }
            }

            result = result with
            {
                SummariesCreated = summariesCreated,
                SummariesUpdated = summariesUpdated,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            errors.Add($"Fatal error processing daily attendance: {ex.Message}");
            result = result with { Errors = errors };
        }

        return result;
    }
}
