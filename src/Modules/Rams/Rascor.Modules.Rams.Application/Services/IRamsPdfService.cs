namespace Rascor.Modules.Rams.Application.Services;

/// <summary>
/// Service for generating PDF documents from RAMS documents
/// </summary>
public interface IRamsPdfService
{
    /// <summary>
    /// Generates a PDF document for a RAMS document
    /// </summary>
    /// <param name="ramsDocumentId">The ID of the RAMS document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF document as byte array</returns>
    Task<byte[]> GeneratePdfAsync(Guid ramsDocumentId, CancellationToken cancellationToken = default);
}
