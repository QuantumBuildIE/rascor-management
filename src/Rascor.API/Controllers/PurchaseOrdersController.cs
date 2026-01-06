using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize(Policy = "StockManagement.View")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    /// <summary>
    /// Get all purchase orders
    /// </summary>
    /// <returns>List of purchase orders</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _purchaseOrderService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a purchase order by ID
    /// </summary>
    /// <param name="id">Purchase order ID</param>
    /// <returns>Purchase order details with lines</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get purchase orders by supplier
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>List of purchase orders for the supplier</returns>
    [HttpGet("by-supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(Guid supplierId)
    {
        var result = await _purchaseOrderService.GetBySupplierAsync(supplierId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get purchase orders by status
    /// </summary>
    /// <param name="status">Status (Draft, Confirmed, PartiallyReceived, FullyReceived, Cancelled)</param>
    /// <returns>List of purchase orders with the specified status</returns>
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var result = await _purchaseOrderService.GetByStatusAsync(status);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new purchase order with lines
    /// </summary>
    /// <param name="dto">Purchase order creation data including lines</param>
    /// <returns>Created purchase order</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var result = await _purchaseOrderService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing purchase order (only Draft orders)
    /// </summary>
    /// <param name="id">Purchase order ID</param>
    /// <param name="dto">Purchase order update data</param>
    /// <returns>Updated purchase order</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        var result = await _purchaseOrderService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Confirm a purchase order (Draft -> Confirmed)
    /// </summary>
    /// <param name="id">Purchase order ID</param>
    /// <returns>Confirmed purchase order</returns>
    [HttpPost("{id}/confirm")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _purchaseOrderService.ConfirmAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a purchase order
    /// </summary>
    /// <param name="id">Purchase order ID</param>
    /// <returns>Cancelled purchase order</returns>
    [HttpPost("{id}/cancel")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _purchaseOrderService.CancelAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a purchase order (soft delete, only Draft orders)
    /// </summary>
    /// <param name="id">Purchase order ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _purchaseOrderService.DeleteAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return NoContent();
    }
}
