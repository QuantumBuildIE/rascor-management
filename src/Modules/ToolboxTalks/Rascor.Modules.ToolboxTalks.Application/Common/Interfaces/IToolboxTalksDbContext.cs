using Microsoft.EntityFrameworkCore;
using Rascor.Modules.ToolboxTalks.Domain.Entities;

namespace Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;

/// <summary>
/// Database context interface for the Toolbox Talks module
/// </summary>
public interface IToolboxTalksDbContext
{
    DbSet<ToolboxTalk> ToolboxTalks { get; }
    DbSet<ToolboxTalkSection> ToolboxTalkSections { get; }
    DbSet<ToolboxTalkQuestion> ToolboxTalkQuestions { get; }
    DbSet<ToolboxTalkTranslation> ToolboxTalkTranslations { get; }
    DbSet<ToolboxTalkVideoTranslation> ToolboxTalkVideoTranslations { get; }
    DbSet<ToolboxTalkSchedule> ToolboxTalkSchedules { get; }
    DbSet<ToolboxTalkScheduleAssignment> ToolboxTalkScheduleAssignments { get; }
    DbSet<ScheduledTalk> ScheduledTalks { get; }
    DbSet<ScheduledTalkSectionProgress> ScheduledTalkSectionProgress { get; }
    DbSet<ScheduledTalkQuizAttempt> ScheduledTalkQuizAttempts { get; }
    DbSet<ScheduledTalkCompletion> ScheduledTalkCompletions { get; }
    DbSet<ToolboxTalkSettings> ToolboxTalkSettings { get; }

    // Subtitle processing entities
    DbSet<SubtitleProcessingJob> SubtitleProcessingJobs { get; }
    DbSet<SubtitleTranslation> SubtitleTranslations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
