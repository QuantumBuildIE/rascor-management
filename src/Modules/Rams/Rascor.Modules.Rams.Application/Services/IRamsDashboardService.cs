using Rascor.Modules.Rams.Application.DTOs;

namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service for RAMS dashboard statistics and reporting
/// </summary>
public interface IRamsDashboardService
{
    /// <summary>
    /// Gets complete dashboard data including summary stats, chart data, and lists
    /// </summary>
    Task<RamsDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents pending approval
    /// </summary>
    Task<List<RamsPendingApprovalDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents that are overdue (past proposed end date and not approved)
    /// </summary>
    Task<List<RamsOverdueDocumentDto>> GetOverdueDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports RAMS data to Excel based on the provided filters
    /// </summary>
    Task<byte[]> ExportToExcelAsync(RamsExportRequestDto request, CancellationToken cancellationToken = default);
}
