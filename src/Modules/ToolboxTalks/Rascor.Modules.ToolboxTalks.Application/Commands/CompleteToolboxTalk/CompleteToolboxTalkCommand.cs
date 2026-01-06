using MediatR;
using Rascor.Modules.ToolboxTalks.Application.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Commands.CompleteToolboxTalk;

/// <summary>
/// Command to complete a scheduled toolbox talk.
/// Requires signature and validates all completion requirements.
/// </summary>
public record CompleteToolboxTalkCommand : IRequest<ScheduledTalkCompletionDto>
{
    /// <summary>
    /// The scheduled talk to complete
    /// </summary>
    public Guid ScheduledTalkId { get; init; }

    /// <summary>
    /// Base64 encoded signature image (PNG)
    /// </summary>
    public string SignatureData { get; init; } = string.Empty;

    /// <summary>
    /// Name entered by the employee when signing
    /// </summary>
    public string SignedByName { get; init; } = string.Empty;
}
