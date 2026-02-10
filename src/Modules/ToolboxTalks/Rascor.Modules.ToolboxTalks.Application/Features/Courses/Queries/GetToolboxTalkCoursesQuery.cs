using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.Courses.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.Courses.Queries;

/// <summary>
/// Query to retrieve all courses for a tenant
/// </summary>
public record GetToolboxTalkCoursesQuery : IRequest<List<ToolboxTalkCourseListDto>>
{
    public Guid TenantId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
}
