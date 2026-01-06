using MediatR;

namespace Rascor.Modules.SiteAttendance.Application.Commands.ProcessDailyAttendance;

/// <summary>
/// Command for background job to process daily attendance events and create summaries
/// </summary>
public record ProcessDailyAttendanceCommand : IRequest<ProcessDailyAttendanceResult>
{
    public Guid TenantId { get; init; }
    public DateOnly Date { get; init; }
}

public record ProcessDailyAttendanceResult
{
    public int EventsProcessed { get; init; }
    public int SummariesCreated { get; init; }
    public int SummariesUpdated { get; init; }
    public List<string> Errors { get; init; } = new();
}
