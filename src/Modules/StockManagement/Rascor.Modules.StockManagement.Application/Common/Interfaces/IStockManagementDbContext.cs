using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Modules.StockManagement.Application.Common.Interfaces;

/// <summary>
/// Interface for the application database context
/// </summary>
public interface IStockManagementDbContext
{
    // DbSets
    DbSet<Category> Categories { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<StockLocation> StockLocations { get; }
    DbSet<BayLocation> BayLocations { get; }
    DbSet<StockLevel> StockLevels { get; }
    DbSet<StockTransaction> StockTransactions { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptLine> GoodsReceiptLines { get; }
    DbSet<StockOrder> StockOrders { get; }
    DbSet<StockOrderLine> StockOrderLines { get; }
    DbSet<Stocktake> Stocktakes { get; }
    DbSet<StocktakeLine> StocktakeLines { get; }
    DbSet<ProductKit> ProductKits { get; }
    DbSet<ProductKitItem> ProductKitItems { get; }

    /// <summary>
    /// Save changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
