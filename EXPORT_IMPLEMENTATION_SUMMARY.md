# Export Functionality - Implementation Summary

## Status
Backend implementation is 95% complete. Minor fixes and configuration needed, then frontend integration.

## Files Created Successfully

### Backend
1. **Interface:** `src/Modules/StockManagement/Rascor.Modules.StockManagement.Application/Interfaces/IExportService.cs` ✅
2. **Implementation:** `src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Services/ExportService.cs` ✅
   - Handles Excel export using ClosedXML
   - Handles PDF export using QuestPDF
   - Professional formatting with headers, alternating rows, currency/number formatting

3. **Controller:** `src/Rascor.API/Controllers/ExportsController.cs` ⚠️
   **Needs minor fixes** - see below

### Frontend
4. **Component:** `web/src/components/shared/export-buttons.tsx` ✅
   - Reusable export buttons for Excel and PDF
   - Handles file download with proper content types
   - Loading states and error handling

## Required Fixes

### 1. Fix ExportsController Property References

**File:** `src/Rascor.API/Controllers/ExportsController.cs`

Replace lines 114 and 318 (StockLocationId):
```csharp
// WRONG:
query = query.Where(sl => sl.StockLocationId == locationId.Value);

// CORRECT:
query = query.Where(sl => sl.LocationId == locationId.Value);
```

Replace line 179 (DeliverToSite):
```csharp
// WRONG:
.Include(so => so.DeliverToSite)
// and
Site = so.DeliverToSite.SiteName,

// CORRECT:
// Remove the .Include(so => so.DeliverToSite) line entirely
// and replace with:
Site = so.SiteName,
```

### 2. Register ExportService

**File:** `src/Modules/StockManagement/Rascor.Modules.StockManagement.Application/DependencyInjection.cs`

Add these using statements at the top:
```csharp
using Rascor.Modules.StockManagement.Application.Interfaces;
using Rascor.Modules.StockManagement.Infrastructure.Services;
```

Add this line in the `AddApplication` method (around line 45):
```csharp
services.AddScoped<IExportService, ExportService>();
```

### 3. Configure QuestPDF License

**File:** `src/Rascor.API/Program.cs`

Add at the very top of the file (line 1, before any other code):
```csharp
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
```

## Frontend Integration

Add ExportButtons component to these pages:

### 1. Products List Page
**File:** `web/src/app/(authenticated)/stock/products/page.tsx`

Add import:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";
```

Update header section:
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

### 2. Stock Levels Page
**File:** `web/src/app/(authenticated)/stock/levels/page.tsx`

Add import and use with filter:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";

<ExportButtons
  exportUrl="/api/exports/stock-levels"
  filters={{ locationId: selectedLocationId }}
/>
```

### 3. Stock Orders Page
**File:** `web/src/app/(authenticated)/stock/orders/page.tsx`

Add import and use with filter:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";

<ExportButtons
  exportUrl="/api/exports/stock-orders"
  filters={{ status: selectedStatus }}
/>
```

### 4. Purchase Orders Page
**File:** `web/src/app/(authenticated)/stock/purchase-orders/page.tsx`

Add import and use:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";

<ExportButtons exportUrl="/api/exports/purchase-orders" />
```

### 5. Stock Valuation Report Page
**File:** `web/src/app/(authenticated)/stock/reports/valuation/page.tsx`

Add import and use with filters:
```typescript
import { ExportButtons } from "@/components/shared/export-buttons";

<ExportButtons
  exportUrl="/api/exports/stock-valuation"
  filters={{
    locationId: selectedLocationId,
    categoryId: selectedCategoryId
  }}
/>
```

## API Endpoints

Once complete, these endpoints will be available:

- `GET /api/exports/products?format=excel|pdf`
  Permission: StockManagement.View

- `GET /api/exports/stock-levels?format=excel|pdf&locationId={guid}`
  Permission: StockManagement.View

- `GET /api/exports/stock-orders?format=excel|pdf&status={status}`
  Permission: StockManagement.View

- `GET /api/exports/purchase-orders?format=excel|pdf`
  Permission: StockManagement.View

- `GET /api/exports/stock-valuation?format=excel|pdf&locationId={guid}&categoryId={guid}`
  Permission: StockManagement.ViewCostings (Finance only)

## Testing Steps

1. Make the fixes listed above
2. Rebuild backend: `dotnet build`
3. Restart API server
4. Test each export endpoint:
   - Click "Export Excel" button
   - Click "Export PDF" button
   - Verify files download correctly
   - Check content and formatting
   - Test with different filters

## Export Features

### Excel Exports
- Professional formatting with colored headers
- Auto-fitted columns
- Frozen header row
- Currency formatting for price/cost/value columns
- Number formatting for quantities
- Date formatting (dd/MM/yyyy)

### PDF Exports
- Landscape A4 layout
- RASCOR Ireland header with report title
- Generation timestamp
- Alternating row colors for readability
- Right-aligned numeric columns
- Page numbers in footer
- Currency formatting (€)

## Packages Installed

- `ClosedXML 0.105.0` - Excel file generation
- `QuestPDF 2025.12.0` - PDF generation

Both installed in:
- src/Rascor.API/Rascor.API.csproj
- src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Rascor.Modules.StockManagement.Infrastructure.csproj
