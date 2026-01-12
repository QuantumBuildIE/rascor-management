using Rascor.Core.Domain.Common;

namespace Rascor.Modules.Rams.Domain.Entities;

public class MethodStep : TenantEntity
{
    public Guid RamsDocumentId { get; set; }
    public RamsDocument RamsDocument { get; set; } = null!;

    public int StepNumber { get; set; }
    public string StepTitle { get; set; } = string.Empty;
    public string? DetailedProcedure { get; set; }

    // Link to related risk assessment (optional)
    public Guid? LinkedRiskAssessmentId { get; set; }
    public RiskAssessment? LinkedRiskAssessment { get; set; }

    // Permits required for this step
    public string? RequiredPermits { get; set; }

    // Sign-off tracking
    public bool RequiresSignoff { get; set; }
    public string? SignoffUrl { get; set; }
}
