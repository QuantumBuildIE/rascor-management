using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Application.Commands.RecordAttendanceEvent;

public record RecordAttendanceEventCommand : IRequest<AttendanceEventDto>
{
    public Guid TenantId { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid SiteId { get; init; }
    public EventType EventType { get; init; }
    public DateTime Timestamp { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public TriggerMethod TriggerMethod { get; init; }
    public string? DeviceIdentifier { get; init; }
    public Guid? UserId { get; init; }
}
