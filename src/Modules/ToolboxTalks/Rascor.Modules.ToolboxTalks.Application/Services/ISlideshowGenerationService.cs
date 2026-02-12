using Rascor.Core.Application.Models;

namespace Rascor.Modules.ToolboxTalks.Application.Services;

/// <summary>
/// Service for generating slide images from a PDF document attached to a toolbox talk.
/// Downloads the PDF, extracts text per page, renders each page to a PNG image,
/// uploads images to R2 storage, and creates ToolboxTalkSlide records.
/// </summary>
public interface ISlideshowGenerationService
{
    /// <summary>
    /// Generates slide images and text from the PDF attached to a toolbox talk.
    /// Returns the number of slides created.
    /// </summary>
    Task<Result<int>> GenerateSlidesFromPdfAsync(
        Guid tenantId,
        Guid toolboxTalkId,
        CancellationToken ct = default);
}
