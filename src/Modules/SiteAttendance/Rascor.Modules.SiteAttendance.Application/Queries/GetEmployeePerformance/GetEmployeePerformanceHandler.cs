using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Application.Services;
using Rascor.Modules.SiteAttendance.Domain.Enums;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetEmployeePerformance;

public class GetEmployeePerformanceHandler : IRequestHandler<GetEmployeePerformanceQuery, PaginatedList<EmployeePerformanceDto>>
{
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly ITimeCalculationService _timeCalculationService;
    private readonly IAttendanceSettingsRepository _settingsRepository;

    public GetEmployeePerformanceHandler(
        IAttendanceSummaryRepository summaryRepository,
        ISitePhotoAttendanceRepository spaRepository,
        ITimeCalculationService timeCalculationService,
        IAttendanceSettingsRepository settingsRepository)
    {
        _summaryRepository = summaryRepository;
        _spaRepository = spaRepository;
        _timeCalculationService = timeCalculationService;
        _settingsRepository = settingsRepository;
    }

    public async Task<PaginatedList<EmployeePerformanceDto>> Handle(GetEmployeePerformanceQuery request, CancellationToken cancellationToken)
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

        if (request.EmployeeId.HasValue)
            summaries = summaries.Where(s => s.EmployeeId == request.EmployeeId.Value);

        var summaryList = summaries.ToList();

        var workingDays = await _timeCalculationService.GetWorkingDaysAsync(
            request.TenantId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        // Get SPA counts per employee
        var spas = await _spaRepository.GetByDateRangeAsync(
            request.TenantId,
            request.FromDate,
            request.ToDate,
            cancellationToken);
        var spaCountByEmployee = spas.GroupBy(s => s.EmployeeId).ToDictionary(g => g.Key, g => g.Count());

        // Group by employee and calculate performance
        var employeePerformances = summaryList
            .GroupBy(s => new { s.EmployeeId, EmployeeName = $"{s.Employee?.FirstName} {s.Employee?.LastName}".Trim() })
            .Select(g =>
            {
                var totalMinutes = g.Sum(s => s.TimeOnSiteMinutes);
                var totalHours = Math.Round(totalMinutes / 60.0m, 2);
                var expectedHours = workingDays * expectedHoursPerDay;
                var utilization = expectedHours > 0 ? Math.Round((totalHours / expectedHours) * 100, 2) : 0;

                var daysPresent = g.Count(s => s.Status != AttendanceStatus.Absent);
                var daysAbsent = workingDays - daysPresent;
                if (daysAbsent < 0) daysAbsent = 0;

                spaCountByEmployee.TryGetValue(g.Key.EmployeeId, out var spaCount);

                var status = utilization switch
                {
                    >= 90 => "Excellent",
                    >= 75 => "Good",
                    > 0 => "BelowTarget",
                    _ => "Absent"
                };

                return new EmployeePerformanceDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeName = string.IsNullOrWhiteSpace(g.Key.EmployeeName) ? "Unknown" : g.Key.EmployeeName,
                    TotalHours = totalHours,
                    ExpectedHours = expectedHours,
                    UtilizationPercent = utilization,
                    VarianceHours = totalHours - expectedHours,
                    Status = status,
                    DaysPresent = daysPresent,
                    DaysAbsent = daysAbsent,
                    SpaCount = spaCount
                };
            })
            .OrderByDescending(p => p.UtilizationPercent)
            .ToList();

        // Paginate
        var totalCount = employeePerformances.Count;
        var pagedPerformances = employeePerformances
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<EmployeePerformanceDto>(pagedPerformances, totalCount, request.Page, request.PageSize);
    }
}
