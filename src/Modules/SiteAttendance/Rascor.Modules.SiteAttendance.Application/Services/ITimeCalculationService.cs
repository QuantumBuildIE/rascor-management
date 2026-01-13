using Rascor.Modules.SiteAttendance.Domain.Entities;
using Rascor.Modules.SiteAttendance.Domain.ValueObjects;

namespace Rascor.Modules.SiteAttendance.Application.Services;

public interface ITimeCalculationService
{
    /// <summary>
    /// Calculates time on site from a list of events for a single day
    /// </summary>
    TimeOnSite CalculateTimeOnSite(IEnumerable<AttendanceEvent> events);

    /// <summary>
    /// Calculates working days between two dates, excluding weekends and bank holidays
    /// </summary>
    Task<int> GetWorkingDaysAsync(Guid tenantId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a date is a working day (not weekend, not bank holiday)
    /// </summary>
    Task<bool> IsWorkingDayAsync(Guid tenantId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates utilization percentage
    /// </summary>
    decimal CalculateUtilization(decimal actualHours, decimal expectedHours);

    /// <summary>
    /// Processes all unprocessed events for a date and creates/updates summaries
    /// </summary>
    Task ProcessDailyAttendanceAsync(Guid tenantId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes all unprocessed events for a date and returns counts
    /// </summary>
    /// <returns>Tuple of (events processed, summaries created/updated)</returns>
    Task<(int EventsProcessed, int SummariesCreated)> ProcessDailyAttendanceWithCountsAsync(Guid tenantId, DateOnly date, CancellationToken cancellationToken = default);
}
