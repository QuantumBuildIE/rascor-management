using MediatR;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Domain.Enums;

namespace Rascor.Modules.ToolboxTalks.Application.Commands.SendScheduledTalkReminder;

public class SendScheduledTalkReminderCommandHandler : IRequestHandler<SendScheduledTalkReminderCommand, bool>
{
    private readonly IToolboxTalksDbContext _dbContext;
    private readonly IToolboxTalkEmailService _emailService;

    public SendScheduledTalkReminderCommandHandler(
        IToolboxTalksDbContext dbContext,
        IToolboxTalkEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task<bool> Handle(SendScheduledTalkReminderCommand request, CancellationToken cancellationToken)
    {
        var scheduledTalk = await _dbContext.ScheduledTalks
            .Include(st => st.Employee)
            .Include(st => st.ToolboxTalk)
            .FirstOrDefaultAsync(st => st.Id == request.Id && st.TenantId == request.TenantId, cancellationToken);

        if (scheduledTalk == null)
        {
            throw new KeyNotFoundException($"Scheduled talk with ID '{request.Id}' not found.");
        }

        if (scheduledTalk.Status == ScheduledTalkStatus.Completed)
        {
            throw new InvalidOperationException("Cannot send a reminder for a completed assignment.");
        }

        if (scheduledTalk.Status == ScheduledTalkStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot send a reminder for a cancelled assignment.");
        }

        scheduledTalk.RemindersSent++;
        scheduledTalk.LastReminderAt = DateTime.UtcNow;

        await _emailService.SendReminderEmailAsync(
            scheduledTalk,
            scheduledTalk.Employee,
            scheduledTalk.RemindersSent,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
