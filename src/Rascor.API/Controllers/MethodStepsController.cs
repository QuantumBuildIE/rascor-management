using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/rams/{ramsDocumentId:guid}/method-steps")]
[Authorize(Policy = "Rams.View")]
public class MethodStepsController : ControllerBase
{
    private readonly IMethodStepService _service;
    private readonly ILogger<MethodStepsController> _logger;

    public MethodStepsController(
        IMethodStepService service,
        ILogger<MethodStepsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all method steps for a RAMS document
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

            var steps = await _service.GetByRamsDocumentIdAsync(ramsDocumentId, cancellationToken);
            return Ok(steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving method steps for RAMS document {RamsDocumentId}", ramsDocumentId);
            return StatusCode(500, new { message = "Error retrieving method steps" });
        }
    }

    /// <summary>
    /// Get a method step by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid ramsDocumentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var step = await _service.GetByIdAsync(id, cancellationToken);

            if (step == null || step.RamsDocumentId != ramsDocumentId)
                return NotFound(new { message = "Method step not found" });

            return Ok(step);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving method step {Id}", id);
            return StatusCode(500, new { message = "Error retrieving method step" });
        }
    }

    /// <summary>
    /// Create a new method step
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Create(
        Guid ramsDocumentId,
        [FromBody] CreateMethodStepDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var step = await _service.CreateAsync(ramsDocumentId, dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { ramsDocumentId, id = step.Id },
                step);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating method step for RAMS document {RamsDocumentId}", ramsDocumentId);
            return StatusCode(500, new { message = "Error creating method step" });
        }
    }

    /// <summary>
    /// Update a method step
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Update(
        Guid ramsDocumentId,
        Guid id,
        [FromBody] UpdateMethodStepDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // First verify the step belongs to the specified document
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null || existing.RamsDocumentId != ramsDocumentId)
                return NotFound(new { message = "Method step not found" });

            var step = await _service.UpdateAsync(id, dto, cancellationToken);
            return Ok(step);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating method step {Id}", id);
            return StatusCode(500, new { message = "Error updating method step" });
        }
    }

    /// <summary>
    /// Delete a method step
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
            // First verify the step belongs to the specified document
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing == null || existing.RamsDocumentId != ramsDocumentId)
                return NotFound(new { message = "Method step not found" });

            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting method step {Id}", id);
            return StatusCode(500, new { message = "Error deleting method step" });
        }
    }

    /// <summary>
    /// Reorder method steps
    /// </summary>
    [HttpPost("reorder")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> Reorder(
        Guid ramsDocumentId,
        [FromBody] ReorderMethodStepsDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var steps = await _service.ReorderAsync(ramsDocumentId, dto, cancellationToken);
            return Ok(steps);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering method steps for RAMS document {RamsDocumentId}", ramsDocumentId);
            return StatusCode(500, new { message = "Error reordering method steps" });
        }
    }

    /// <summary>
    /// Insert a method step at a specific position
    /// </summary>
    [HttpPost("insert-at/{position:int}")]
    [Authorize(Policy = "Rams.Edit")]
    public async Task<IActionResult> InsertAt(
        Guid ramsDocumentId,
        int position,
        [FromBody] CreateMethodStepDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var step = await _service.InsertAtAsync(ramsDocumentId, position, dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { ramsDocumentId, id = step.Id },
                step);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting method step at position {Position} for RAMS document {RamsDocumentId}", position, ramsDocumentId);
            return StatusCode(500, new { message = "Error inserting method step" });
        }
    }
}
