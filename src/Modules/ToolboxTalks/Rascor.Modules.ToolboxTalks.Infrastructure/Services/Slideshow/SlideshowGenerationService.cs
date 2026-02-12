using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PDFtoImage;
using Rascor.Core.Application.Models;
using Rascor.Modules.ToolboxTalks.Application.Abstractions.Storage;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Services;
using Rascor.Modules.ToolboxTalks.Domain.Entities;
using UglyToad.PdfPig;

namespace Rascor.Modules.ToolboxTalks.Infrastructure.Services.Slideshow;

/// <summary>
/// Service that generates slide images from PDF documents attached to toolbox talks.
/// Uses PdfPig for text extraction and PDFtoImage (PDFium) for rendering pages to PNG.
/// </summary>
public class SlideshowGenerationService : ISlideshowGenerationService
{
    private readonly IToolboxTalksDbContext _context;
    private readonly IR2StorageService _r2Storage;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlideshowGenerationService> _logger;

    private const int MaxPages = 50;
    private const int RenderDpi = 150;
    private const string SlidesFolder = "slides";

    public SlideshowGenerationService(
        IToolboxTalksDbContext context,
        IR2StorageService r2Storage,
        HttpClient httpClient,
        ILogger<SlideshowGenerationService> logger)
    {
        _context = context;
        _r2Storage = r2Storage;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<int>> GenerateSlidesFromPdfAsync(
        Guid tenantId,
        Guid toolboxTalkId,
        CancellationToken ct = default)
    {
        var talk = await _context.ToolboxTalks
            .IgnoreQueryFilters()
            .Include(t => t.Slides.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(t => t.Id == toolboxTalkId
                && t.TenantId == tenantId
                && !t.IsDeleted, ct);

        if (talk == null)
            return Result.Fail<int>("Toolbox talk not found");

        if (string.IsNullOrEmpty(talk.PdfUrl))
            return Result.Fail<int>("No PDF attached to this talk");

        _logger.LogInformation(
            "Starting slide generation for talk {TalkId} from PDF {PdfUrl}",
            toolboxTalkId, talk.PdfUrl);

        // Download PDF from R2 via public URL
        byte[] pdfBytes;
        try
        {
            using var response = await _httpClient.GetAsync(talk.PdfUrl, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to download PDF. Status: {StatusCode}", response.StatusCode);
                return Result.Fail<int>($"Failed to download PDF. HTTP status: {response.StatusCode}");
            }
            pdfBytes = await response.Content.ReadAsByteArrayAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download PDF from {PdfUrl}", talk.PdfUrl);
            return Result.Fail<int>($"Failed to download PDF: {ex.Message}");
        }

        // Soft-delete existing slides if regenerating
        if (talk.Slides.Any())
        {
            foreach (var existingSlide in talk.Slides)
            {
                existingSlide.IsDeleted = true;
            }
            _logger.LogInformation("Marked {Count} existing slides for deletion", talk.Slides.Count);
        }

        try
        {
            // Open PDF for text extraction
            using var pdfDocument = PdfDocument.Open(pdfBytes);
            var pageCount = pdfDocument.NumberOfPages;

            if (pageCount > MaxPages)
            {
                _logger.LogWarning(
                    "PDF has {PageCount} pages, exceeding limit of {MaxPages}",
                    pageCount, MaxPages);
                return Result.Fail<int>(
                    $"PDF has too many pages ({pageCount}). Maximum allowed is {MaxPages}.");
            }

            var slidesCreated = 0;
            var renderFailures = 0;
            var uploadFailures = 0;

            for (var pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                ct.ThrowIfCancellationRequested();

                // Extract text from page using PdfPig
                var page = pdfDocument.GetPage(pageNum);
                var pageText = ExtractPageText(page, pageNum);

                // Render page to PNG using PDFtoImage (0-based index)
                var imageBytes = RenderPageToImage(pdfBytes, pageNum - 1, pageNum);
                if (imageBytes == null)
                {
                    renderFailures++;
                    _logger.LogWarning("Failed to render page {PageNum}/{Total}, skipping", pageNum, pageCount);
                    continue;
                }

                // Upload image to R2
                var storagePath = $"{tenantId}/{SlidesFolder}/{toolboxTalkId}/{pageNum}.png";
                var uploadResult = await _r2Storage.UploadSlideImageAsync(storagePath, imageBytes, ct);

                if (!uploadResult.Success)
                {
                    uploadFailures++;
                    _logger.LogWarning(
                        "Failed to upload slide image for page {PageNum}/{Total}: {Error}",
                        pageNum, pageCount, uploadResult.ErrorMessage);
                    continue;
                }

                // Create slide record - use DbSet.Add for reliable change tracking
                var slide = new ToolboxTalkSlide
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ToolboxTalkId = toolboxTalkId,
                    PageNumber = pageNum,
                    ImageStoragePath = storagePath,
                    OriginalText = string.IsNullOrWhiteSpace(pageText) ? null : pageText.Trim(),
                };

                _context.ToolboxTalkSlides.Add(slide);
                slidesCreated++;

                _logger.LogDebug(
                    "Created slide {PageNum}/{Total} for talk {TalkId}",
                    pageNum, pageCount, toolboxTalkId);
            }

            // Only mark as generated if at least one slide was created
            if (slidesCreated == 0)
            {
                _logger.LogError(
                    "No slides could be generated for talk {TalkId}. " +
                    "Pages: {PageCount}, RenderFailures: {RenderFailures}, UploadFailures: {UploadFailures}. " +
                    "This usually means the PDF rendering library (PDFium) is not available on the server.",
                    toolboxTalkId, pageCount, renderFailures, uploadFailures);

                return Result.Fail<int>(
                    $"Failed to generate slides: {renderFailures} page(s) failed to render, " +
                    $"{uploadFailures} failed to upload (out of {pageCount} pages).");
            }

            talk.SlidesGenerated = true;

            var saved = await _context.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Successfully generated {Count}/{Total} slides for talk {TalkId} (SaveChanges wrote {Saved} rows). " +
                "RenderFailures: {RenderFailures}, UploadFailures: {UploadFailures}",
                slidesCreated, pageCount, toolboxTalkId, saved, renderFailures, uploadFailures);

            return Result.Ok(slidesCreated);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Slide generation was cancelled for talk {TalkId}", toolboxTalkId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating slides for talk {TalkId}", toolboxTalkId);
            return Result.Fail<int>($"Error processing PDF: {ex.Message}");
        }
    }

    private string ExtractPageText(UglyToad.PdfPig.Content.Page page, int pageNum)
    {
        try
        {
            return page.Text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract text from page {PageNum}", pageNum);
            return string.Empty;
        }
    }

    private byte[]? RenderPageToImage(byte[] pdfBytes, int pageIndex, int pageNum)
    {
        try
        {
            using var stream = new MemoryStream();
            Conversion.SavePng(stream, pdfBytes, pageIndex, options: new RenderOptions(Dpi: RenderDpi));
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render page {PageNum} to image", pageNum);
            return null;
        }
    }
}
