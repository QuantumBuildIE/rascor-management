using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Core.Application.Models;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/rams")]
[Authorize(Policy = "Rams.View")]
public class RamsDocumentsController : ControllerBase
{
    private readonly IRamsDocumentService _documentService;
    private readonly IRamsPdfService _pdfService;
    private readonly ILogger<RamsDocumentsController> _logger;

    public RamsDocumentsController(
        IRamsDocumentService documentService,
        IRamsPdfService pdfService,
        ILogger<RamsDocumentsController> logger)
    {
        _documentService = documentService;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Get RAMS documents with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] string? search = null,
        [FromQuery] RamsStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _documentService.GetDocumentsAsync(
                search, status, pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);
            return Ok(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving RAMS documents");
            return StatusCode(500, Result.Fail("Error retrieving RAMS documents"));
        }
    }

    /// <summary>
    /// Get a RAMS document by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);

            if (document == null)
                return NotFound(new { message = "RAMS document not found" });

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving RAMS document {DocumentId}", id);
            return StatusCode(500, new { message = "Error retrieving RAMS document" });
        }
    }

    /// <summary>
    /// Create a new RAMS document
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Rams.Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateRamsDocumentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.CreateDocumentAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating RAMS document");
            return StatusCode(500, new { message = "Error creating RAMS document" });
        }
    }

    /// <summary>
    /// Update a RAMS document
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRamsDocumentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.UpdateDocumentAsync(id, dto, cancellationToken);
            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating RAMS document {DocumentId}", id);
            return StatusCode(500, new { message = "Error updating RAMS document" });
        }
    }

    /// <summary>
    /// Delete a RAMS document
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Rams.Delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _documentService.DeleteDocumentAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting RAMS document {DocumentId}", id);
            return StatusCode(500, new { message = "Error deleting RAMS document" });
        }
    }

    /// <summary>
    /// Submit a RAMS document for review
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "Rams.Submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.SubmitForReviewAsync(id, cancellationToken);
            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting RAMS document {DocumentId}", id);
            return StatusCode(500, new { message = "Error submitting RAMS document" });
        }
    }

    /// <summary>
    /// Approve a RAMS document
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Rams.Approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApprovalDto? dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.ApproveAsync(id, dto, cancellationToken);
            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving RAMS document {DocumentId}", id);
            return StatusCode(500, new { message = "Error approving RAMS document" });
        }
    }

    /// <summary>
    /// Reject a RAMS document
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "Rams.Approve")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] ApprovalDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.RejectAsync(id, dto, cancellationToken);
            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting RAMS document {DocumentId}", id);
            return StatusCode(500, new { message = "Error rejecting RAMS document" });
        }
    }

    /// <summary>
    /// Generate PDF for a RAMS document (download)
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GeneratePdf(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);

            if (document == null)
                return NotFound(new { message = "RAMS document not found" });

            var pdfBytes = await _pdfService.GeneratePdfAsync(id, cancellationToken);
            var fileName = $"RAMS_{document.ProjectReference}_{DateTime.UtcNow:yyyyMMdd}.pdf";

            _logger.LogInformation("Generated PDF for RAMS document {Id}: {FileName}", id, fileName);

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for RAMS document {Id}", id);
            return StatusCode(500, new { message = "Failed to generate PDF" });
        }
    }

    /// <summary>
    /// Preview PDF for a RAMS document (inline display)
    /// </summary>
    [HttpGet("{id:guid}/pdf/preview")]
    public async Task<IActionResult> PreviewPdf(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);

            if (document == null)
                return NotFound(new { message = "RAMS document not found" });

            var pdfBytes = await _pdfService.GeneratePdfAsync(id, cancellationToken);

            return File(pdfBytes, "application/pdf");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF preview for RAMS document {Id}", id);
            return StatusCode(500, new { message = "Failed to generate PDF" });
        }
    }
}
