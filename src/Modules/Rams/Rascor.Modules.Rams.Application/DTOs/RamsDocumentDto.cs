using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record RamsDocumentDto
{
    public Guid Id { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectReference { get; init; } = string.Empty;
    public ProjectType ProjectType { get; init; }
    public string ProjectTypeDisplay => ProjectType.ToString();
    public string? ClientName { get; init; }
    public string? SiteAddress { get; init; }
    public string? AreaOfActivity { get; init; }
    public DateOnly? ProposedStartDate { get; init; }
    public DateOnly? ProposedEndDate { get; init; }
    public Guid? SafetyOfficerId { get; init; }
    public string? SafetyOfficerName { get; init; }
    public RamsStatus Status { get; init; }
    public string StatusDisplay => Status.ToString();
    public DateTime? DateApproved { get; init; }
    public Guid? ApprovedById { get; init; }
    public string? ApprovedByName { get; init; }
    public string? ApprovalComments { get; init; }
    public string? MethodStatementBody { get; init; }
    public string? GeneratedPdfUrl { get; init; }
    public Guid? ProposalId { get; init; }
    public Guid? SiteId { get; init; }
    public string? SiteName { get; init; }
    public int RiskAssessmentCount { get; init; }
    public int MethodStepCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}
