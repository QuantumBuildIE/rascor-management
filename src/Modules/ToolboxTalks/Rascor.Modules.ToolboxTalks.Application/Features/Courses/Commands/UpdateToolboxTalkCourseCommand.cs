using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Courses.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Courses.Commands;

/// <summary>
/// Command to update an existing toolbox talk course
/// </summary>
public record UpdateToolboxTalkCourseCommand : IRequest<ToolboxTalkCourseDto>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public UpdateToolboxTalkCourseDto Dto { get; init; } = null!;
}
