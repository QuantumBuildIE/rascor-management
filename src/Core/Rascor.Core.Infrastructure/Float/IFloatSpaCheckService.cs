using Rascor.Core.Infrastructure.Float.Models;

namespace Rascor.Core.Infrastructure.Float;

/// <summary>
/// Service for checking Float schedules against SPA submissions and sending reminders.
/// </summary>
public interface IFloatSpaCheckService
{
    /// <summary>
    /// Run the full SPA check for a tenant - checks all scheduled employees for the specified date.
    /// </summary>
    /// <param name="tenantId">The tenant to check</param>
    /// <param name="date">The date to check (defaults to today)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result summary of the SPA check operation</returns>
    Task<SpaCheckResult> RunSpaCheckAsync(Guid tenantId, DateOnly? date = null, CancellationToken ct = default);

    /// <summary>
    /// Check if an employee has submitted SPA for a specific site and date.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="employeeId">The employee to check</param>
    /// <param name="siteId">The site to check</param>
    /// <param name="date">The date to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if SPA was submitted</returns>
    Task<bool> HasSubmittedSpaAsync(Guid tenantId, Guid employeeId, Guid siteId, DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Send an SPA reminder notification to an employee.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="employeeId">The employee to notify</param>
    /// <param name="siteId">The site they are scheduled at</param>
    /// <param name="scheduledDate">The scheduled work date</param>
    /// <param name="floatTask">Optional Float task that triggered this reminder</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The audit record for this notification</returns>
    Task<Domain.Entities.SpaNotificationAudit> SendSpaReminderAsync(
        Guid tenantId,
        Guid employeeId,
        Guid siteId,
        DateOnly scheduledDate,
        FloatTask? floatTask,
        CancellationToken ct = default);
}

/// <summary>
/// Result summary of an SPA check operation.
/// </summary>
public class SpaCheckResult
{
    /// <summary>
    /// The date that was checked.
    /// </summary>
    public DateOnly CheckDate { get; set; }

    /// <summary>
    /// Total number of Float tasks scheduled for the date.
    /// </summary>
    public int TotalScheduledTasks { get; set; }

    /// <summary>
    /// Number of unique employees checked.
    /// </summary>
    public int EmployeesChecked { get; set; }

    /// <summary>
    /// Number of employees who have already submitted SPA.
    /// </summary>
    public int SpaSubmitted { get; set; }

    /// <summary>
    /// Number of employees with missing SPA.
    /// </summary>
    public int SpaMissing { get; set; }

    /// <summary>
    /// Number of reminders successfully sent.
    /// </summary>
    public int RemindersSent { get; set; }

    /// <summary>
    /// Number of reminders that failed to send.
    /// </summary>
    public int RemindersFailed { get; set; }

    /// <summary>
    /// Number of tasks skipped because Float person is not linked to an employee.
    /// </summary>
    public int SkippedUnmatchedPeople { get; set; }

    /// <summary>
    /// Number of tasks skipped because Float project is not linked to a site.
    /// </summary>
    public int SkippedUnmatchedProjects { get; set; }

    /// <summary>
    /// Number of employees skipped because they have no email address.
    /// </summary>
    public int SkippedNoEmail { get; set; }

    /// <summary>
    /// Number of employees skipped because they were already notified today.
    /// </summary>
    public int SkippedAlreadyNotified { get; set; }

    /// <summary>
    /// Error messages encountered during processing.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Total duration of the check operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Summary string for logging.
    /// </summary>
    public override string ToString() =>
        $"SPA Check for {CheckDate:yyyy-MM-dd}: " +
        $"Tasks={TotalScheduledTasks}, Checked={EmployeesChecked}, " +
        $"Submitted={SpaSubmitted}, Missing={SpaMissing}, " +
        $"RemindersSent={RemindersSent}, Failed={RemindersFailed}, " +
        $"Duration={Duration.TotalSeconds:F1}s";
}
