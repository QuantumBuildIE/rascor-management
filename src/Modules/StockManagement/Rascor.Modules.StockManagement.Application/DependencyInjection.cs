using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rascor.Modules.StockManagement.Application.Features.BayLocations;
using Rascor.Modules.StockManagement.Application.Features.Categories;
using Rascor.Modules.StockManagement.Application.Features.GoodsReceipts;
using Rascor.Modules.StockManagement.Application.Features.Products;
using Rascor.Modules.StockManagement.Application.Features.PurchaseOrders;
using Rascor.Modules.StockManagement.Application.Features.StockLevels;
using Rascor.Modules.StockManagement.Application.Features.StockLocations;
using Rascor.Modules.StockManagement.Application.Features.StockOrders;
using Rascor.Modules.StockManagement.Application.Features.Stocktakes;
using Rascor.Modules.StockManagement.Application.Features.StockTransactions;
using Rascor.Modules.StockManagement.Application.Features.Reports;
using Rascor.Modules.StockManagement.Application.Features.Suppliers;

namespace Rascor.Modules.StockManagement.Application;

/// <summary>
/// Dependency injection configuration for the Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register application services
        services.AddScoped<IBayLocationService, BayLocationService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IStockLevelService, StockLevelService>();
        services.AddScoped<IStockLocationService, StockLocationService>();
        services.AddScoped<IStockOrderService, StockOrderService>();
        services.AddScoped<IStocktakeService, StocktakeService>();
        services.AddScoped<IStockTransactionService, StockTransactionService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IStockReportService, StockReportService>();

        return services;
    }
}
