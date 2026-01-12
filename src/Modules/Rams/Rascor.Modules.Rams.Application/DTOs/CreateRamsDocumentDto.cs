using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record CreateRamsDocumentDto
{
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectReference { get; init; } = string.Empty;
    public ProjectType ProjectType { get; init; }
    public string? ClientName { get; init; }
    public string? SiteAddress { get; init; }
    public string? AreaOfActivity { get; init; }
    public DateOnly? ProposedStartDate { get; init; }
    public DateOnly? ProposedEndDate { get; init; }
    public Guid? SafetyOfficerId { get; init; }
    public string? MethodStatementBody { get; init; }
    public Guid? ProposalId { get; init; }
    public Guid? SiteId { get; init; }
}
