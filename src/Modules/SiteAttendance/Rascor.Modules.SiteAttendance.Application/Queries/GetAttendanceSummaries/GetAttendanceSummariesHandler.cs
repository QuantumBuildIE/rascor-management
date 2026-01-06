using AutoMapper;
using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceSummaries;

public class GetAttendanceSummariesHandler : IRequestHandler<GetAttendanceSummariesQuery, PaginatedList<AttendanceSummaryDto>>
{
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly IMapper _mapper;

    public GetAttendanceSummariesHandler(
        IAttendanceSummaryRepository summaryRepository,
        IMapper mapper)
    {
        _summaryRepository = summaryRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AttendanceSummaryDto>> Handle(GetAttendanceSummariesQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        IEnumerable<Domain.Entities.AttendanceSummary> summaries;

        if (request.EmployeeId.HasValue)
        {
            summaries = await _summaryRepository.GetByEmployeeAsync(
                request.TenantId,
                request.EmployeeId.Value,
                fromDate,
                toDate,
                cancellationToken);
        }
        else if (request.SiteId.HasValue)
        {
            summaries = await _summaryRepository.GetBySiteAsync(
                request.TenantId,
                request.SiteId.Value,
                fromDate,
                toDate,
                cancellationToken);
        }
        else
        {
            summaries = await _summaryRepository.GetByDateRangeAsync(
                request.TenantId,
                fromDate,
                toDate,
                cancellationToken);
        }

        var summaryList = summaries.ToList();

        // Apply additional filters
        if (request.SiteId.HasValue && request.EmployeeId.HasValue)
        {
            summaryList = summaryList.Where(s => s.SiteId == request.SiteId.Value).ToList();
        }

        if (request.Status.HasValue)
        {
            summaryList = summaryList.Where(s => s.Status == request.Status.Value).ToList();
        }

        // Order by date descending, then by employee name
        summaryList = summaryList
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.Employee?.FirstName)
            .ToList();

        // Paginate
        var totalCount = summaryList.Count;
        var pagedSummaries = summaryList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<AttendanceSummaryDto>>(pagedSummaries);

        return new PaginatedList<AttendanceSummaryDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
