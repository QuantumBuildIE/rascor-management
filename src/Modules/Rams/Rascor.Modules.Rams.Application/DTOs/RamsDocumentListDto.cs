using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Application.DTOs;

public record RamsDocumentListDto
{
    public Guid Id { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectReference { get; init; } = string.Empty;
    public ProjectType ProjectType { get; init; }
    public string ProjectTypeDisplay => ProjectType.ToString();
    public string? ClientName { get; init; }
    public RamsStatus Status { get; init; }
    public string StatusDisplay => Status.ToString();
    public DateOnly? ProposedStartDate { get; init; }
    public string? SiteName { get; init; }
    public int RiskAssessmentCount { get; init; }
    public int MethodStepCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
