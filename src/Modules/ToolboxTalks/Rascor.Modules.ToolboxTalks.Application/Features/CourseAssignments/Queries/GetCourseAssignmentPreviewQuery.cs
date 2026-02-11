using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Queries;

public record GetCourseAssignmentPreviewQuery : IRequest<CourseAssignmentPreviewDto?>
{
    public Guid TenantId { get; init; }
    public Guid CourseId { get; init; }
    public List<Guid> EmployeeIds { get; init; } = new();
}
