using AutoMapper;
using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Interfaces;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceEventById;

public class GetAttendanceEventByIdHandler : IRequestHandler<GetAttendanceEventByIdQuery, AttendanceEventDto?>
{
    private readonly IAttendanceEventRepository _eventRepository;
    private readonly IMapper _mapper;

    public GetAttendanceEventByIdHandler(
        IAttendanceEventRepository eventRepository,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<AttendanceEventDto?> Handle(GetAttendanceEventByIdQuery request, CancellationToken cancellationToken)
    {
        var attendanceEvent = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);

        if (attendanceEvent == null || attendanceEvent.TenantId != request.TenantId)
        {
            return null;
        }

        return _mapper.Map<AttendanceEventDto>(attendanceEvent);
    }
}
