using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.StockTransactions;
using Rascor.Modules.StockManagement.Application.Features.StockTransactions.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/stock-transactions")]
[Authorize(Policy = "StockManagement.View")]
public class StockTransactionsController : ControllerBase
{
    private readonly IStockTransactionService _stockTransactionService;

    public StockTransactionsController(IStockTransactionService stockTransactionService)
    {
        _stockTransactionService = stockTransactionService;
    }

    /// <summary>
    /// Get all stock transactions
    /// </summary>
    /// <returns>List of stock transactions</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _stockTransactionService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a stock transaction by ID
    /// </summary>
    /// <param name="id">Stock transaction ID</param>
    /// <returns>Stock transaction details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockTransactionService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock transactions by product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of stock transactions for the product</returns>
    [HttpGet("by-product/{productId}")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var result = await _stockTransactionService.GetByProductAsync(productId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock transactions by location
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>List of stock transactions at the location</returns>
    [HttpGet("by-location/{locationId}")]
    public async Task<IActionResult> GetByLocation(Guid locationId)
    {
        var result = await _stockTransactionService.GetByLocationAsync(locationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock transactions by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of stock transactions within the date range</returns>
    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _stockTransactionService.GetByDateRangeAsync(startDate, endDate);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new stock transaction
    /// </summary>
    /// <param name="dto">Stock transaction creation data</param>
    /// <returns>Created stock transaction</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockTransactionDto dto)
    {
        var result = await _stockTransactionService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Delete a stock transaction (soft delete)
    /// </summary>
    /// <param name="id">Stock transaction ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockTransactionService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
