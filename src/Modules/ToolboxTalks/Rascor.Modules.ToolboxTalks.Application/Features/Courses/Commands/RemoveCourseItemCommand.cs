using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Courses.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Courses.Commands;

/// <summary>
/// Command to remove a talk from a course
/// </summary>
public record RemoveCourseItemCommand : IRequest<ToolboxTalkCourseDto>
{
    public Guid CourseId { get; init; }
    public Guid ToolboxTalkId { get; init; }
    public Guid TenantId { get; init; }
}
