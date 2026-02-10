using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Commands;

public record AssignCourseCommand : IRequest<List<ToolboxTalkCourseAssignmentDto>>
{
    public Guid TenantId { get; init; }
    public AssignCourseDto Dto { get; init; } = null!;
}
