using MediatR;
using Rascor.Core.Application.Models;
using Rascor.Modules.SiteAttendance.Application.DTOs;

namespace Rascor.Modules.SiteAttendance.Application.Queries.GetEmployeePerformance;

public record GetEmployeePerformanceQuery : IRequest<PaginatedList<EmployeePerformanceDto>>
{
    public Guid TenantId { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public Guid? SiteId { get; init; }
    public Guid? EmployeeId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
