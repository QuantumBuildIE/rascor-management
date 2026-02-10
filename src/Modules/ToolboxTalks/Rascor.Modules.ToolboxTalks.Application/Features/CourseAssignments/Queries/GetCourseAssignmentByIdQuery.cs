using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Queries;

public record GetCourseAssignmentByIdQuery : IRequest<ToolboxTalkCourseAssignmentDto?>
{
    public Guid TenantId { get; init; }
    public Guid Id { get; init; }
}
