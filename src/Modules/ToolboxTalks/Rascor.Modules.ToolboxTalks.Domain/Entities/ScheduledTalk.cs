using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Domain.Entities;

/// <summary>
/// Represents an individual toolbox talk assignment to a specific employee.
/// Created from schedules or manually assigned.
/// </summary>
public class ScheduledTalk : TenantEntity
{
    /// <summary>
    /// The toolbox talk to be completed
    /// </summary>
    public Guid ToolboxTalkId { get; set; }

    /// <summary>
    /// The employee assigned to complete this talk
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// The schedule that created this assignment (null if manually assigned)
    /// </summary>
    public Guid? ScheduleId { get; set; }

    /// <summary>
    /// Date when the talk was assigned/required
    /// </summary>
    public DateTime RequiredDate { get; set; }

    /// <summary>
    /// Deadline for completing the talk
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Current status of this assignment
    /// </summary>
    public ScheduledTalkStatus Status { get; set; } = ScheduledTalkStatus.Pending;

    /// <summary>
    /// Number of reminder notifications sent
    /// </summary>
    public int RemindersSent { get; set; } = 0;

    /// <summary>
    /// When the last reminder was sent
    /// </summary>
    public DateTime? LastReminderAt { get; set; }

    /// <summary>
    /// Language code for the employee's preferred language (e.g., "en", "es", "pl")
    /// </summary>
    public string LanguageCode { get; set; } = "en";

    /// <summary>
    /// Current video watch progress percentage (0-100)
    /// Updated as employee watches the video
    /// </summary>
    public int VideoWatchPercent { get; set; } = 0;

    // Navigation properties

    /// <summary>
    /// The toolbox talk to be completed
    /// </summary>
    public ToolboxTalk ToolboxTalk { get; set; } = null!;

    /// <summary>
    /// The assigned employee
    /// </summary>
    public Employee Employee { get; set; } = null!;

    /// <summary>
    /// The schedule that created this assignment (if any)
    /// </summary>
    public ToolboxTalkSchedule? Schedule { get; set; }

    /// <summary>
    /// Progress tracking for each section
    /// </summary>
    public ICollection<ScheduledTalkSectionProgress> SectionProgress { get; set; } = new List<ScheduledTalkSectionProgress>();

    /// <summary>
    /// Quiz attempts for this assignment
    /// </summary>
    public ICollection<ScheduledTalkQuizAttempt> QuizAttempts { get; set; } = new List<ScheduledTalkQuizAttempt>();

    /// <summary>
    /// Completion record when talk is finished
    /// </summary>
    public ScheduledTalkCompletion? Completion { get; set; }
}
