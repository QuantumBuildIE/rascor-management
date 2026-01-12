using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/rams/{ramsDocumentId:guid}/risk-assessments")]
[Authorize(Policy = "Rams.View")]
public class RiskAssessmentsController : ControllerBase
{
    private readonly IRiskAssessmentService _service;
    private readonly ILogger<RiskAssessmentsController> _logger;

    public RiskAssessmentsController(
        IRiskAssessmentService service,
        ILogger<RiskAssessmentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all risk assessments for a RAMS document
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid ramsDocumentId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await _service.RamsDocumentExistsAsync(ramsDocumentId, cancellationToken))
                return NotFound(new { message = "RAMS document not found" });

            var assessments = await _service.GetByRamsDocumentIdAsync(ramsDocumentId, cancellationToken);
            return Ok(assessments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessments for RAMS document {RamsDocumentId}", ramsDocumentId);
            return StatusCode(500, new { message = "Error retrieving risk assessments" });
        }
    }

    /// <summary>
    /// Get a risk assessment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid ramsDocumentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var assessment = await _service.GetByIdAsync(id, cancellationToken);

            if (assessment == null || assessment.RamsDocumentId != ramsDocumentId)
                return NotFound(new { message = "Risk assessment not found" });

            return Ok(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessment {Id}", id);
            return StatusCode(500, new { message = "Error retrieving risk assessment" });
        }
    }

    /// <summary>
    /// Create a new risk assessment
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Create(
        Guid ramsDocumentId,
        [FromBody] CreateRiskAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var assessment = await _service.CreateAsync(ramsDocumentId, dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { ramsDocumentId, id = assessment.Id },
                assessment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating risk assessment for RAMS document {RamsDocumentId}", ramsDocumentId);
            return StatusCode(500, new { message = "Error creating risk assessment" });
        }
    }

    /// <summary>
    /// Update a risk assessment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Update(
        Guid ramsDocumentId,
        Guid id,
        [FromBody] UpdateRiskAssessmentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // First verify the assessment belongs to the specified document
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null || existing.RamsDocumentId != ramsDocumentId)
                return NotFound(new { message = "Risk assessment not found" });

            var assessment = await _service.UpdateAsync(id, dto, cancellationToken);
            return Ok(assessment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating risk assessment {Id}", id);
            return StatusCode(500, new { message = "Error updating risk assessment" });
        }
    }

    /// <summary>
    /// Delete a risk assessment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Delete(
        Guid ramsDocumentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            // First verify the assessment belongs to the specified document
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null || existing.RamsDocumentId != ramsDocumentId)
                return NotFound(new { message = "Risk assessment not found" });

            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting risk assessment {Id}", id);
            return StatusCode(500, new { message = "Error deleting risk assessment" });
        }
    }

    /// <summary>
    /// Reorder risk assessments
    /// </summary>
    [HttpPost("reorder")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Reorder(
        Guid ramsDocumentId,
        [FromBody] ReorderRiskAssessmentsDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var assessments = await _service.ReorderAsync(ramsDocumentId, dto, cancellationToken);
            return Ok(assessments);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering risk assessments for RAMS document {RamsDocumentId}", ramsDocumentId);
            return StatusCode(500, new { message = "Error reordering risk assessments" });
        }
    }
}
