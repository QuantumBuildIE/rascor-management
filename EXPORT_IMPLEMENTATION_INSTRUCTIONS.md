# Export Functionality Implementation Instructions

## Backend Changes

### 1. Register ExportService in DependencyInjection.cs

**File:** `src/Modules/StockManagement/Rascor.Modules.StockManagement.Application/DependencyInjection.cs`

Add these using statements at the top:
```csharp
using Rascor.Modules.StockManagement.Application.Interfaces;
using Rascor.Modules.StockManagement.Infrastructure.Services;
```

Add this line in the `AddApplication` method (around line 45, after the other service registrations):
```csharp
services.AddScoped<IExportService, ExportService>();
```

### 2. QuestPDF License Configuration

**File:** `src/Rascor.API/Program.cs`

Add this line at the very top of the file (before the `var builder` line):
```csharp
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
```

Also add this using statement:
```csharp
using QuestPDF.Infrastructure;
```

## Frontend Changes

### 1. Add ExportButtons to Products Page

**File:** `web/src/app/(authenticated)/stock/products/page.tsx`

Add import:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";
```

Replace the header section (around lines 250-258) with:
```typescript
<div className="flex items-center justify-between">
  <div>
    <h1 className="text-2xl font-semibold tracking-tight">Products</h1>
    <p className="text-muted-foreground">Manage your product catalog</p>
  </div>
  <div className="flex gap-2">
    <ExportButtons exportUrl="/api/exports/products" />
    <Button asChild>
      <Link href="/stock/products/new">Add Product</Link>
    </Button>
  </div>
</div>
```

### 2. Add ExportButtons to Stock Levels Page

**File:** `web/src/app/(authenticated)/stock/levels/page.tsx`

Add import:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";
```

In the header section, add ExportButtons before the page actions, passing the current locationId filter:
```typescript
<ExportButtons
  exportUrl="/api/exports/stock-levels"
  filters={{ locationId: selectedLocationId }}
/>
```

### 3. Add ExportButtons to Stock Orders Page

**File:** `web/src/app/(authenticated)/stock/orders/page.tsx`

Add import:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";
```

In the header section, add ExportButtons before the "Create Order" button, passing the current status filter:
```typescript
<ExportButtons
  exportUrl="/api/exports/stock-orders"
  filters={{ status: selectedStatus }}
/>
```

### 4. Add ExportButtons to Purchase Orders Page

**File:** `web/src/app/(authenticated)/stock/purchase-orders/page.tsx`

Add import:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";
```

In the header section, add ExportButtons before the "Create Purchase Order" button:
```typescript
<ExportButtons exportUrl="/api/exports/purchase-orders" />
```

### 5. Add ExportButtons to Stock Valuation Report Page

**File:** `web/src/app/(authenticated)/stock/reports/valuation/page.tsx`

Add import:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";
```

In the header section (likely near any existing print button), add ExportButtons passing the current filters:
```typescript
<ExportButtons
  exportUrl="/api/exports/stock-valuation"
  filters={{
    locationId: selectedLocationId,
    categoryId: selectedCategoryId
  }}
/>
```

## Files Already Created

The following files have been successfully created and don't need any changes:

1. `src/Modules/StockManagement/Rascor.Modules.StockManagement.Application/Interfaces/IExportService.cs`
2. `src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Services/ExportService.cs`
3. `src/Rascor.API/Controllers/ExportsController.cs`
4. `web/src/components/shared/export-buttons.tsx`

## Testing

After making these changes:

1. Rebuild the backend: `dotnet build`
2. Restart the API server
3. In the frontend, test export buttons on each page:
   - Products list
   - Stock levels
   - Stock orders
   - Purchase orders
   - Stock valuation report
4. Verify both Excel and PDF exports work correctly
5. Check that filters are properly applied to exports

## API Endpoints Created

- `GET /api/exports/products?format=excel|pdf`
- `GET /api/exports/stock-levels?format=excel|pdf&locationId={guid}`
- `GET /api/exports/stock-orders?format=excel|pdf&status={status}`
- `GET /api/exports/purchase-orders?format=excel|pdf`
- `GET /api/exports/stock-valuation?format=excel|pdf&locationId={guid}&categoryId={guid}` (requires StockManagement.ViewCostings permission)
