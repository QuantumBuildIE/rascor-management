using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetDashboardKpis;

public class GetDashboardKpisHandler : IRequestHandler<GetDashboardKpisQuery, DashboardKpisDto>
{
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ITimeCalculationService _timeCalculationService;
    private readonly IAttendanceSettingsRepository _settingsRepository;

    public GetDashboardKpisHandler(
        IAttendanceSummaryRepository summaryRepository,
        ITimeCalculationService timeCalculationService,
        IAttendanceSettingsRepository settingsRepository)
    {
        _summaryRepository = summaryRepository;
        _timeCalculationService = timeCalculationService;
        _settingsRepository = settingsRepository;
    }

    public async Task<DashboardKpisDto> Handle(GetDashboardKpisQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByTenantAsync(request.TenantId, cancellationToken);
        var expectedHoursPerDay = settings?.ExpectedHoursPerDay ?? 7.5m;

        var summaries = await _summaryRepository.GetByDateRangeAsync(
            request.TenantId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        if (request.SiteId.HasValue)
            summaries = summaries.Where(s => s.SiteId == request.SiteId.Value);

        var summaryList = summaries.ToList();

        var workingDays = await _timeCalculationService.GetWorkingDaysAsync(
            request.TenantId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var uniqueEmployees = summaryList.Select(s => s.EmployeeId).Distinct().Count();
        var uniqueSites = summaryList.Select(s => s.SiteId).Distinct().Count();

        var totalActualMinutes = summaryList.Sum(s => s.TimeOnSiteMinutes);
        var totalActualHours = Math.Round(totalActualMinutes / 60.0m, 2);

        var expectedHours = uniqueEmployees * workingDays * expectedHoursPerDay;
        var utilization = expectedHours > 0
            ? Math.Round((totalActualHours / expectedHours) * 100, 2)
            : 0;

        var avgHoursPerDay = workingDays > 0 && uniqueEmployees > 0
            ? Math.Round(totalActualHours / (workingDays * uniqueEmployees), 2)
            : 0;

        return new DashboardKpisDto
        {
            OverallUtilization = utilization,
            AverageHoursPerDay = avgHoursPerDay,
            TotalActiveEmployees = uniqueEmployees,
            TotalActiveSites = uniqueSites,
            ExcellentCount = summaryList.Count(s => s.Status == AttendanceStatus.Excellent),
            GoodCount = summaryList.Count(s => s.Status == AttendanceStatus.Good),
            BelowTargetCount = summaryList.Count(s => s.Status == AttendanceStatus.BelowTarget),
            AbsentCount = summaryList.Count(s => s.Status == AttendanceStatus.Absent),
            ExpectedHours = expectedHours,
            ActualHours = totalActualHours,
            VarianceHours = totalActualHours - expectedHours,
            WorkingDays = workingDays,
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };
    }
}
