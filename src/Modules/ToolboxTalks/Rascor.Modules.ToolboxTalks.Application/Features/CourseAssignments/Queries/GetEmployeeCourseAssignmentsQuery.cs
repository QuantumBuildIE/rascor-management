using MediatR;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Queries;

public record GetEmployeeCourseAssignmentsQuery : IRequest<List<ToolboxTalkCourseAssignmentDto>>
{
    public Guid TenantId { get; init; }
    public Guid EmployeeId { get; init; }
}
