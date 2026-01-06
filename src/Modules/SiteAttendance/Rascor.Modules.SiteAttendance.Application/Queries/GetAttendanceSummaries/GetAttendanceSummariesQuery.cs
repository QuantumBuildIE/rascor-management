using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Enums;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetAttendanceSummaries;

public record GetAttendanceSummariesQuery : IRequest<PaginatedList<AttendanceSummaryDto>>
{
    public Guid TenantId { get; init; }
    public Guid? EmployeeId { get; init; }
    public Guid? SiteId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public AttendanceStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
