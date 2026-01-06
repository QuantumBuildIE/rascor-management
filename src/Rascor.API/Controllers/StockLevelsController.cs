using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.StockLevels;
using Rascor.Modules.StockManagement.Application.Features.StockLevels.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/stock-levels")]
[Authorize(Policy = "StockManagement.View")]
public class StockLevelsController : ControllerBase
{
    private readonly IStockLevelService _stockLevelService;

    public StockLevelsController(IStockLevelService stockLevelService)
    {
        _stockLevelService = stockLevelService;
    }

    /// <summary>
    /// Get all stock levels
    /// </summary>
    /// <returns>List of stock levels</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _stockLevelService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a stock level by ID
    /// </summary>
    /// <param name="id">Stock level ID</param>
    /// <returns>Stock level details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockLevelService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock levels by location
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>List of stock levels at the location</returns>
    [HttpGet("by-location/{locationId}")]
    public async Task<IActionResult> GetByLocation(Guid locationId)
    {
        var result = await _stockLevelService.GetByLocationAsync(locationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get low stock items (quantity on hand at or below reorder level)
    /// </summary>
    /// <returns>List of low stock items</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await _stockLevelService.GetLowStockAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock level by product and location
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="locationId">Location ID</param>
    /// <returns>Stock level for the product at the location</returns>
    [HttpGet("by-product/{productId}/location/{locationId}")]
    public async Task<IActionResult> GetByProductAndLocation(Guid productId, Guid locationId)
    {
        var result = await _stockLevelService.GetByProductAndLocationAsync(productId, locationId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new stock level
    /// </summary>
    /// <param name="dto">Stock level creation data</param>
    /// <returns>Created stock level</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockLevelDto dto)
    {
        var result = await _stockLevelService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing stock level
    /// </summary>
    /// <param name="id">Stock level ID</param>
    /// <param name="dto">Stock level update data</param>
    /// <returns>Updated stock level</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockLevelDto dto)
    {
        var result = await _stockLevelService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a stock level (soft delete)
    /// </summary>
    /// <param name="id">Stock level ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockLevelService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
