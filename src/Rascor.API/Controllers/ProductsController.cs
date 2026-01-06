using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rascor.Modules.StockManagement.Application.Features.Products;
using Rascor.Modules.StockManagement.Application.Features.Products.DTOs;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;

namespace Rascor.StockManagement.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = "StockManagement.View")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStockManagementDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IStockManagementDbContext context,
        IWebHostEnvironment environment,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Get all products (non-paginated)
    /// </summary>
    /// <returns>List of products</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get products with pagination, sorting, and search
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="sortColumn">Column to sort by</param>
    /// <param name="sortDirection">Sort direction (asc/desc)</param>
    /// <param name="search">Search term</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortDirection = null,
        [FromQuery] string? search = null)
    {
        var query = new GetProductsQueryDto(pageNumber, pageSize, sortColumn, sortDirection, search);
        var result = await _productService.GetPaginatedAsync(query);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="dto">Product creation data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Upload an image for a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="file">Image file</param>
    /// <returns>Updated product with image URL</returns>
    [HttpPost("{id:guid}/image")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "No file provided" });
            }

            // Validate file size (5MB max)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { Success = false, Message = "File size exceeds 5MB limit" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { Success = false, Message = "Invalid file type. Allowed: jpg, jpeg, png, webp" });
            }

            // Get product
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Success = false, Message = $"Product with ID {id} not found" });
            }

            // Delete old image if exists
            if (!string.IsNullOrEmpty(product.ImageFileName))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, "uploads", "products", product.ImageFileName);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Generate unique filename
            var timestamp = DateTime.UtcNow.Ticks;
            var fileName = $"{id}_{timestamp}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

            // Ensure directory exists
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update product
            product.ImageFileName = fileName;
            product.ImageUrl = $"/uploads/products/{fileName}";

            await _context.SaveChangesAsync();

            // Return updated product
            var result = await _productService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for product {ProductId}", id);
            return StatusCode(500, new { Success = false, Message = "Error uploading image" });
        }
    }

    /// <summary>
    /// Delete product image
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Updated product without image</returns>
    [HttpDelete("{id:guid}/image")]
    [Authorize(Policy = "StockManagement.ManageProducts")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Success = false, Message = $"Product with ID {id} not found" });
            }

            if (string.IsNullOrEmpty(product.ImageFileName))
            {
                return BadRequest(new { Success = false, Message = "Product has no image" });
            }

            // Delete physical file
            var imagePath = Path.Combine(_environment.WebRootPath, "uploads", "products", product.ImageFileName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // Clear image fields
            product.ImageFileName = null;
            product.ImageUrl = null;

            await _context.SaveChangesAsync();

            // Return updated product
            var result = await _productService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image for product {ProductId}", id);
            return StatusCode(500, new { Success = false, Message = "Error deleting image" });
        }
    }
}
