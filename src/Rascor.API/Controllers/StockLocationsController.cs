using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.StockLocations;
using Rascor.Modules.StockManagement.Application.Features.StockLocations.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/stock-locations")]
[Authorize(Policy = "StockManagement.View")]
public class StockLocationsController : ControllerBase
{
    private readonly IStockLocationService _stockLocationService;

    public StockLocationsController(IStockLocationService stockLocationService)
    {
        _stockLocationService = stockLocationService;
    }

    /// <summary>
    /// Get all stock locations
    /// </summary>
    /// <returns>List of stock locations</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _stockLocationService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a stock location by ID
    /// </summary>
    /// <param name="id">Stock location ID</param>
    /// <returns>Stock location details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockLocationService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new stock location
    /// </summary>
    /// <param name="dto">Stock location creation data</param>
    /// <returns>Created stock location</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.Admin")]
    public async Task<IActionResult> Create([FromBody] CreateStockLocationDto dto)
    {
        var result = await _stockLocationService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing stock location
    /// </summary>
    /// <param name="id">Stock location ID</param>
    /// <param name="dto">Stock location update data</param>
    /// <returns>Updated stock location</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "StockManagement.Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockLocationDto dto)
    {
        var result = await _stockLocationService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a stock location (soft delete)
    /// </summary>
    /// <param name="id">Stock location ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockLocationService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
