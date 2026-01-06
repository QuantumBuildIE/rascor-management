using AutoMapper;
using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceEvents;

public class GetAttendanceEventsHandler : IRequestHandler<GetAttendanceEventsQuery, PaginatedList<AttendanceEventDto>>
{
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly IMapper _mapper;

    public GetAttendanceEventsHandler(
        IAttendanceEventRepository eventRepository,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AttendanceEventDto>> Handle(GetAttendanceEventsQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        IEnumerable<Domain.Entities.AttendanceEvent> events;

        if (request.EmployeeId.HasValue)
        {
            events = await _eventRepository.GetByEmployeeAsync(
                request.TenantId,
                request.EmployeeId.Value,
                fromDate,
                toDate,
                cancellationToken);
        }
        else if (request.SiteId.HasValue)
        {
            events = await _eventRepository.GetBySiteAsync(
                request.TenantId,
                request.SiteId.Value,
                fromDate,
                toDate,
                cancellationToken);
        }
        else
        {
            events = await _eventRepository.GetByDateRangeAsync(
                request.TenantId,
                fromDate,
                toDate,
                cancellationToken);
        }

        var eventsList = events.ToList();

        // Apply additional filters
        if (request.SiteId.HasValue && request.EmployeeId.HasValue)
        {
            eventsList = eventsList.Where(e => e.SiteId == request.SiteId.Value).ToList();
        }

        if (request.EventType.HasValue)
        {
            eventsList = eventsList.Where(e => e.EventType == request.EventType.Value).ToList();
        }

        if (request.IncludeNoise.HasValue && !request.IncludeNoise.Value)
        {
            eventsList = eventsList.Where(e => !e.IsNoise).ToList();
        }

        // Order by timestamp descending
        eventsList = eventsList.OrderByDescending(e => e.Timestamp).ToList();

        // Paginate
        var totalCount = eventsList.Count;
        var pagedEvents = eventsList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<AttendanceEventDto>>(pagedEvents);

        return new PaginatedList<AttendanceEventDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
