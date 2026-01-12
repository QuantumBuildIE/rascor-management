using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.ProductKits;
using Rascor.Modules.StockManagement.Application.Features.ProductKits.DTOs;

namespace Rascor.API.Controllers;

[ApiController]
[Route("api/stockmanagement/productkits")]
[Authorize(Policy = "StockManagement.View")]
public class ProductKitsController : ControllerBase
{
    private readonly IProductKitService _productKitService;

    public ProductKitsController(IProductKitService productKitService)
    {
        _productKitService = productKitService;
    }

    /// <summary>
    /// Get all product kits with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await _productKitService.GetAllAsync(
            search, categoryId, isActive, pageNumber, pageSize, sortColumn, sortDirection);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a product kit by ID with all items
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productKitService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new product kit
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Create([FromBody] CreateProductKitDto dto)
    {
        var result = await _productKitService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing product kit
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductKitDto dto)
    {
        var result = await _productKitService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a product kit (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productKitService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Add an item to a product kit
    /// </summary>
    [HttpPost("{kitId}/items")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> AddItem(Guid kitId, [FromBody] CreateProductKitItemDto dto)
    {
        var result = await _productKitService.AddItemAsync(kitId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a product kit item
    /// </summary>
    [HttpPut("items/{itemId}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateProductKitItemDto dto)
    {
        var result = await _productKitService.UpdateItemAsync(itemId, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a product kit item
    /// </summary>
    [HttpDelete("items/{itemId}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> DeleteItem(Guid itemId)
    {
        var result = await _productKitService.DeleteItemAsync(itemId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
