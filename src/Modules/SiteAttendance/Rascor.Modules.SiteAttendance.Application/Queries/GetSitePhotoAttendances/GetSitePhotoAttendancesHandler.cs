using AutoMapper;
using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetSitePhotoAttendances;

public class GetSitePhotoAttendancesHandler : IRequestHandler<GetSitePhotoAttendancesQuery, PaginatedList<SitePhotoAttendanceDto>>
{
    private readonly ISitePhotoAttendanceRepository _spaRepository;
    private readonly IMapper _mapper;

    public GetSitePhotoAttendancesHandler(
        ISitePhotoAttendanceRepository spaRepository,
        IMapper mapper)
    {
        _spaRepository = spaRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SitePhotoAttendanceDto>> Handle(GetSitePhotoAttendancesQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        IEnumerable<Domain.Entities.SitePhotoAttendance> spas;

        if (request.EmployeeId.HasValue)
        {
            spas = await _spaRepository.GetByEmployeeAsync(
                request.TenantId,
                request.EmployeeId.Value,
                fromDate,
                toDate,
                cancellationToken);
        }
        else
        {
            spas = await _spaRepository.GetByDateRangeAsync(
                request.TenantId,
                fromDate,
                toDate,
                cancellationToken);
        }

        var spaList = spas.ToList();

        // Apply additional filters
        if (request.SiteId.HasValue)
        {
            spaList = spaList.Where(s => s.SiteId == request.SiteId.Value).ToList();
        }

        // Order by date descending
        spaList = spaList.OrderByDescending(s => s.EventDate).ThenByDescending(s => s.CreatedAt).ToList();

        // Paginate
        var totalCount = spaList.Count;
        var pagedSpas = spaList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<SitePhotoAttendanceDto>>(pagedSpas);

        return new PaginatedList<SitePhotoAttendanceDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
