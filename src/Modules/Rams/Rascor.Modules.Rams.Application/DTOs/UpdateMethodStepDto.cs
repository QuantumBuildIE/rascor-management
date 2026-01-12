namespace Rascor.Modules.Rams.Application.DTOs;

public record UpdateMethodStepDto
{
    public int? StepNumber { get; init; }
    public string StepTitle { get; init; } = string.Empty;
    public string? DetailedProcedure { get; init; }
    public Guid? LinkedRiskAssessmentId { get; init; }
    public string? RequiredPermits { get; init; }
    public bool RequiresSignoff { get; init; }
}
