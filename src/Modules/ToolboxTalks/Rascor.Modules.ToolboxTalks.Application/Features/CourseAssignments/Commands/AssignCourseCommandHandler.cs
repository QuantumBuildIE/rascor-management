using MediatR;
using Microsoft.EntityFrameworkCore;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.DTOs;
using Rascor.Modules.ToolboxTalks.Domain.Entities;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.Features.CourseAssignments.Commands;

public class AssignCourseCommandHandler : IRequestHandler<AssignCourseCommand, List<ToolboxTalkCourseAssignmentDto>>
{
    private readonly IToolboxTalksDbContext _dbContext;
    private readonly ICoreDbContext _coreDbContext;

    public AssignCourseCommandHandler(IToolboxTalksDbContext dbContext, ICoreDbContext coreDbContext)
    {
        _dbContext = dbContext;
        _coreDbContext = coreDbContext;
    }

    public async Task<List<ToolboxTalkCourseAssignmentDto>> Handle(AssignCourseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;
        var dto = request.Dto;

        // 1. Get course with items
        var course = await _dbContext.ToolboxTalkCourses
            .Include(c => c.CourseItems.Where(ci => !ci.IsDeleted))
                .ThenInclude(ci => ci.ToolboxTalk)
            .FirstOrDefaultAsync(c => c.Id == dto.CourseId && c.TenantId == tenantId && !c.IsDeleted, cancellationToken);

        if (course == null)
            throw new KeyNotFoundException("Course not found");

        if (!course.IsActive)
            throw new InvalidOperationException("Course is not active");

        var courseItems = course.CourseItems
            .Where(ci => ci.ToolboxTalk != null && !ci.ToolboxTalk.IsDeleted)
            .OrderBy(ci => ci.OrderIndex)
            .ToList();

        if (!courseItems.Any())
            throw new InvalidOperationException("Course has no talks");

        // 2. Validate employees exist
        var employees = await _coreDbContext.Employees
            .Where(e => dto.EmployeeIds.Contains(e.Id) && e.TenantId == tenantId && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        if (employees.Count != dto.EmployeeIds.Distinct().Count())
            throw new KeyNotFoundException("One or more employees not found");

        // 3. Check for existing active assignments - skip employees who already have one
        var existingEmployeeIds = await _dbContext.ToolboxTalkCourseAssignments
            .Where(a => a.CourseId == dto.CourseId
                && dto.EmployeeIds.Contains(a.EmployeeId)
                && a.Status != CourseAssignmentStatus.Completed
                && !a.IsDeleted)
            .Select(a => a.EmployeeId)
            .ToListAsync(cancellationToken);

        var eligibleEmployees = employees.Where(e => !existingEmployeeIds.Contains(e.Id)).ToList();

        if (!eligibleEmployees.Any())
            throw new InvalidOperationException("All selected employees already have active assignments for this course");

        // 4. Create assignments
        var results = new List<ToolboxTalkCourseAssignmentDto>();

        foreach (var employee in eligibleEmployees)
        {
            var assignment = new ToolboxTalkCourseAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CourseId = course.Id,
                EmployeeId = employee.Id,
                AssignedAt = DateTime.UtcNow,
                DueDate = dto.DueDate.HasValue
                    ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                    : null,
                Status = CourseAssignmentStatus.Assigned,
            };

            var scheduledTalkDtos = new List<CourseScheduledTalkDto>();

            foreach (var item in courseItems)
            {
                var scheduledTalk = new ScheduledTalk
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ToolboxTalkId = item.ToolboxTalkId,
                    EmployeeId = employee.Id,
                    RequiredDate = DateTime.UtcNow,
                    DueDate = dto.DueDate.HasValue
                        ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                        : DateTime.UtcNow.AddDays(30),
                    Status = ScheduledTalkStatus.Pending,
                    CourseAssignmentId = assignment.Id,
                    CourseOrderIndex = item.OrderIndex,
                };

                _dbContext.ScheduledTalks.Add(scheduledTalk);

                scheduledTalkDtos.Add(new CourseScheduledTalkDto
                {
                    ScheduledTalkId = scheduledTalk.Id,
                    ToolboxTalkId = item.ToolboxTalkId,
                    TalkTitle = item.ToolboxTalk.Title,
                    OrderIndex = item.OrderIndex,
                    IsRequired = item.IsRequired,
                    Status = ScheduledTalkStatus.Pending.ToString(),
                    CompletedAt = null,
                    IsLocked = false,
                    LockedReason = null,
                });
            }

            _dbContext.ToolboxTalkCourseAssignments.Add(assignment);

            results.Add(new ToolboxTalkCourseAssignmentDto
            {
                Id = assignment.Id,
                CourseId = course.Id,
                CourseTitle = course.Title,
                CourseDescription = course.Description,
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmployeeCode = employee.EmployeeCode,
                AssignedAt = assignment.AssignedAt,
                DueDate = assignment.DueDate,
                StartedAt = null,
                CompletedAt = null,
                Status = CourseAssignmentStatus.Assigned.ToString(),
                IsRefresher = false,
                RefresherDueDate = null,
                TotalTalks = courseItems.Count,
                CompletedTalks = 0,
                ScheduledTalks = scheduledTalkDtos,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return results;
    }
}
