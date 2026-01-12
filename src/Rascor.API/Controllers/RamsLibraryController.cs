using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.API.Controllers;

/// <summary>
/// API endpoints for RAMS reference libraries (Hazards, Controls, Legislation, SOPs)
/// </summary>
[ApiController]
[Route("api/rams/library")]
[Authorize(Policy = "Rams.View")]
public class RamsLibraryController : ControllerBase
{
    private readonly IRamsLibraryService _libraryService;
    private readonly ILogger<RamsLibraryController> _logger;

    public RamsLibraryController(
        IRamsLibraryService libraryService,
        ILogger<RamsLibraryController> logger)
    {
        _libraryService = libraryService;
        _logger = logger;
    }

    #region Hazards

    /// <summary>
    /// Get all hazards with optional filtering
    /// </summary>
    [HttpGet("hazards")]
    public async Task<ActionResult<IEnumerable<HazardLibraryDto>>> GetAllHazards(
        [FromQuery] bool includeInactive = false,
        [FromQuery] HazardCategory? category = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<HazardLibraryDto> hazards;

            if (!string.IsNullOrWhiteSpace(search))
                hazards = await _libraryService.SearchHazardsAsync(search, cancellationToken);
            else if (category.HasValue)
                hazards = await _libraryService.GetHazardsByCategoryAsync(category.Value, cancellationToken);
            else
                hazards = await _libraryService.GetAllHazardsAsync(includeInactive, cancellationToken);

            return Ok(hazards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hazards");
            return StatusCode(500, new { message = "Error retrieving hazards" });
        }
    }

    /// <summary>
    /// Get a hazard by ID
    /// </summary>
    [HttpGet("hazards/{id:guid}")]
    public async Task<ActionResult<HazardLibraryDto>> GetHazardById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var hazard = await _libraryService.GetHazardByIdAsync(id, cancellationToken);

            if (hazard == null)
                return NotFound(new { message = "Hazard not found" });

            return Ok(hazard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hazard {HazardId}", id);
            return StatusCode(500, new { message = "Error retrieving hazard" });
        }
    }

    /// <summary>
    /// Create a new hazard
    /// </summary>
    [HttpPost("hazards")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<HazardLibraryDto>> CreateHazard(
        [FromBody] CreateHazardLibraryDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var hazard = await _libraryService.CreateHazardAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetHazardById), new { id = hazard.Id }, hazard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hazard");
            return StatusCode(500, new { message = "Error creating hazard" });
        }
    }

    /// <summary>
    /// Update a hazard
    /// </summary>
    [HttpPut("hazards/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<HazardLibraryDto>> UpdateHazard(
        Guid id,
        [FromBody] UpdateHazardLibraryDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var hazard = await _libraryService.UpdateHazardAsync(id, dto, cancellationToken);
            return Ok(hazard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hazard {HazardId}", id);
            return StatusCode(500, new { message = "Error updating hazard" });
        }
    }

    /// <summary>
    /// Delete a hazard
    /// </summary>
    [HttpDelete("hazards/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<IActionResult> DeleteHazard(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _libraryService.DeleteHazardAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hazard {HazardId}", id);
            return StatusCode(500, new { message = "Error deleting hazard" });
        }
    }

    #endregion

    #region Controls

    /// <summary>
    /// Get all control measures with optional filtering
    /// </summary>
    [HttpGet("controls")]
    public async Task<ActionResult<IEnumerable<ControlMeasureLibraryDto>>> GetAllControls(
        [FromQuery] bool includeInactive = false,
        [FromQuery] HazardCategory? category = null,
        [FromQuery] ControlHierarchy? hierarchy = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<ControlMeasureLibraryDto> controls;

            if (!string.IsNullOrWhiteSpace(search))
                controls = await _libraryService.SearchControlsAsync(search, cancellationToken);
            else if (hierarchy.HasValue)
                controls = await _libraryService.GetControlsByHierarchyAsync(hierarchy.Value, cancellationToken);
            else if (category.HasValue)
                controls = await _libraryService.GetControlsByCategoryAsync(category.Value, cancellationToken);
            else
                controls = await _libraryService.GetAllControlsAsync(includeInactive, cancellationToken);

            return Ok(controls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving controls");
            return StatusCode(500, new { message = "Error retrieving controls" });
        }
    }

    /// <summary>
    /// Get a control measure by ID
    /// </summary>
    [HttpGet("controls/{id:guid}")]
    public async Task<ActionResult<ControlMeasureLibraryDto>> GetControlById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var control = await _libraryService.GetControlByIdAsync(id, cancellationToken);

            if (control == null)
                return NotFound(new { message = "Control not found" });

            return Ok(control);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving control {ControlId}", id);
            return StatusCode(500, new { message = "Error retrieving control" });
        }
    }

    /// <summary>
    /// Create a new control measure
    /// </summary>
    [HttpPost("controls")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<ControlMeasureLibraryDto>> CreateControl(
        [FromBody] CreateControlMeasureLibraryDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var control = await _libraryService.CreateControlAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetControlById), new { id = control.Id }, control);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating control");
            return StatusCode(500, new { message = "Error creating control" });
        }
    }

    /// <summary>
    /// Update a control measure
    /// </summary>
    [HttpPut("controls/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<ControlMeasureLibraryDto>> UpdateControl(
        Guid id,
        [FromBody] UpdateControlMeasureLibraryDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var control = await _libraryService.UpdateControlAsync(id, dto, cancellationToken);
            return Ok(control);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating control {ControlId}", id);
            return StatusCode(500, new { message = "Error updating control" });
        }
    }

    /// <summary>
    /// Delete a control measure
    /// </summary>
    [HttpDelete("controls/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<IActionResult> DeleteControl(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _libraryService.DeleteControlAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting control {ControlId}", id);
            return StatusCode(500, new { message = "Error deleting control" });
        }
    }

    #endregion

    #region Legislation

    /// <summary>
    /// Get all legislation references with optional filtering
    /// </summary>
    [HttpGet("legislation")]
    public async Task<ActionResult<IEnumerable<LegislationReferenceDto>>> GetAllLegislation(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? jurisdiction = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<LegislationReferenceDto> legislation;

            if (!string.IsNullOrWhiteSpace(search))
                legislation = await _libraryService.SearchLegislationAsync(search, cancellationToken);
            else if (!string.IsNullOrWhiteSpace(jurisdiction))
                legislation = await _libraryService.GetLegislationByJurisdictionAsync(jurisdiction, cancellationToken);
            else
                legislation = await _libraryService.GetAllLegislationAsync(includeInactive, cancellationToken);

            return Ok(legislation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving legislation");
            return StatusCode(500, new { message = "Error retrieving legislation" });
        }
    }

    /// <summary>
    /// Get a legislation reference by ID
    /// </summary>
    [HttpGet("legislation/{id:guid}")]
    public async Task<ActionResult<LegislationReferenceDto>> GetLegislationById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var legislation = await _libraryService.GetLegislationByIdAsync(id, cancellationToken);

            if (legislation == null)
                return NotFound(new { message = "Legislation not found" });

            return Ok(legislation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving legislation {LegislationId}", id);
            return StatusCode(500, new { message = "Error retrieving legislation" });
        }
    }

    /// <summary>
    /// Create a new legislation reference
    /// </summary>
    [HttpPost("legislation")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<LegislationReferenceDto>> CreateLegislation(
        [FromBody] CreateLegislationReferenceDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var legislation = await _libraryService.CreateLegislationAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetLegislationById), new { id = legislation.Id }, legislation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating legislation");
            return StatusCode(500, new { message = "Error creating legislation" });
        }
    }

    /// <summary>
    /// Update a legislation reference
    /// </summary>
    [HttpPut("legislation/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<LegislationReferenceDto>> UpdateLegislation(
        Guid id,
        [FromBody] UpdateLegislationReferenceDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var legislation = await _libraryService.UpdateLegislationAsync(id, dto, cancellationToken);
            return Ok(legislation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating legislation {LegislationId}", id);
            return StatusCode(500, new { message = "Error updating legislation" });
        }
    }

    /// <summary>
    /// Delete a legislation reference
    /// </summary>
    [HttpDelete("legislation/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<IActionResult> DeleteLegislation(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _libraryService.DeleteLegislationAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting legislation {LegislationId}", id);
            return StatusCode(500, new { message = "Error deleting legislation" });
        }
    }

    #endregion

    #region SOPs

    /// <summary>
    /// Get all SOP references with optional filtering
    /// </summary>
    [HttpGet("sops")]
    public async Task<ActionResult<IEnumerable<SopReferenceDto>>> GetAllSops(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<SopReferenceDto> sops;

            if (!string.IsNullOrWhiteSpace(search))
                sops = await _libraryService.SearchSopsAsync(search, cancellationToken);
            else
                sops = await _libraryService.GetAllSopsAsync(includeInactive, cancellationToken);

            return Ok(sops);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SOPs");
            return StatusCode(500, new { message = "Error retrieving SOPs" });
        }
    }

    /// <summary>
    /// Get a SOP reference by ID
    /// </summary>
    [HttpGet("sops/{id:guid}")]
    public async Task<ActionResult<SopReferenceDto>> GetSopById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var sop = await _libraryService.GetSopByIdAsync(id, cancellationToken);

            if (sop == null)
                return NotFound(new { message = "SOP not found" });

            return Ok(sop);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SOP {SopId}", id);
            return StatusCode(500, new { message = "Error retrieving SOP" });
        }
    }

    /// <summary>
    /// Create a new SOP reference
    /// </summary>
    [HttpPost("sops")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<SopReferenceDto>> CreateSop(
        [FromBody] CreateSopReferenceDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var sop = await _libraryService.CreateSopAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetSopById), new { id = sop.Id }, sop);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SOP");
            return StatusCode(500, new { message = "Error creating SOP" });
        }
    }

    /// <summary>
    /// Update a SOP reference
    /// </summary>
    [HttpPut("sops/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<ActionResult<SopReferenceDto>> UpdateSop(
        Guid id,
        [FromBody] UpdateSopReferenceDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var sop = await _libraryService.UpdateSopAsync(id, dto, cancellationToken);
            return Ok(sop);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SOP {SopId}", id);
            return StatusCode(500, new { message = "Error updating SOP" });
        }
    }

    /// <summary>
    /// Delete a SOP reference
    /// </summary>
    [HttpDelete("sops/{id:guid}")]
    [Authorize(Policy = "Rams.Admin")]
    public async Task<IActionResult> DeleteSop(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _libraryService.DeleteSopAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SOP {SopId}", id);
            return StatusCode(500, new { message = "Error deleting SOP" });
        }
    }

    #endregion
}
