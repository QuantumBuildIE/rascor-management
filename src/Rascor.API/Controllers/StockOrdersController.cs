using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.StockOrders;
using Rascor.Modules.StockManagement.Application.Features.StockOrders.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/stock-orders")]
[Authorize(Policy = "StockManagement.View")]
public class StockOrdersController : ControllerBase
{
    private readonly IStockOrderService _stockOrderService;

    public StockOrdersController(IStockOrderService stockOrderService)
    {
        _stockOrderService = stockOrderService;
    }

    /// <summary>
    /// Get all stock orders
    /// </summary>
    /// <returns>List of stock orders</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _stockOrderService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a stock order by ID
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <returns>Stock order details with lines</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockOrderService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a stock order for docket printing (includes bay locations for picking)
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <param name="warehouseLocationId">Warehouse location to get bay codes from</param>
    /// <returns>Stock order details with bay codes for each product</returns>
    [HttpGet("{id}/docket")]
    public async Task<IActionResult> GetForDocket(Guid id, [FromQuery] Guid warehouseLocationId)
    {
        var result = await _stockOrderService.GetForDocketAsync(id, warehouseLocationId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock orders by site
    /// </summary>
    /// <param name="siteId">Site ID</param>
    /// <returns>List of stock orders for the site</returns>
    [HttpGet("by-site/{siteId}")]
    public async Task<IActionResult> GetBySite(Guid siteId)
    {
        var result = await _stockOrderService.GetBySiteAsync(siteId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get stock orders by status
    /// </summary>
    /// <param name="status">Status (Draft, PendingApproval, Approved, AwaitingPick, ReadyForCollection, Collected, Cancelled)</param>
    /// <returns>List of stock orders with the specified status</returns>
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var result = await _stockOrderService.GetByStatusAsync(status);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new stock order with lines
    /// </summary>
    /// <param name="dto">Stock order creation data including lines</param>
    /// <returns>Created stock order</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.CreateOrders")]
    public async Task<IActionResult> Create([FromBody] CreateStockOrderDto dto)
    {
        var result = await _stockOrderService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Submit a draft stock order for approval
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <returns>Updated stock order</returns>
    [HttpPost("{id}/submit")]
    [Authorize(Policy = "StockManagement.CreateOrders")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _stockOrderService.SubmitAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Approve a stock order (reserves stock)
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <param name="request">Approval request with approver name and warehouse location</param>
    /// <returns>Updated stock order</returns>
    [HttpPost("{id}/approve")]
    [Authorize(Policy = "StockManagement.ApproveOrders")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveStockOrderRequest request)
    {
        var result = await _stockOrderService.ApproveAsync(id, request.ApprovedBy, request.WarehouseLocationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject a stock order (returns to Draft status)
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <param name="request">Rejection request with rejector name and reason</param>
    /// <returns>Updated stock order</returns>
    [HttpPost("{id}/reject")]
    [Authorize(Policy = "StockManagement.ApproveOrders")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectStockOrderRequest request)
    {
        var result = await _stockOrderService.RejectAsync(id, request.RejectedBy, request.Reason);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mark a stock order as ready for collection
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <returns>Updated stock order</returns>
    [HttpPost("{id}/ready-for-collection")]
    [Authorize(Policy = "StockManagement.ReceiveGoods")]
    public async Task<IActionResult> ReadyForCollection(Guid id)
    {
        var result = await _stockOrderService.ReadyForCollectionAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Collect a stock order (issues stock)
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <param name="request">Collection request with warehouse location</param>
    /// <returns>Updated stock order</returns>
    [HttpPost("{id}/collect")]
    [Authorize(Policy = "StockManagement.ReceiveGoods")]
    public async Task<IActionResult> Collect(Guid id, [FromBody] CollectStockOrderRequest request)
    {
        var result = await _stockOrderService.CollectAsync(id, request.WarehouseLocationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a stock order (releases reserved stock if approved)
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <param name="request">Cancel request with optional warehouse location (required if order was approved)</param>
    /// <returns>Updated stock order</returns>
    [HttpPost("{id}/cancel")]
    [Authorize(Policy = "StockManagement.ApproveOrders")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelStockOrderRequest? request)
    {
        var result = await _stockOrderService.CancelAsync(id, request?.WarehouseLocationId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a stock order (soft delete, only Draft or Cancelled orders)
    /// </summary>
    /// <param name="id">Stock order ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.CreateOrders")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockOrderService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}

// Request DTOs for workflow actions
public record ApproveStockOrderRequest(string ApprovedBy, Guid WarehouseLocationId);
public record RejectStockOrderRequest(string RejectedBy, string Reason);
public record CollectStockOrderRequest(Guid WarehouseLocationId);
public record CancelStockOrderRequest(Guid? WarehouseLocationId);
