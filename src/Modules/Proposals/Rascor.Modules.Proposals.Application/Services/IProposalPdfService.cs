namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for generating PDF documents from proposals
/// </summary>
public interface IProposalPdfService
{
    /// <summary>
    /// Generates a PDF document for a proposal
    /// </summary>
    /// <param name="proposalId">The ID of the proposal</param>
    /// <param name="includeCosting">Whether to include internal costing information (cost, margin)</param>
    /// <returns>PDF document as byte array</returns>
    Task<byte[]> GeneratePdfAsync(Guid proposalId, bool includeCosting = false);
}
