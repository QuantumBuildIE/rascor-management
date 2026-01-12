namespace Rascor.Modules.Rams.Application.DTOs;

public record MethodStepDto
{
    public Guid Id { get; init; }
    public Guid RamsDocumentId { get; init; }
    public int StepNumber { get; init; }
    public string StepTitle { get; init; } = string.Empty;
    public string? DetailedProcedure { get; init; }
    public Guid? LinkedRiskAssessmentId { get; init; }
    public string? LinkedRiskAssessmentTask { get; init; }
    public string? RequiredPermits { get; init; }
    public bool RequiresSignoff { get; init; }
    public string? SignoffUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}
