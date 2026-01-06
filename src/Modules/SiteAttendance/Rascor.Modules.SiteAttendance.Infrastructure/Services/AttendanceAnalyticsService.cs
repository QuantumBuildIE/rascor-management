using Microsoft.EntityFrameworkCore;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;
using Rascor.Modules.SiteAttendance.Infrastructure.Persistence;

namespace Rascor.Modules.SiteAttendance.Infrastructure.Services;

public class AttendanceAnalyticsService : IAttendanceAnalyticsService
{
    private readonly SiteAttendanceDbContext _dbContext;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly ITimeCalculationService _timeCalculationService;

    public AttendanceAnalyticsService(
        SiteAttendanceDbContext dbContext,
        IAttendanceSummaryRepository summaryRepository,
        ISitePhotoAttendanceRepository spaRepository,
        ITimeCalculationService timeCalculationService)
    {
        _dbContext = dbContext;
        _summaryRepository = summaryRepository;
        _spaRepository = spaRepository;
        _timeCalculationService = timeCalculationService;
    }

    public async Task<DashboardKpisDto> GetDashboardKpisAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var summaries = await _dbContext.AttendanceSummaries
            .Where(s => s.TenantId == tenantId
                && s.Date >= fromDate
                && s.Date <= toDate)
            .ToListAsync(cancellationToken);

        var workingDays = await _timeCalculationService.GetWorkingDaysAsync(tenantId, fromDate, toDate, cancellationToken);

        var totalActualMinutes = summaries.Sum(s => s.TimeOnSiteMinutes);
        var totalExpectedHours = summaries.Sum(s => s.ExpectedHours);
        var totalActualHours = Math.Round(totalActualMinutes / 60.0m, 2);

        var overallUtilization = totalExpectedHours > 0
            ? Math.Round((totalActualHours / totalExpectedHours) * 100, 2)
            : 0m;

        var activeEmployees = summaries.Select(s => s.EmployeeId).Distinct().Count();
        var activeSites = summaries.Select(s => s.SiteId).Distinct().Count();

        var excellentCount = summaries.Count(s => s.Status == AttendanceStatus.Excellent);
        var goodCount = summaries.Count(s => s.Status == AttendanceStatus.Good);
        var belowTargetCount = summaries.Count(s => s.Status == AttendanceStatus.BelowTarget);
        var absentCount = summaries.Count(s => s.Status == AttendanceStatus.Absent);

        var averageHoursPerDay = summaries.Any()
            ? Math.Round(totalActualHours / summaries.Count, 2)
            : 0m;

        return new DashboardKpisDto
        {
            OverallUtilization = overallUtilization,
            AverageHoursPerDay = averageHoursPerDay,
            TotalActiveEmployees = activeEmployees,
            TotalActiveSites = activeSites,
            ExcellentCount = excellentCount,
            GoodCount = goodCount,
            BelowTargetCount = belowTargetCount,
            AbsentCount = absentCount,
            ExpectedHours = totalExpectedHours,
            ActualHours = totalActualHours,
            VarianceHours = totalActualHours - totalExpectedHours,
            WorkingDays = workingDays,
            FromDate = fromDate,
            ToDate = toDate
        };
    }

    public async Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var summaries = await _dbContext.AttendanceSummaries
            .Include(s => s.Employee)
            .Where(s => s.TenantId == tenantId
                && s.Date >= fromDate
                && s.Date <= toDate)
            .ToListAsync(cancellationToken);

        var spaRecords = await _dbContext.SitePhotoAttendances
            .Where(s => s.TenantId == tenantId
                && s.EventDate >= fromDate
                && s.EventDate <= toDate)
            .ToListAsync(cancellationToken);

        var workingDays = await _timeCalculationService.GetWorkingDaysAsync(tenantId, fromDate, toDate, cancellationToken);

        var employeeGroups = summaries
            .GroupBy(s => new { s.EmployeeId, s.Employee.FirstName, s.Employee.LastName })
            .Select(g =>
            {
                var totalMinutes = g.Sum(s => s.TimeOnSiteMinutes);
                var totalActualHours = Math.Round(totalMinutes / 60.0m, 2);
                var totalExpectedHours = g.Sum(s => s.ExpectedHours);
                var daysPresent = g.Count(s => s.Status != AttendanceStatus.Absent);
                var daysAbsent = workingDays - daysPresent;
                var spaCount = spaRecords.Count(s => s.EmployeeId == g.Key.EmployeeId);

                var utilizationPercent = totalExpectedHours > 0
                    ? Math.Round((totalActualHours / totalExpectedHours) * 100, 2)
                    : 0m;

                var status = utilizationPercent switch
                {
                    >= 90 => "Excellent",
                    >= 75 => "Good",
                    > 0 => "Below Target",
                    _ => "Absent"
                };

                return new EmployeePerformanceDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                    TotalHours = totalActualHours,
                    ExpectedHours = totalExpectedHours,
                    UtilizationPercent = utilizationPercent,
                    VarianceHours = totalActualHours - totalExpectedHours,
                    Status = status,
                    DaysPresent = daysPresent,
                    DaysAbsent = daysAbsent > 0 ? daysAbsent : 0,
                    SpaCount = spaCount
                };
            })
            .OrderByDescending(e => e.UtilizationPercent)
            .ToList();

        return employeeGroups;
    }

    public async Task<IEnumerable<SiteActivityDto>> GetSiteActivityAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var summaries = await _dbContext.AttendanceSummaries
            .Include(s => s.Site)
            .Where(s => s.TenantId == tenantId
                && s.Date >= fromDate
                && s.Date <= toDate)
            .ToListAsync(cancellationToken);

        var events = await _dbContext.AttendanceEvents
            .Where(e => e.TenantId == tenantId
                && e.Timestamp >= DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                && e.Timestamp <= DateTime.SpecifyKind(toDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc))
            .ToListAsync(cancellationToken);

        var siteGroups = summaries
            .GroupBy(s => new { s.SiteId, s.Site.SiteName })
            .Select(g =>
            {
                var totalMinutes = g.Sum(s => s.TimeOnSiteMinutes);
                var totalHours = Math.Round(totalMinutes / 60.0m, 2);
                var employeeCount = g.Select(s => s.EmployeeId).Distinct().Count();
                var daysActive = g.Select(s => s.Date).Distinct().Count();
                var totalEvents = events.Count(e => e.SiteId == g.Key.SiteId);
                var averageHoursPerEmployee = employeeCount > 0
                    ? Math.Round(totalHours / employeeCount, 2)
                    : 0m;

                return new SiteActivityDto
                {
                    SiteId = g.Key.SiteId,
                    SiteName = g.Key.SiteName,
                    EmployeeCount = employeeCount,
                    TotalHours = totalHours,
                    AverageHoursPerEmployee = averageHoursPerEmployee,
                    TotalEvents = totalEvents,
                    DaysActive = daysActive
                };
            })
            .OrderByDescending(s => s.TotalHours)
            .ToList();

        return siteGroups;
    }
}
