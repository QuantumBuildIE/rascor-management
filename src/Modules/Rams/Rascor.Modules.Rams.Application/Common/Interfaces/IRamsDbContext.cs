using Microsoft.EntityFrameworkCore;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Application.Common.Interfaces;

/// <summary>
/// Database context interface for the RAMS module
/// </summary>
public interface IRamsDbContext
{
    DbSet<RamsDocument> RamsDocuments { get; }
    DbSet<RiskAssessment> RamsRiskAssessments { get; }
    DbSet<MethodStep> RamsMethodSteps { get; }
    DbSet<HazardLibrary> RamsHazardLibrary { get; }
    DbSet<ControlMeasureLibrary> RamsControlMeasureLibrary { get; }
    DbSet<LegislationReference> RamsLegislationReferences { get; }
    DbSet<SopReference> RamsSopReferences { get; }
    DbSet<HazardControlLink> RamsHazardControlLinks { get; }
    DbSet<McpAuditLog> RamsMcpAuditLogs { get; }
    DbSet<RamsNotificationLog> RamsNotificationLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
