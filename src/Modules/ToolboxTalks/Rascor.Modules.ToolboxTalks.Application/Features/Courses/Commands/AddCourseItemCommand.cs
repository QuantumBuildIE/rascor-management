using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Courses.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Courses.Commands;

/// <summary>
/// Command to add a talk to a course
/// </summary>
public record AddCourseItemCommand : IRequest<ToolboxTalkCourseDto>
{
    public Guid CourseId { get; init; }
    public Guid TenantId { get; init; }
    public CreateToolboxTalkCourseItemDto Dto { get; init; } = null!;
}
