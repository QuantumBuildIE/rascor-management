using MediatR;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetDashboardKpis;

public record GetDashboardKpisQuery : IRequest<DashboardKpisDto>
{
    public Guid TenantId { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public Guid? SiteId { get; init; }
}
