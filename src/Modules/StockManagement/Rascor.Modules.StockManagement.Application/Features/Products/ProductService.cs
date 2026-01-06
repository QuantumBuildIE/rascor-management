using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.Products.DTOs;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Features.Products;

public class ProductService : IProductService
{
    private readonly IStockManagementDbContext _context;

    public ProductService(IStockManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductDto>>> GetAllAsync()
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.ProductCode)
                .Select(p => new ProductDto(
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.CategoryId,
                    p.Category.CategoryName,
                    p.SupplierId,
                    p.Supplier != null ? p.Supplier.SupplierName : null,
                    p.UnitType,
                    p.BaseRate,
                    p.ReorderLevel,
                    p.ReorderQuantity,
                    p.LeadTimeDays,
                    p.IsActive,
                    p.QrCodeData,
                    p.CostPrice,
                    p.SellPrice,
                    p.ProductType,
                    p.SellPrice.HasValue && p.CostPrice.HasValue ? p.SellPrice.Value - p.CostPrice.Value : (decimal?)null,
                    p.SellPrice.HasValue && p.CostPrice.HasValue && p.SellPrice.Value != 0
                        ? Math.Round((p.SellPrice.Value - p.CostPrice.Value) / p.SellPrice.Value * 100, 2)
                        : (decimal?)null,
                    p.ImageFileName,
                    p.ImageUrl
                ))
                .ToListAsync();

            return Result.Ok(products);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<ProductDto>>($"Error retrieving products: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<ProductDto>>> GetPaginatedAsync(GetProductsQueryDto query)
    {
        try
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchLower = query.Search.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.ProductCode.ToLower().Contains(searchLower) ||
                    p.ProductName.ToLower().Contains(searchLower) ||
                    p.Category.CategoryName.ToLower().Contains(searchLower) ||
                    (p.Supplier != null && p.Supplier.SupplierName.ToLower().Contains(searchLower))
                );
            }

            // Apply sorting
            productsQuery = ApplySorting(productsQuery, query.SortColumn, query.SortDirection);

            // Get total count before pagination
            var totalCount = await productsQuery.CountAsync();

            // Apply pagination
            var products = await productsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new ProductDto(
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.CategoryId,
                    p.Category.CategoryName,
                    p.SupplierId,
                    p.Supplier != null ? p.Supplier.SupplierName : null,
                    p.UnitType,
                    p.BaseRate,
                    p.ReorderLevel,
                    p.ReorderQuantity,
                    p.LeadTimeDays,
                    p.IsActive,
                    p.QrCodeData,
                    p.CostPrice,
                    p.SellPrice,
                    p.ProductType,
                    p.SellPrice.HasValue && p.CostPrice.HasValue ? p.SellPrice.Value - p.CostPrice.Value : (decimal?)null,
                    p.SellPrice.HasValue && p.CostPrice.HasValue && p.SellPrice.Value != 0
                        ? Math.Round((p.SellPrice.Value - p.CostPrice.Value) / p.SellPrice.Value * 100, 2)
                        : (decimal?)null,
                    p.ImageFileName,
                    p.ImageUrl
                ))
                .ToListAsync();

            var result = new PaginatedList<ProductDto>(products, totalCount, query.PageNumber, query.PageSize);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<PaginatedList<ProductDto>>($"Error retrieving products: {ex.Message}");
        }
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortColumn, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn?.ToLower() switch
        {
            "productcode" => isDescending ? query.OrderByDescending(p => p.ProductCode) : query.OrderBy(p => p.ProductCode),
            "productname" => isDescending ? query.OrderByDescending(p => p.ProductName) : query.OrderBy(p => p.ProductName),
            "categoryname" => isDescending ? query.OrderByDescending(p => p.Category.CategoryName) : query.OrderBy(p => p.Category.CategoryName),
            "suppliername" => isDescending ? query.OrderByDescending(p => p.Supplier != null ? p.Supplier.SupplierName : "") : query.OrderBy(p => p.Supplier != null ? p.Supplier.SupplierName : ""),
            "unittype" => isDescending ? query.OrderByDescending(p => p.UnitType) : query.OrderBy(p => p.UnitType),
            "baserate" => isDescending ? query.OrderByDescending(p => p.BaseRate) : query.OrderBy(p => p.BaseRate),
            "isactive" => isDescending ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
            _ => query.OrderBy(p => p.ProductCode)
        };
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto(
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.CategoryId,
                    p.Category.CategoryName,
                    p.SupplierId,
                    p.Supplier != null ? p.Supplier.SupplierName : null,
                    p.UnitType,
                    p.BaseRate,
                    p.ReorderLevel,
                    p.ReorderQuantity,
                    p.LeadTimeDays,
                    p.IsActive,
                    p.QrCodeData,
                    p.CostPrice,
                    p.SellPrice,
                    p.ProductType,
                    p.SellPrice.HasValue && p.CostPrice.HasValue ? p.SellPrice.Value - p.CostPrice.Value : (decimal?)null,
                    p.SellPrice.HasValue && p.CostPrice.HasValue && p.SellPrice.Value != 0
                        ? Math.Round((p.SellPrice.Value - p.CostPrice.Value) / p.SellPrice.Value * 100, 2)
                        : (decimal?)null,
                    p.ImageFileName,
                    p.ImageUrl
                ))
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return Result.Fail<ProductDto>($"Product with ID {id} not found");
            }

            return Result.Ok(product);
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductDto>($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        try
        {
            // Validate that CategoryId exists
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return Result.Fail<ProductDto>($"Category with ID {dto.CategoryId} not found");
            }

            // Validate that SupplierId exists if provided
            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.Id == dto.SupplierId.Value);

                if (!supplierExists)
                {
                    return Result.Fail<ProductDto>($"Supplier with ID {dto.SupplierId} not found");
                }
            }

            // Check for duplicate ProductCode within the same tenant
            var duplicateCode = await _context.Products
                .AnyAsync(p => p.ProductCode == dto.ProductCode);

            if (duplicateCode)
            {
                return Result.Fail<ProductDto>($"Product with code '{dto.ProductCode}' already exists");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductCode = dto.ProductCode,
                ProductName = dto.ProductName,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId,
                UnitType = dto.UnitType,
                BaseRate = dto.BaseRate,
                ReorderLevel = dto.ReorderLevel,
                ReorderQuantity = dto.ReorderQuantity,
                LeadTimeDays = dto.LeadTimeDays,
                IsActive = dto.IsActive,
                CostPrice = dto.CostPrice,
                SellPrice = dto.SellPrice,
                ProductType = dto.ProductType
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Reload with related entities to get names
            var createdProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstAsync(p => p.Id == product.Id);

            var productDto = new ProductDto(
                createdProduct.Id,
                createdProduct.ProductCode,
                createdProduct.ProductName,
                createdProduct.CategoryId,
                createdProduct.Category.CategoryName,
                createdProduct.SupplierId,
                createdProduct.Supplier?.SupplierName,
                createdProduct.UnitType,
                createdProduct.BaseRate,
                createdProduct.ReorderLevel,
                createdProduct.ReorderQuantity,
                createdProduct.LeadTimeDays,
                createdProduct.IsActive,
                createdProduct.QrCodeData,
                createdProduct.CostPrice,
                createdProduct.SellPrice,
                createdProduct.ProductType,
                createdProduct.SellPrice.HasValue && createdProduct.CostPrice.HasValue
                    ? createdProduct.SellPrice.Value - createdProduct.CostPrice.Value
                    : null,
                createdProduct.SellPrice.HasValue && createdProduct.CostPrice.HasValue && createdProduct.SellPrice.Value != 0
                    ? Math.Round((createdProduct.SellPrice.Value - createdProduct.CostPrice.Value) / createdProduct.SellPrice.Value * 100, 2)
                    : null,
                createdProduct.ImageFileName,
                createdProduct.ImageUrl
            );

            return Result.Ok(productDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductDto>($"Error creating product: {ex.Message}");
        }
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Result.Fail<ProductDto>($"Product with ID {id} not found");
            }

            // Validate that CategoryId exists
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return Result.Fail<ProductDto>($"Category with ID {dto.CategoryId} not found");
            }

            // Validate that SupplierId exists if provided
            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.Id == dto.SupplierId.Value);

                if (!supplierExists)
                {
                    return Result.Fail<ProductDto>($"Supplier with ID {dto.SupplierId} not found");
                }
            }

            // Check for duplicate ProductCode (excluding current product)
            var duplicateCode = await _context.Products
                .AnyAsync(p => p.ProductCode == dto.ProductCode && p.Id != id);

            if (duplicateCode)
            {
                return Result.Fail<ProductDto>($"Product with code '{dto.ProductCode}' already exists");
            }

            product.ProductCode = dto.ProductCode;
            product.ProductName = dto.ProductName;
            product.CategoryId = dto.CategoryId;
            product.SupplierId = dto.SupplierId;
            product.UnitType = dto.UnitType;
            product.BaseRate = dto.BaseRate;
            product.ReorderLevel = dto.ReorderLevel;
            product.ReorderQuantity = dto.ReorderQuantity;
            product.LeadTimeDays = dto.LeadTimeDays;
            product.IsActive = dto.IsActive;
            product.CostPrice = dto.CostPrice;
            product.SellPrice = dto.SellPrice;
            product.ProductType = dto.ProductType;

            await _context.SaveChangesAsync();

            // Reload to get updated related entity names
            await _context.Products
                .Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            if (product.SupplierId.HasValue)
            {
                await _context.Products
                    .Entry(product)
                    .Reference(p => p.Supplier)
                    .LoadAsync();
            }

            var productDto = new ProductDto(
                product.Id,
                product.ProductCode,
                product.ProductName,
                product.CategoryId,
                product.Category.CategoryName,
                product.SupplierId,
                product.Supplier?.SupplierName,
                product.UnitType,
                product.BaseRate,
                product.ReorderLevel,
                product.ReorderQuantity,
                product.LeadTimeDays,
                product.IsActive,
                product.QrCodeData,
                product.CostPrice,
                product.SellPrice,
                product.ProductType,
                product.SellPrice.HasValue && product.CostPrice.HasValue
                    ? product.SellPrice.Value - product.CostPrice.Value
                    : null,
                product.SellPrice.HasValue && product.CostPrice.HasValue && product.SellPrice.Value != 0
                    ? Math.Round((product.SellPrice.Value - product.CostPrice.Value) / product.SellPrice.Value * 100, 2)
                    : null,
                product.ImageFileName,
                product.ImageUrl
            );

            return Result.Ok(productDto);
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductDto>($"Error updating product: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Result.Fail($"Product with ID {id} not found");
            }

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting product: {ex.Message}");
        }
    }
}
