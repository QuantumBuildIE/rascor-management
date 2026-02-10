using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Courses.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Courses.Commands;

/// <summary>
/// Command to create a new toolbox talk course
/// </summary>
public record CreateToolboxTalkCourseCommand : IRequest<ToolboxTalkCourseDto>
{
    public Guid TenantId { get; init; }
    public CreateToolboxTalkCourseDto Dto { get; init; } = null!;
}
