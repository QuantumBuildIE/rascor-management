using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.Stocktakes;
using Rascor.Modules.StockManagement.Application.Features.Stocktakes.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/stocktakes")]
[Authorize(Policy = "StockManagement.Stocktake")]
public class StocktakesController : ControllerBase
{
    private readonly IStocktakeService _stocktakeService;

    public StocktakesController(IStocktakeService stocktakeService)
    {
        _stocktakeService = stocktakeService;
    }

    /// <summary>
    /// Get all stocktakes
    /// </summary>
    /// <returns>List of stocktakes</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _stocktakeService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a stocktake by ID
    /// </summary>
    /// <param name="id">Stocktake ID</param>
    /// <returns>Stocktake details with lines</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stocktakeService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stocktakes by location
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>List of stocktakes for the location</returns>
    [HttpGet("by-location/{locationId}")]
    public async Task<IActionResult> GetByLocation(Guid locationId)
    {
        var result = await _stocktakeService.GetByLocationAsync(locationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new stocktake (auto-populates lines with all products at the location)
    /// </summary>
    /// <param name="dto">Stocktake creation data</param>
    /// <returns>Created stocktake with auto-populated lines</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStocktakeDto dto)
    {
        var result = await _stocktakeService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Start a stocktake (changes status from Draft to InProgress)
    /// </summary>
    /// <param name="id">Stocktake ID</param>
    /// <returns>Updated stocktake</returns>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _stocktakeService.StartAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a stocktake line's counted quantity
    /// </summary>
    /// <param name="id">Stocktake ID</param>
    /// <param name="lineId">Stocktake line ID</param>
    /// <param name="dto">Updated counted quantity</param>
    /// <returns>Updated stocktake</returns>
    [HttpPut("{id}/lines/{lineId}")]
    public async Task<IActionResult> UpdateLine(Guid id, Guid lineId, [FromBody] UpdateStocktakeLineDto dto)
    {
        var result = await _stocktakeService.UpdateLineAsync(id, lineId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Complete a stocktake (creates adjustments for variances)
    /// </summary>
    /// <param name="id">Stocktake ID</param>
    /// <returns>Completed stocktake</returns>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _stocktakeService.CompleteAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a stocktake
    /// </summary>
    /// <param name="id">Stocktake ID</param>
    /// <returns>Cancelled stocktake</returns>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _stocktakeService.CancelAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a stocktake (soft delete, only non-completed stocktakes)
    /// </summary>
    /// <param name="id">Stocktake ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stocktakeService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
