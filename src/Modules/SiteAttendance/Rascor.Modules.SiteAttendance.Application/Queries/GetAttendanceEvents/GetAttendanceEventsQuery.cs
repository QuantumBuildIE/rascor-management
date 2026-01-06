using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceEvents;

public record GetAttendanceEventsQuery : IRequest<PaginatedList<AttendanceEventDto>>
{
    public Guid TenantId { get; init; }
    public Guid? EmployeeId { get; init; }
    public Guid? SiteId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public EventType? EventType { get; init; }
    public bool? IncludeNoise { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
