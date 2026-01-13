using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Domain.ValueObjects;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services;

public class TimeCalculationService : ITimeCalculationService
{
    private readonly IBankHolidayRepository _bankHolidayRepository;
    private readonly IAttendanceSettingsRepository _settingsRepository;
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;

    public TimeCalculationService(
        IBankHolidayRepository bankHolidayRepository,
        IAttendanceSettingsRepository settingsRepository,
        IAttendanceEventRepository eventRepository,
        IAttendanceSummaryRepository summaryRepository)
    {
        _bankHolidayRepository = bankHolidayRepository;
        _settingsRepository = settingsRepository;
        _eventRepository = eventRepository;
        _summaryRepository = summaryRepository;
    }

    public TimeOnSite CalculateTimeOnSite(IEnumerable<AttendanceEvent> events)
    {
        var orderedEvents = events
            .Where(e => !e.IsNoise)
            .OrderBy(e => e.Timestamp)
            .ToList();

        if (!orderedEvents.Any())
            return TimeOnSite.Zero;

        var totalMinutes = 0;
        DateTime? entryTime = null;

        foreach (var evt in orderedEvents)
        {
            if (evt.EventType == EventType.Enter)
            {
                // If we had a previous entry without exit, use this as new entry
                entryTime = evt.Timestamp;
            }
            else if (evt.EventType == EventType.Exit && entryTime.HasValue)
            {
                var duration = evt.Timestamp - entryTime.Value;
                if (duration.TotalMinutes > 0)
                {
                    totalMinutes += (int)duration.TotalMinutes;
                }
                entryTime = null;
            }
        }

        return new TimeOnSite(totalMinutes);
    }

    public async Task<int> GetWorkingDaysAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetByTenantAsync(tenantId, cancellationToken);
        var bankHolidays = await _bankHolidayRepository.GetBankHolidayDatesAsync(tenantId, fromDate, toDate, cancellationToken);
        var bankHolidaySet = bankHolidays.ToHashSet();

        var workingDays = 0;
        var currentDate = fromDate;

        while (currentDate <= toDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var isWeekend = (dayOfWeek == DayOfWeek.Saturday && !(settings?.IncludeSaturday ?? false)) ||
                           (dayOfWeek == DayOfWeek.Sunday && !(settings?.IncludeSunday ?? false));
            var isBankHoliday = bankHolidaySet.Contains(currentDate);

            if (!isWeekend && !isBankHoliday)
                workingDays++;

            currentDate = currentDate.AddDays(1);
        }

        return workingDays;
    }

    public async Task<bool> IsWorkingDayAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetByTenantAsync(tenantId, cancellationToken);
        var dayOfWeek = date.DayOfWeek;

        // Check weekend
        if (dayOfWeek == DayOfWeek.Saturday && !(settings?.IncludeSaturday ?? false))
            return false;
        if (dayOfWeek == DayOfWeek.Sunday && !(settings?.IncludeSunday ?? false))
            return false;

        // Check bank holiday
        var isBankHoliday = await _bankHolidayRepository.IsBankHolidayAsync(tenantId, date, cancellationToken);
        return !isBankHoliday;
    }

    public decimal CalculateUtilization(decimal actualHours, decimal expectedHours)
    {
        if (expectedHours <= 0) return 0;
        return Math.Round((actualHours / expectedHours) * 100, 2);
    }

    public async Task ProcessDailyAttendanceAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        await ProcessDailyAttendanceWithCountsAsync(tenantId, date, cancellationToken);
    }

    public async Task<(int EventsProcessed, int SummariesCreated)> ProcessDailyAttendanceWithCountsAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetByTenantAsync(tenantId, cancellationToken);
        var expectedHours = settings?.ExpectedHoursPerDay ?? 7.5m;

        // Get all unprocessed events for the date
        var events = await _eventRepository.GetUnprocessedAsync(tenantId, date, cancellationToken);
        var eventsList = events.ToList();
        var eventsProcessed = eventsList.Count;
        var summariesCreated = 0;

        // Group by employee and site
        var groupedEvents = eventsList.GroupBy(e => new { e.EmployeeId, e.SiteId });

        foreach (var group in groupedEvents)
        {
            var employeeId = group.Key.EmployeeId;
            var siteId = group.Key.SiteId;
            var groupEventsList = group.ToList();

            // Calculate time on site
            var timeOnSite = CalculateTimeOnSite(groupEventsList);

            // Get or create summary
            var summary = await _summaryRepository.GetByEmployeeSiteDateAsync(tenantId, employeeId, siteId, date, cancellationToken);

            if (summary == null)
            {
                summary = AttendanceSummary.Create(tenantId, employeeId, siteId, date, expectedHours);
                await _summaryRepository.AddAsync(summary, cancellationToken);
                summariesCreated++;
            }

            // Update summary
            var firstEntry = groupEventsList.Where(e => e.EventType == EventType.Enter).MinBy(e => e.Timestamp)?.Timestamp;
            var lastExit = groupEventsList.Where(e => e.EventType == EventType.Exit).MaxBy(e => e.Timestamp)?.Timestamp;
            var entryCount = groupEventsList.Count(e => e.EventType == EventType.Enter);
            var exitCount = groupEventsList.Count(e => e.EventType == EventType.Exit);

            summary.UpdateFromEvents(firstEntry, lastExit, timeOnSite.TotalMinutes, entryCount, exitCount);
            await _summaryRepository.UpdateAsync(summary, cancellationToken);

            // Mark events as processed
            foreach (var evt in groupEventsList)
            {
                evt.MarkAsProcessed();
                await _eventRepository.UpdateAsync(evt, cancellationToken);
            }
        }

        return (eventsProcessed, summariesCreated);
    }
}
