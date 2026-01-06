using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.BayLocations;
using Rascor.Modules.StockManagement.Application.Features.BayLocations.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/bay-locations")]
[Authorize(Policy = "StockManagement.View")]
public class BayLocationsController : ControllerBase
{
    private readonly IBayLocationService _bayLocationService;

    public BayLocationsController(IBayLocationService bayLocationService)
    {
        _bayLocationService = bayLocationService;
    }

    /// <summary>
    /// Get all bay locations, optionally filtered by stock location
    /// </summary>
    /// <param name="stockLocationId">Optional stock location ID to filter by</param>
    /// <returns>List of bay locations</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? stockLocationId = null)
    {
        var result = await _bayLocationService.GetAllAsync(stockLocationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a bay location by ID
    /// </summary>
    /// <param name="id">Bay location ID</param>
    /// <returns>Bay location details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _bayLocationService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get bay locations by stock location
    /// </summary>
    /// <param name="stockLocationId">Stock location ID</param>
    /// <returns>List of bay locations at the specified stock location</returns>
    [HttpGet("by-location/{stockLocationId}")]
    public async Task<IActionResult> GetByLocation(Guid stockLocationId)
    {
        var result = await _bayLocationService.GetByLocationAsync(stockLocationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new bay location
    /// </summary>
    /// <param name="dto">Bay location creation data</param>
    /// <returns>Created bay location</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBayLocationDto dto)
    {
        var result = await _bayLocationService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing bay location
    /// </summary>
    /// <param name="id">Bay location ID</param>
    /// <param name="dto">Bay location update data</param>
    /// <returns>Updated bay location</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "StockManagement.Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBayLocationDto dto)
    {
        var result = await _bayLocationService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a bay location (soft delete)
    /// </summary>
    /// <param name="id">Bay location ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _bayLocationService.DeleteAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return NoContent();
    }
}
