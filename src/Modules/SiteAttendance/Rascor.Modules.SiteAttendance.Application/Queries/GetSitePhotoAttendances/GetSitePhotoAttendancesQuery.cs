using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetSitePhotoAttendances;

public record GetSitePhotoAttendancesQuery : IRequest<PaginatedList<SitePhotoAttendanceDto>>
{
    public Guid TenantId { get; init; }
    public Guid? EmployeeId { get; init; }
    public Guid? SiteId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
