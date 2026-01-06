using MediatR;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.DTOs;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalkScheduleById;

public class GetToolboxTalkScheduleByIdQueryHandler : IRequestHandler<GetToolboxTalkScheduleByIdQuery, ToolboxTalkScheduleDto?>
{
    private readonly IToolboxTalksDbContext _context;

    public GetToolboxTalkScheduleByIdQueryHandler(IToolboxTalksDbContext context)
    {
        _context = context;
    }

    public async Task<ToolboxTalkScheduleDto?> Handle(GetToolboxTalkScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _context.ToolboxTalkSchedules
            .Include(s => s.ToolboxTalk)
            .Include(s => s.Assignments)
            .Where(s => s.Id == request.Id && s.TenantId == request.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (schedule == null)
            return null;

        // Get employee information for assignments
        var employeeIds = schedule.Assignments.Select(a => a.EmployeeId).ToList();

        // Note: In a real implementation, we would query the employees from Core module
        // For now, we'll include a placeholder for employee names
        // This would typically be done via a cross-module service or repository

        return new ToolboxTalkScheduleDto
        {
            Id = schedule.Id,
            ToolboxTalkId = schedule.ToolboxTalkId,
            ToolboxTalkTitle = schedule.ToolboxTalk.Title,
            ScheduledDate = schedule.ScheduledDate,
            EndDate = schedule.EndDate,
            Frequency = schedule.Frequency,
            FrequencyDisplay = GetFrequencyDisplay(schedule.Frequency),
            AssignToAllEmployees = schedule.AssignToAllEmployees,
            Status = schedule.Status,
            StatusDisplay = GetStatusDisplay(schedule.Status),
            NextRunDate = schedule.NextRunDate,
            Notes = schedule.Notes,
            AssignmentCount = schedule.Assignments.Count,
            ProcessedCount = schedule.Assignments.Count(a => a.IsProcessed),
            Assignments = schedule.Assignments.Select(a => new ToolboxTalkScheduleAssignmentDto
            {
                Id = a.Id,
                ScheduleId = a.ScheduleId,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"Employee {a.EmployeeId}", // Placeholder - would be resolved via employee service
                EmployeeEmail = null, // Placeholder - would be resolved via employee service
                IsProcessed = a.IsProcessed,
                ProcessedAt = a.ProcessedAt
            }).ToList(),
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }

    private static string GetFrequencyDisplay(ToolboxTalkFrequency frequency) => frequency switch
    {
        ToolboxTalkFrequency.Once => "One-time",
        ToolboxTalkFrequency.Weekly => "Weekly",
        ToolboxTalkFrequency.Monthly => "Monthly",
        ToolboxTalkFrequency.Annually => "Annually",
        _ => frequency.ToString()
    };

    private static string GetStatusDisplay(ToolboxTalkScheduleStatus status) => status switch
    {
        ToolboxTalkScheduleStatus.Draft => "Draft",
        ToolboxTalkScheduleStatus.Active => "Active",
        ToolboxTalkScheduleStatus.Completed => "Completed",
        ToolboxTalkScheduleStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };
}
