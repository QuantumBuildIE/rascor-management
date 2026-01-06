using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Enums;

namespace Rascor.Core.Infrastructure.Persistence;

/// <summary>
/// Seeds test data for Stock Management module
/// </summary>
public static class StockManagementSeeder
{
    private static readonly Guid TenantId = DataSeeder.DefaultTenantId;

    /// <summary>
    /// Seed stock management test data if database is empty
    /// </summary>
    public static async Task SeedAsync(DbContext context, ILogger logger)
    {
        // Check if base data already exists
        var hasBaseData = await context.Set<Category>().IgnoreQueryFilters().AnyAsync();

        if (!hasBaseData)
        {
            logger.LogInformation("Seeding stock management test data...");

            // Seed in order to respect foreign keys
            var categories = await SeedCategoriesAsync(context, logger);
            var suppliers = await SeedSuppliersAsync(context, logger);
            var locations = await SeedStockLocationsAsync(context, logger);
            var bayLocations = await SeedBayLocationsAsync(context, locations[0], logger);
            var products = await SeedProductsAsync(context, categories, suppliers, logger);
            await SeedProductKitsAsync(context, products, categories, logger);
            await SeedStockLevelsAsync(context, products, locations[0], bayLocations, logger);
            await SeedSampleTransactionsAsync(context, products, locations[0], logger);
            await SeedSamplePurchaseOrderAsync(context, suppliers[0], products, logger);
            await SeedSampleStockOrderAsync(context, products, locations, logger);
            await SeedHistoricalCollectedOrdersAsync(context, products, locations, logger);

            logger.LogInformation("Stock management test data seeding completed");
        }
        else
        {
            logger.LogInformation("Stock management base data already exists");

            // Still try to seed historical data if not present
            var products = await context.Set<Product>().IgnoreQueryFilters().ToListAsync();
            var locations = await context.Set<StockLocation>().IgnoreQueryFilters().ToListAsync();

            if (products.Count > 0 && locations.Count > 0)
            {
                await SeedHistoricalCollectedOrdersAsync(context, products, locations, logger);
            }
        }
    }

    private static async Task<List<Category>> SeedCategoriesAsync(DbContext context, ILogger logger)
    {
        var categories = new List<Category>
        {
            CreateCategory("Adhesives", 1),
            CreateCategory("Sealants", 2),
            CreateCategory("Waterproofing", 3),
            CreateCategory("Drainage", 4),
            CreateCategory("Insulation", 5),
            CreateCategory("Fixings", 6),
            CreateCategory("Tools", 7),
            CreateCategory("Safety Equipment", 8),
            CreateCategory("Concrete", 9),
            CreateCategory("General Materials", 10)
        };

        await context.Set<Category>().AddRangeAsync(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} categories", categories.Count);

        return categories;
    }

    private static async Task<List<Supplier>> SeedSuppliersAsync(DbContext context, ILogger logger)
    {
        var suppliers = new List<Supplier>
        {
            CreateSupplier("SUP001", "SIKA Ireland", "John Murphy", "sales@sika.ie", "+353 1 234 5678", "Unit 5, Industrial Estate, Dublin", "Net 30"),
            CreateSupplier("SUP002", "JUTA UK", "Sarah Johnson", "orders@juta.co.uk", "+44 20 7946 0958", "Birmingham Business Park, B1 2AB", "Net 45"),
            CreateSupplier("SUP003", "Bostik", "Michael O'Brien", "trade@bostik.com", "+353 1 456 7890", "Cork Industrial Park, Cork", "Net 30"),
            CreateSupplier("SUP004", "Dulux Trade", "Emma Walsh", "trade@dulux.ie", "+353 1 567 8901", "Blanchardstown, Dublin 15", "Net 30"),
            CreateSupplier("SUP005", "Screwfix", "Trade Desk", "trade@screwfix.ie", "+353 1 678 9012", "Citywest Business Park, Dublin 24", "COD")
        };

        await context.Set<Supplier>().AddRangeAsync(suppliers);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} suppliers", suppliers.Count);

        return suppliers;
    }

    private static async Task<List<StockLocation>> SeedStockLocationsAsync(DbContext context, ILogger logger)
    {
        var locations = new List<StockLocation>
        {
            CreateStockLocation("WH001", "Main Warehouse", LocationType.Warehouse, "Unit 10, Rascor Industrial Park, Dublin 12"),
            CreateStockLocation("SS001", "Site Store Alpha", LocationType.SiteStore, "Construction Site A, Sandyford, Dublin 18"),
            CreateStockLocation("VAN001", "Van Stock 1", LocationType.VanStock, null)
        };

        await context.Set<StockLocation>().AddRangeAsync(locations);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} stock locations", locations.Count);

        return locations;
    }

    private static async Task<List<BayLocation>> SeedBayLocationsAsync(DbContext context, StockLocation warehouse, ILogger logger)
    {
        var bayLocations = new List<BayLocation>
        {
            CreateBayLocation("A-1", "Aisle A, Bay 1 - Adhesives & Sealants", warehouse.Id, 100),
            CreateBayLocation("A-2", "Aisle A, Bay 2 - Adhesives & Sealants", warehouse.Id, 100),
            CreateBayLocation("A-3", "Aisle A, Bay 3 - Waterproofing", warehouse.Id, 80),
            CreateBayLocation("B-1", "Aisle B, Bay 1 - Drainage & Concrete", warehouse.Id, 150),
            CreateBayLocation("B-2", "Aisle B, Bay 2 - Insulation", warehouse.Id, 120),
            CreateBayLocation("B-3", "Aisle B, Bay 3 - Insulation", warehouse.Id, 120),
            CreateBayLocation("C-1", "Aisle C, Bay 1 - Fixings & Tools", warehouse.Id, 200),
            CreateBayLocation("C-2", "Aisle C, Bay 2 - Safety Equipment", warehouse.Id, 150)
        };

        await context.Set<BayLocation>().AddRangeAsync(bayLocations);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} bay locations", bayLocations.Count);

        return bayLocations;
    }

    private static async Task<List<Product>> SeedProductsAsync(DbContext context, List<Category> categories, List<Supplier> suppliers, ILogger logger)
    {
        var catDict = categories.ToDictionary(c => c.CategoryName, c => c.Id);
        var supDict = suppliers.ToDictionary(s => s.SupplierCode, s => s.Id);

        var products = new List<Product>
        {
            // Adhesives (ADH)
            CreateProduct("ADH001", "SIKA Primer MB", catDict["Adhesives"], supDict["SUP001"], "Litre", 45.00m, 20, 50),
            CreateProduct("ADH002", "SIKA Flex 11FC", catDict["Adhesives"], supDict["SUP001"], "Tube", 12.50m, 30, 100),
            CreateProduct("ADH003", "Bostik Grip N Grab", catDict["Adhesives"], supDict["SUP003"], "Tube", 8.99m, 25, 75),

            // Sealants (SEA)
            CreateProduct("SEA001", "SIKA Sikasil SG-500", catDict["Sealants"], supDict["SUP001"], "Tube", 18.50m, 20, 60),
            CreateProduct("SEA002", "Bostik Fire Retardant Sealant", catDict["Sealants"], supDict["SUP003"], "Tube", 15.99m, 15, 40),

            // Waterproofing (WPF)
            CreateProduct("WPF001", "Waterguard Membrane 1.5mm", catDict["Waterproofing"], supDict["SUP002"], "Roll", 85.00m, 10, 25),
            CreateProduct("WPF002", "SIKA Sikalastic 152", catDict["Waterproofing"], supDict["SUP001"], "20L Pail", 125.00m, 10, 20),
            CreateProduct("WPF003", "JUTA Radon Barrier", catDict["Waterproofing"], supDict["SUP002"], "Roll", 145.00m, 10, 15),

            // Drainage (DRN)
            CreateProduct("DRN001", "110mm Underground Drainage Pipe 3m", catDict["Drainage"], supDict["SUP005"], "Length", 22.50m, 30, 50),
            CreateProduct("DRN002", "Drainage Channel 1m", catDict["Drainage"], supDict["SUP005"], "Each", 35.00m, 15, 30),

            // Insulation (INS)
            CreateProduct("INS001", "Kingspan K8 50mm", catDict["Insulation"], supDict["SUP005"], "Board", 28.00m, 40, 100),
            CreateProduct("INS002", "Rockwool Flexi 100mm", catDict["Insulation"], supDict["SUP005"], "Pack", 42.00m, 20, 50),

            // Fixings (FIX)
            CreateProduct("FIX001", "M10 Anchor Bolt 100mm", catDict["Fixings"], supDict["SUP005"], "Box", 18.50m, 30, 75),
            CreateProduct("FIX002", "Frame Fixings 8x100mm", catDict["Fixings"], supDict["SUP005"], "Box", 12.99m, 40, 100),
            CreateProduct("FIX003", "Heavy Duty Wall Plugs Brown", catDict["Fixings"], supDict["SUP005"], "Pack", 5.50m, 50, 150),

            // Tools (TLS)
            CreateProduct("TLS001", "Stanley FatMax Tape 8m", catDict["Tools"], supDict["SUP005"], "Each", 22.00m, 10, 25),
            CreateProduct("TLS002", "DeWalt Hammer Drill Bit Set", catDict["Tools"], supDict["SUP005"], "Set", 65.00m, 10, 20),

            // Safety Equipment (SAF)
            CreateProduct("SAF001", "Hard Hat - Yellow", catDict["Safety Equipment"], supDict["SUP005"], "Each", 8.50m, 20, 50),
            CreateProduct("SAF002", "Hi-Vis Vest Orange XL", catDict["Safety Equipment"], supDict["SUP005"], "Each", 5.99m, 30, 75),

            // Concrete (CON)
            CreateProduct("CON001", "Ready Mix Concrete 25kg", catDict["Concrete"], supDict["SUP005"], "Bag", 6.50m, 50, 150)
        };

        await context.Set<Product>().AddRangeAsync(products);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} products", products.Count);

        return products;
    }

    private static async Task SeedProductKitsAsync(DbContext context, List<Product> products, List<Category> categories, ILogger logger)
    {
        // Create a product lookup by product code for easy reference
        var productDict = products.ToDictionary(p => p.ProductCode, p => p);
        var catDict = categories.ToDictionary(c => c.CategoryName, c => c.Id);

        var productKits = new List<ProductKit>
        {
            // Waterproofing Membrane Kit
            CreateProductKit(
                "KIT-WRAP-001",
                "Waterproofing Membrane Kit",
                "Complete kit for waterproofing membrane installation including membrane, adhesive, and tape",
                catDict["Waterproofing"]
            ),

            // Damp Proofing Injection Kit
            CreateProductKit(
                "KIT-DPI-001",
                "Damp Proofing Injection Kit",
                "Damp proof course injection treatment kit with cream, nozzles, drill bit, and safety equipment",
                catDict["Waterproofing"]
            ),

            // Basement Tanking Kit
            CreateProductKit(
                "KIT-TANK-001",
                "Basement Tanking Kit",
                "Complete basement tanking and waterproofing solution with slurry, primer, and reinforcement",
                catDict["Waterproofing"]
            ),

            // Floor Sealing Kit
            CreateProductKit(
                "KIT-FLOOR-001",
                "Floor Sealing Kit",
                "Floor preparation and sealing kit with sealer, primer, and application tools",
                catDict["Concrete"]
            )
        };

        await context.Set<ProductKit>().AddRangeAsync(productKits);
        await context.SaveChangesAsync();

        // Create kit items (components of each kit)
        var kitItems = new List<ProductKitItem>();

        // Waterproofing Membrane Kit items
        var waterproofingKit = productKits.First(k => k.KitCode == "KIT-WRAP-001");
        if (productDict.ContainsKey("WPF001")) // Waterguard Membrane 1.5mm
        {
            kitItems.Add(CreateProductKitItem(waterproofingKit.Id, productDict["WPF001"].Id, 1, 1, "Main waterproofing membrane roll"));
        }
        if (productDict.ContainsKey("ADH001")) // SIKA Primer MB
        {
            kitItems.Add(CreateProductKitItem(waterproofingKit.Id, productDict["ADH001"].Id, 2, 2, "Adhesive paste - use 2 for large areas"));
        }
        if (productDict.ContainsKey("SEA001")) // SIKA Sikasil SG-500
        {
            kitItems.Add(CreateProductKitItem(waterproofingKit.Id, productDict["SEA001"].Id, 1, 3, "Corner and joint sealant"));
        }

        // Damp Proofing Injection Kit items
        var dampProofingKit = productKits.First(k => k.KitCode == "KIT-DPI-001");
        if (productDict.ContainsKey("WPF002")) // SIKA Sikalastic 152 (DPC Injection Cream)
        {
            kitItems.Add(CreateProductKitItem(dampProofingKit.Id, productDict["WPF002"].Id, 1, 1, "DPC injection cream"));
        }
        if (productDict.ContainsKey("FIX001")) // Anchor bolts (representing injection nozzles)
        {
            kitItems.Add(CreateProductKitItem(dampProofingKit.Id, productDict["FIX001"].Id, 1, 2, "Injection nozzles and fittings"));
        }
        if (productDict.ContainsKey("TLS002")) // DeWalt Hammer Drill Bit Set
        {
            kitItems.Add(CreateProductKitItem(dampProofingKit.Id, productDict["TLS002"].Id, 1, 3, "Drill bit set for injection holes"));
        }
        if (productDict.ContainsKey("SAF001")) // Hard Hat / Safety goggles
        {
            kitItems.Add(CreateProductKitItem(dampProofingKit.Id, productDict["SAF001"].Id, 1, 4, "Safety goggles"));
        }

        // Basement Tanking Kit items
        var tankingKit = productKits.First(k => k.KitCode == "KIT-TANK-001");
        if (productDict.ContainsKey("WPF002")) // SIKA Sikalastic 152 (Tanking Slurry)
        {
            kitItems.Add(CreateProductKitItem(tankingKit.Id, productDict["WPF002"].Id, 2, 1, "Tanking slurry - 2 units for full coverage"));
        }
        if (productDict.ContainsKey("ADH001")) // SIKA Primer MB
        {
            kitItems.Add(CreateProductKitItem(tankingKit.Id, productDict["ADH001"].Id, 1, 2, "Primer"));
        }
        if (productDict.ContainsKey("SEA001")) // Sealant (Reinforcement Tape alternative)
        {
            kitItems.Add(CreateProductKitItem(tankingKit.Id, productDict["SEA001"].Id, 1, 3, "Reinforcement sealant for corners"));
        }
        if (productDict.ContainsKey("TLS001")) // Stanley FatMax Tape (Mixing Paddle alternative)
        {
            kitItems.Add(CreateProductKitItem(tankingKit.Id, productDict["TLS001"].Id, 1, 4, "Measuring tape"));
        }

        // Floor Sealing Kit items
        var floorSealingKit = productKits.First(k => k.KitCode == "KIT-FLOOR-001");
        if (productDict.ContainsKey("CON001")) // Ready Mix Concrete (Floor Sealer alternative)
        {
            kitItems.Add(CreateProductKitItem(floorSealingKit.Id, productDict["CON001"].Id, 5, 1, "Floor repair concrete"));
        }
        if (productDict.ContainsKey("ADH001")) // SIKA Primer MB
        {
            kitItems.Add(CreateProductKitItem(floorSealingKit.Id, productDict["ADH001"].Id, 2, 2, "Floor primer"));
        }
        if (productDict.ContainsKey("TLS001")) // Stanley FatMax Tape (Roller alternative)
        {
            kitItems.Add(CreateProductKitItem(floorSealingKit.Id, productDict["TLS001"].Id, 1, 3, "Measuring tape for application"));
        }

        await context.Set<ProductKitItem>().AddRangeAsync(kitItems);
        await context.SaveChangesAsync();

        // Calculate and update total costs/prices for each kit
        foreach (var kit in productKits)
        {
            var items = kitItems.Where(ki => ki.ProductKitId == kit.Id).ToList();
            kit.TotalCost = items.Sum(ki =>
            {
                var product = products.First(p => p.Id == ki.ProductId);
                return (product.CostPrice ?? product.BaseRate) * ki.DefaultQuantity;
            });
            kit.TotalPrice = items.Sum(ki =>
            {
                var product = products.First(p => p.Id == ki.ProductId);
                return (product.SellPrice ?? product.BaseRate * 1.3m) * ki.DefaultQuantity; // 30% markup if no sell price
            });
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} product kits with {ItemCount} items", productKits.Count, kitItems.Count);
    }

    private static async Task SeedStockLevelsAsync(DbContext context, List<Product> products, StockLocation warehouse, List<BayLocation> bayLocations, ILogger logger)
    {
        var random = new Random(42); // Fixed seed for reproducible test data
        var stockLevels = new List<StockLevel>();

        // Create bay lookup by code for easy assignment
        var bayDict = bayLocations.ToDictionary(b => b.BayCode, b => b.Id);

        foreach (var product in products)
        {
            // Assign bay based on product category/code prefix
            Guid? bayLocationId = product.ProductCode switch
            {
                var code when code.StartsWith("ADH") => bayDict["A-1"],
                var code when code.StartsWith("SEA") => bayDict["A-2"],
                var code when code.StartsWith("WPF") => bayDict["A-3"],
                var code when code.StartsWith("DRN") => bayDict["B-1"],
                var code when code.StartsWith("CON") => bayDict["B-1"],
                var code when code.StartsWith("INS") => bayDict["B-2"],
                var code when code.StartsWith("FIX") => bayDict["C-1"],
                var code when code.StartsWith("TLS") => bayDict["C-1"],
                var code when code.StartsWith("SAF") => bayDict["C-2"],
                _ => null
            };

            var stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                ProductId = product.Id,
                LocationId = warehouse.Id,
                QuantityOnHand = random.Next(20, 201),
                QuantityReserved = 0,
                QuantityOnOrder = 0,
                BinLocation = null, // No longer using BinLocation, using BayLocationId instead
                BayLocationId = bayLocationId,
                LastMovementDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                LastCountDate = DateTime.UtcNow.AddDays(-random.Next(30, 90)),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            stockLevels.Add(stockLevel);
        }

        await context.Set<StockLevel>().AddRangeAsync(stockLevels);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} stock levels", stockLevels.Count);
    }

    private static async Task SeedSampleTransactionsAsync(DbContext context, List<Product> products, StockLocation warehouse, ILogger logger)
    {
        var transactions = new List<StockTransaction>();
        var transactionNumber = 1;

        // Add some GRN receipts for first 5 products
        foreach (var product in products.Take(5))
        {
            transactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                TransactionNumber = $"TXN{transactionNumber++:D6}",
                TransactionDate = DateTime.UtcNow.AddDays(-15),
                TransactionType = TransactionType.GrnReceipt,
                ProductId = product.Id,
                LocationId = warehouse.Id,
                Quantity = 50,
                ReferenceType = "InitialStock",
                Notes = "Initial stock receipt",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            });
        }

        // Add some order issues for next 3 products
        foreach (var product in products.Skip(5).Take(3))
        {
            transactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                TransactionNumber = $"TXN{transactionNumber++:D6}",
                TransactionDate = DateTime.UtcNow.AddDays(-7),
                TransactionType = TransactionType.OrderIssue,
                ProductId = product.Id,
                LocationId = warehouse.Id,
                Quantity = -10,
                ReferenceType = "StockOrder",
                Notes = "Stock issued to site",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            });
        }

        await context.Set<StockTransaction>().AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} stock transactions", transactions.Count);
    }

    private static async Task SeedSamplePurchaseOrderAsync(DbContext context, Supplier supplier, List<Product> products, ILogger logger)
    {
        var po = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            PoNumber = "PO-2024-001",
            SupplierId = supplier.Id,
            OrderDate = DateTime.UtcNow.AddDays(-10),
            ExpectedDate = DateTime.UtcNow.AddDays(5),
            Status = PurchaseOrderStatus.Confirmed,
            Notes = "Monthly restock order",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        // Add lines for SIKA products (first 4 products belong to SIKA)
        var sikaProducts = products.Where(p => p.ProductCode.StartsWith("ADH") || p.ProductCode.StartsWith("SEA") || p.ProductCode == "WPF002").Take(4).ToList();
        decimal totalValue = 0;

        foreach (var product in sikaProducts)
        {
            var qty = 25;
            var line = new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                PurchaseOrderId = po.Id,
                ProductId = product.Id,
                QuantityOrdered = qty,
                QuantityReceived = 0,
                UnitPrice = product.BaseRate,
                LineStatus = PurchaseOrderLineStatus.Open,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            po.Lines.Add(line);
            totalValue += qty * product.BaseRate;
        }

        po.TotalValue = totalValue;

        await context.Set<PurchaseOrder>().AddAsync(po);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded 1 sample purchase order with {LineCount} lines", po.Lines.Count);
    }

    private static async Task SeedSampleStockOrderAsync(DbContext context, List<Product> products, List<StockLocation> locations, ILogger logger)
    {
        // Use one of the seeded sites
        var siteId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Use the main warehouse as the source location
        var warehouse = locations[0];

        var stockOrder = new StockOrder
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            OrderNumber = "SO-2024-001",
            SiteId = siteId,
            SiteName = "Quantum Build",
            OrderDate = DateTime.UtcNow.AddDays(-3),
            RequiredDate = DateTime.UtcNow.AddDays(2),
            Status = StockOrderStatus.Approved,
            RequestedBy = "John Smith",
            ApprovedBy = "System Administrator",
            ApprovedDate = DateTime.UtcNow.AddDays(-2),
            Notes = "Urgent - needed for foundation work",
            SourceLocationId = warehouse.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        // Add lines for various products
        var orderProducts = products.Where(p =>
            p.ProductCode == "CON001" ||
            p.ProductCode == "FIX001" ||
            p.ProductCode == "SAF001" ||
            p.ProductCode == "SAF002").ToList();

        decimal totalValue = 0;
        var quantities = new[] { 20, 5, 10, 10 };
        var i = 0;

        foreach (var product in orderProducts)
        {
            var qty = quantities[i++];
            var line = new StockOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                StockOrderId = stockOrder.Id,
                ProductId = product.Id,
                QuantityRequested = qty,
                QuantityIssued = 0,
                UnitPrice = product.BaseRate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            stockOrder.Lines.Add(line);
            totalValue += qty * product.BaseRate;
        }

        stockOrder.OrderTotal = totalValue;

        await context.Set<StockOrder>().AddAsync(stockOrder);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded 1 sample stock order with {LineCount} lines", stockOrder.Lines.Count);
    }

    private static async Task SeedHistoricalCollectedOrdersAsync(DbContext context, List<Product> products, List<StockLocation> locations, ILogger logger)
    {
        // Check if historical data already exists
        if (await context.Set<StockOrder>().IgnoreQueryFilters().AnyAsync(so => so.OrderNumber.StartsWith("SO-HIST-")))
        {
            logger.LogInformation("Historical collected orders already exist, skipping");
            return;
        }

        var random = new Random(123); // Fixed seed for reproducible data
        var warehouse = locations[0]; // Main warehouse

        // Define sites for historical orders (matching seeded sites)
        var sites = new[]
        {
            (Id: Guid.Parse("22222222-2222-2222-2222-222222222222"), Name: "Quantum Build"),
            (Id: Guid.Parse("33333333-3333-3333-3333-333333333333"), Name: "South West Gate"),
            (Id: Guid.Parse("44444444-4444-4444-4444-444444444444"), Name: "Marmalade Lane"),
            (Id: Guid.Parse("55555555-5555-5555-5555-555555555555"), Name: "Rathbourne Crossing"),
            (Id: Guid.Parse("66666666-6666-6666-6666-666666666666"), Name: "Castleforbes Prem Inn"),
            (Id: Guid.Parse("77777777-7777-7777-7777-777777777777"), Name: "Eden"),
            (Id: Guid.Parse("88888888-8888-8888-8888-888888888888"), Name: "Ford"),
        };

        var stockOrders = new List<StockOrder>();
        var stockOrderLines = new List<StockOrderLine>();
        var orderNumber = 100; // Start at 100 to avoid conflicts

        // Generate orders for the past 4 months
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-4);
        var currentDate = startDate;

        // Generate 3-5 orders per week over 4 months
        while (currentDate < endDate)
        {
            var ordersThisWeek = random.Next(3, 6);

            for (var i = 0; i < ordersThisWeek; i++)
            {
                var orderDate = currentDate.AddDays(random.Next(0, 7)).AddHours(random.Next(8, 17));
                if (orderDate > endDate) break;

                var site = sites[random.Next(sites.Length)];
                var approvalDate = orderDate.AddDays(random.Next(1, 3));
                var collectedDate = approvalDate.AddDays(random.Next(1, 4));

                // Make sure collected date is in the past
                if (collectedDate > endDate)
                {
                    collectedDate = endDate.AddDays(-random.Next(1, 3));
                    approvalDate = collectedDate.AddDays(-random.Next(1, 3));
                }

                var stockOrder = new StockOrder
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    OrderNumber = $"SO-HIST-{orderNumber++:D4}",
                    SiteId = site.Id,
                    SiteName = site.Name,
                    OrderDate = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc),
                    RequiredDate = DateTime.SpecifyKind(orderDate.AddDays(5), DateTimeKind.Utc),
                    Status = StockOrderStatus.Collected,
                    RequestedBy = GetRandomRequestor(random),
                    ApprovedBy = "System Administrator",
                    ApprovedDate = DateTime.SpecifyKind(approvalDate, DateTimeKind.Utc),
                    CollectedDate = DateTime.SpecifyKind(collectedDate, DateTimeKind.Utc),
                    Notes = "Historical test data",
                    SourceLocationId = warehouse.Id,
                    CreatedAt = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc),
                    CreatedBy = "system"
                };

                // Add 2-6 line items per order
                var lineCount = random.Next(2, 7);
                var selectedProducts = products.OrderBy(_ => random.Next()).Take(lineCount).ToList();
                decimal orderTotal = 0;

                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(5, 31); // 5-30 units
                    var line = new StockOrderLine
                    {
                        Id = Guid.NewGuid(),
                        TenantId = TenantId,
                        StockOrderId = stockOrder.Id,
                        ProductId = product.Id,
                        QuantityRequested = quantity,
                        QuantityIssued = quantity, // All issued for collected orders
                        UnitPrice = product.BaseRate,
                        CreatedAt = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc),
                        CreatedBy = "system"
                    };

                    stockOrderLines.Add(line);
                    orderTotal += quantity * product.BaseRate;
                }

                stockOrder.OrderTotal = orderTotal;
                stockOrders.Add(stockOrder);
            }

            currentDate = currentDate.AddDays(7); // Move to next week
        }

        await context.Set<StockOrder>().AddRangeAsync(stockOrders);
        await context.Set<StockOrderLine>().AddRangeAsync(stockOrderLines);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {OrderCount} historical collected orders with {LineCount} lines",
            stockOrders.Count, stockOrderLines.Count);
    }

    private static string GetRandomRequestor(Random random)
    {
        var requestors = new[]
        {
            "John Smith",
            "Mary O'Brien",
            "Patrick Murphy",
            "Sarah Walsh",
            "Michael Kelly",
            "Emma Byrne",
            "David Ryan",
            "Lisa Doyle"
        };
        return requestors[random.Next(requestors.Length)];
    }

    #region Helper Methods

    private static Category CreateCategory(string name, int sortOrder) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        CategoryName = name,
        SortOrder = sortOrder,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static Supplier CreateSupplier(string code, string name, string? contact, string? email, string? phone, string? address, string? paymentTerms) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        SupplierCode = code,
        SupplierName = name,
        ContactName = contact,
        Email = email,
        Phone = phone,
        Address = address,
        PaymentTerms = paymentTerms,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static StockLocation CreateStockLocation(string code, string name, LocationType type, string? address) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        LocationCode = code,
        LocationName = name,
        LocationType = type,
        Address = address,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static BayLocation CreateBayLocation(string code, string name, Guid stockLocationId, int? capacity) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        BayCode = code,
        BayName = name,
        StockLocationId = stockLocationId,
        Capacity = capacity,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static Product CreateProduct(string code, string name, Guid categoryId, Guid supplierId, string unitType, decimal baseRate, int reorderLevel, int reorderQty) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        ProductCode = code,
        ProductName = name,
        CategoryId = categoryId,
        SupplierId = supplierId,
        UnitType = unitType,
        BaseRate = baseRate,
        ReorderLevel = reorderLevel,
        ReorderQuantity = reorderQty,
        LeadTimeDays = 3,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static ProductKit CreateProductKit(string kitCode, string kitName, string? description, Guid? categoryId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        KitCode = kitCode,
        KitName = kitName,
        Description = description,
        CategoryId = categoryId,
        IsActive = true,
        TotalCost = 0, // Will be calculated after items are added
        TotalPrice = 0, // Will be calculated after items are added
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static ProductKitItem CreateProductKitItem(Guid productKitId, Guid productId, decimal defaultQuantity, int sortOrder, string? notes) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        ProductKitId = productKitId,
        ProductId = productId,
        DefaultQuantity = defaultQuantity,
        SortOrder = sortOrder,
        Notes = notes,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    #endregion
}
