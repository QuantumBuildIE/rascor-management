using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.Suppliers;
using Rascor.Modules.StockManagement.Application.Features.Suppliers.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize(Policy = "StockManagement.View")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Get all suppliers
    /// </summary>
    /// <returns>List of suppliers</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _supplierService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a supplier by ID
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _supplierService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new supplier
    /// </summary>
    /// <param name="dto">Supplier creation data</param>
    /// <returns>Created supplier</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.ManageSuppliers")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var result = await _supplierService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="dto">Supplier update data</param>
    /// <returns>Updated supplier</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "StockManagement.ManageSuppliers")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto)
    {
        var result = await _supplierService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a supplier (soft delete)
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.ManageSuppliers")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _supplierService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
