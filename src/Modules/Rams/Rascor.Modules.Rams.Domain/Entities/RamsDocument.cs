using Rascor.Core.Domain.Common;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Domain.Entities;

public class RamsDocument : TenantEntity
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectReference { get; set; } = string.Empty;
    public ProjectType ProjectType { get; set; }
    public string? ClientName { get; set; }
    public string? SiteAddress { get; set; }
    public string? AreaOfActivity { get; set; }
    public DateOnly? ProposedStartDate { get; set; }
    public DateOnly? ProposedEndDate { get; set; }

    // Safety officer (reference to Employee)
    public Guid? SafetyOfficerId { get; set; }

    // Approval workflow
    public RamsStatus Status { get; set; } = RamsStatus.Draft;
    public DateTime? DateApproved { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? ApprovalComments { get; set; }

    // Method statement content (rich text)
    public string? MethodStatementBody { get; set; }

    // Generated document link
    public string? GeneratedPdfUrl { get; set; }

    // Navigation properties
    public ICollection<RiskAssessment> RiskAssessments { get; set; } = new List<RiskAssessment>();
    public ICollection<MethodStep> MethodSteps { get; set; } = new List<MethodStep>();

    // Link to Proposal (optional - if RAMS created from won proposal)
    public Guid? ProposalId { get; set; }

    // Link to Site (optional)
    public Guid? SiteId { get; set; }
}
