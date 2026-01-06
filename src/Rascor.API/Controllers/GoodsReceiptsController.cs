using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts.DTOs;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/goods-receipts")]
[Authorize(Policy = "StockManagement.View")]
public class GoodsReceiptsController : ControllerBase
{
    private readonly IGoodsReceiptService _goodsReceiptService;

    public GoodsReceiptsController(IGoodsReceiptService goodsReceiptService)
    {
        _goodsReceiptService = goodsReceiptService;
    }

    /// <summary>
    /// Get all goods receipts
    /// </summary>
    /// <returns>List of goods receipts</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _goodsReceiptService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a goods receipt by ID
    /// </summary>
    /// <param name="id">Goods receipt ID</param>
    /// <returns>Goods receipt details with lines</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _goodsReceiptService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get goods receipts by supplier
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>List of goods receipts from the supplier</returns>
    [HttpGet("by-supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(Guid supplierId)
    {
        var result = await _goodsReceiptService.GetBySupplierAsync(supplierId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get goods receipts by purchase order
    /// </summary>
    /// <param name="purchaseOrderId">Purchase order ID</param>
    /// <returns>List of goods receipts for the purchase order</returns>
    [HttpGet("by-po/{purchaseOrderId}")]
    public async Task<IActionResult> GetByPurchaseOrder(Guid purchaseOrderId)
    {
        var result = await _goodsReceiptService.GetByPurchaseOrderAsync(purchaseOrderId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new goods receipt with lines
    /// </summary>
    /// <param name="dto">Goods receipt creation data including lines</param>
    /// <returns>Created goods receipt</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.ReceiveGoods")]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptDto dto)
    {
        var result = await _goodsReceiptService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Delete a goods receipt (soft delete)
    /// </summary>
    /// <param name="id">Goods receipt ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "StockManagement.ReceiveGoods")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _goodsReceiptService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }
}
