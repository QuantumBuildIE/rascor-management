using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Queries;

public record GetCourseAssignmentsQuery : IRequest<List<CourseAssignmentListDto>>
{
    public Guid TenantId { get; init; }
    public Guid CourseId { get; init; }
}
