using MediatR;
using Rascor.Modules.ToolboxTalks.Application.DTOs;

namespace Rascor.Modules.ToolboxTalks.Application.Queries.GetToolboxTalkById;

/// <summary>
/// Query to retrieve a single toolbox talk by ID with full details
/// </summary>
public record GetToolboxTalkByIdQuery : IRequest<ToolboxTalkDto?>
{
    /// <summary>
    /// Tenant ID for multi-tenancy filtering
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// The ID of the toolbox talk to retrieve
    /// </summary>
    public Guid Id { get; init; }
}
