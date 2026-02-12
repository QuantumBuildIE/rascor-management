using MediatR;
using Rascor.Modules.ToolboxTalks.Application.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalkSlides;

/// <summary>
/// Query to retrieve slides for a toolbox talk with optional translated text
/// </summary>
public record GetToolboxTalkSlidesQuery : IRequest<List<SlideDto>>
{
    /// <summary>
    /// Tenant ID for multi-tenancy filtering
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// The toolbox talk ID to retrieve slides for
    /// </summary>
    public Guid ToolboxTalkId { get; init; }

    /// <summary>
    /// Optional language code (ISO 639-1) for translated text.
    /// If null or "en", returns the original extracted text.
    /// </summary>
    public string? LanguageCode { get; init; }
}
