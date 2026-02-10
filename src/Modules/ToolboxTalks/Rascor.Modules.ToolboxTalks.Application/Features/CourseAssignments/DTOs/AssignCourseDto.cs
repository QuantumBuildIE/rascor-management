using System.ComponentModel.DataAnnotations;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;

public record AssignCourseDto
{
    [Required]
    public Guid CourseId { get; init; }

    [Required]
    [MinLength(1)]
    public List<Guid> EmployeeIds { get; init; } = new();

    public DateTime? DueDate { get; init; }
}
