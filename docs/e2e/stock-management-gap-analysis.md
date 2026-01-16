# Stock Management E2E Gap Analysis

**Generated:** January 15, 2026
**Codebase Version:** Latest (as of analysis date)
**Total API Endpoints:** 74
**Total Frontend Pages:** 30+
**Existing E2E Tests:** 21 tests across 3 files

---

## Executive Summary

The Stock Management module has **~20% test coverage** by feature count. While the stock order workflow is partially covered, critical areas like Purchase Orders, Goods Receipts, Stocktakes, Categories, Suppliers, Bay Locations, and Reports have **zero E2E test coverage**.

### Key Findings:
- ❌ **8 of 12 sub-modules are completely untested**
- ⚠️ **4 tests have no assertions** (always pass)
- ⚠️ **12 tests have weak assertions** (only check visibility)
- ✅ **Only 5 tests have meaningful assertions**
- ❌ **No tests verify backend data** (stock levels, transactions)
- ❌ **No tests for edit/delete operations** on any entity

---

## 1. Implementation Inventory

### 1.1 Products Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/products` | List products (paginated, searchable) | ⚠️ PARTIAL - `products.spec.ts:6` (visibility only) |
| `/stock/products/new` | Create product form | ⚠️ WEAK - `products.spec.ts:31` (no data verification) |
| `/stock/products/[id]/edit` | Edit product | ❌ NOT TESTED |
| `/stock/products/[id]` | View product details | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/products` | ⚠️ PARTIAL - Loaded but no data assertions |
| GET | `/api/products/all` | ❌ NOT TESTED |
| GET | `/api/products/{id}` | ❌ NOT TESTED |
| POST | `/api/products` | ⚠️ WEAK - Created but no verification |
| PUT | `/api/products/{id}` | ❌ NOT TESTED |
| DELETE | `/api/products/{id}` | ❌ NOT TESTED |
| POST | `/api/products/{id}/image` | ❌ NOT TESTED |
| DELETE | `/api/products/{id}/image` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status | Notes |
|-----------|-------------|-------|
| Create product with full details | ⚠️ WEAK | `products.spec.ts:31` - No backend verification |
| Edit existing product | ❌ NOT TESTED | |
| Delete product | ❌ NOT TESTED | |
| Search products | ❌ BROKEN | `products.spec.ts:14` - **No assertions** |
| Sort products | ❌ NOT TESTED | |
| Paginate products | ❌ NOT TESTED | |
| Upload product image | ❌ NOT TESTED | |
| Delete product image | ❌ NOT TESTED | |
| View product details | ❌ NOT TESTED | |

---

### 1.2 Categories Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/categories` | List categories | ❌ NOT TESTED |
| `/stock/categories/new` | Create category form | ❌ NOT TESTED |
| `/stock/categories/[id]/edit` | Edit category | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/categories` | ❌ NOT TESTED |
| GET | `/api/categories/{id}` | ❌ NOT TESTED |
| POST | `/api/categories` | ❌ NOT TESTED |
| PUT | `/api/categories/{id}` | ❌ NOT TESTED |
| DELETE | `/api/categories/{id}` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status |
|-----------|-------------|
| Create category | ❌ NOT TESTED |
| Edit category | ❌ NOT TESTED |
| Delete category | ❌ NOT TESTED |
| List categories | ❌ NOT TESTED |
| Toggle category active status | ❌ NOT TESTED |

---

### 1.3 Suppliers Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/suppliers` | List suppliers | ❌ NOT TESTED |
| `/stock/suppliers/new` | Create supplier form | ❌ NOT TESTED |
| `/stock/suppliers/[id]/edit` | Edit supplier | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/suppliers` | ❌ NOT TESTED |
| GET | `/api/suppliers/{id}` | ❌ NOT TESTED |
| POST | `/api/suppliers` | ❌ NOT TESTED |
| PUT | `/api/suppliers/{id}` | ❌ NOT TESTED |
| DELETE | `/api/suppliers/{id}` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status |
|-----------|-------------|
| Create supplier | ❌ NOT TESTED |
| Edit supplier | ❌ NOT TESTED |
| Delete supplier | ❌ NOT TESTED |
| List suppliers | ❌ NOT TESTED |

---

### 1.4 Bay Locations Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/bay-locations` | List bay locations | ❌ NOT TESTED |
| `/stock/bay-locations/new` | Create bay location | ❌ NOT TESTED |
| `/stock/bay-locations/[id]/edit` | Edit bay location | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/bay-locations` | ❌ NOT TESTED |
| GET | `/api/bay-locations/{id}` | ❌ NOT TESTED |
| GET | `/api/bay-locations/by-location/{stockLocationId}` | ❌ NOT TESTED |
| POST | `/api/bay-locations` | ❌ NOT TESTED |
| PUT | `/api/bay-locations/{id}` | ❌ NOT TESTED |
| DELETE | `/api/bay-locations/{id}` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status |
|-----------|-------------|
| Create bay location | ❌ NOT TESTED |
| Edit bay location | ❌ NOT TESTED |
| Delete bay location | ❌ NOT TESTED |
| Filter by stock location | ❌ NOT TESTED |

---

### 1.5 Stock Locations Module

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/stock-locations` | ⚠️ INDIRECT - Used in forms but not tested directly |
| GET | `/api/stock-locations/{id}` | ❌ NOT TESTED |
| POST | `/api/stock-locations` | ❌ NOT TESTED |
| PUT | `/api/stock-locations/{id}` | ❌ NOT TESTED |
| DELETE | `/api/stock-locations/{id}` | ❌ NOT TESTED |

---

### 1.6 Stock Levels Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/levels` | View stock levels with filtering | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/stock-levels` | ❌ NOT TESTED |
| GET | `/api/stock-levels/{id}` | ❌ NOT TESTED |
| GET | `/api/stock-levels/by-location/{locationId}` | ❌ NOT TESTED |
| GET | `/api/stock-levels/low-stock` | ❌ NOT TESTED |
| GET | `/api/stock-levels/by-product/{productId}/location/{locationId}` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status | Notes |
|-----------|-------------|-------|
| View stock levels | ❌ NOT TESTED | |
| Filter by location | ❌ NOT TESTED | |
| Low stock highlighting | ❌ NOT TESTED | |
| Verify stock after order collection | ❌ NOT TESTED | Critical gap! |
| Verify reservation on approval | ❌ NOT TESTED | Critical gap! |

---

### 1.7 Stock Orders Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/orders` | List orders with status tabs | ⚠️ PARTIAL - `stock-orders.spec.ts:10` |
| `/stock/orders/new` | Create order form | ⚠️ PARTIAL - `stock-order-workflow.spec.ts:25` |
| `/stock/orders/[id]` | View order details | ✅ GOOD - Multiple workflow tests |
| `/stock/orders/[id]/edit` | Edit order | ❌ NOT TESTED |
| `/stock/orders/[id]/print` | Print docket | ⚠️ WEAK - `stock-order-workflow.spec.ts:292` |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/stock-orders` | ⚠️ PARTIAL - Loaded but weak assertions |
| GET | `/api/stock-orders/{id}` | ⚠️ INDIRECT - Used in workflow tests |
| GET | `/api/stock-orders/{id}/docket` | ⚠️ WEAK - No content verification |
| GET | `/api/stock-orders/by-site/{siteId}` | ❌ NOT TESTED |
| GET | `/api/stock-orders/by-status/{status}` | ⚠️ PARTIAL - Tab filtering tested |
| POST | `/api/stock-orders` | ✅ GOOD - `stock-order-workflow.spec.ts:25` |
| POST | `/api/stock-orders/{id}/submit` | ✅ GOOD - `stock-order-workflow.spec.ts:95` |
| POST | `/api/stock-orders/{id}/approve` | ✅ GOOD - `stock-order-workflow.spec.ts:128` |
| POST | `/api/stock-orders/{id}/reject` | ✅ GOOD - `stock-order-workflow.spec.ts:243` |
| POST | `/api/stock-orders/{id}/ready-for-collection` | ✅ GOOD - `stock-order-workflow.spec.ts:167` |
| POST | `/api/stock-orders/{id}/collect` | ✅ GOOD - `stock-order-workflow.spec.ts:200` |
| POST | `/api/stock-orders/{id}/cancel` | ❌ NOT TESTED |
| PUT | `/api/stock-orders/{id}` | ❌ NOT TESTED |
| DELETE | `/api/stock-orders/{id}` | ❌ NOT TESTED |

**Workflow Actions:**
| Action | Test Status | Notes |
|--------|-------------|-------|
| Create Draft Order | ✅ GOOD | `stock-order-workflow.spec.ts:25-89` |
| Submit for Approval | ✅ GOOD | `stock-order-workflow.spec.ts:95-126` |
| Approve Order | ✅ GOOD | `stock-order-workflow.spec.ts:128-165` |
| Reject Order | ✅ GOOD | `stock-order-workflow.spec.ts:243-286` |
| Mark Ready for Collection | ✅ GOOD | `stock-order-workflow.spec.ts:167-198` |
| Complete Collection | ✅ GOOD | `stock-order-workflow.spec.ts:200-237` |
| Cancel Order | ❌ NOT TESTED | API exists, no test |
| Edit Draft Order | ❌ NOT TESTED | |
| Delete Draft Order | ❌ NOT TESTED | |

**Missing Backend Verifications:**
| Verification | Test Status | Notes |
|--------------|-------------|-------|
| Stock reserved on approval | ❌ NOT TESTED | Should check QuantityReserved increases |
| Stock decremented on collection | ❌ NOT TESTED | Should check QuantityOnHand decreases |
| Reserved released on cancel | ❌ NOT TESTED | Should check QuantityReserved decreases |
| Order total calculated correctly | ❌ NOT TESTED | Should verify line totals |
| Transaction audit log created | ❌ NOT TESTED | Should check StockTransaction records |

---

### 1.8 Purchase Orders Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/purchase-orders` | List POs with status tabs | ❌ NOT TESTED |
| `/stock/purchase-orders/new` | Create PO form | ❌ NOT TESTED |
| `/stock/purchase-orders/[id]` | View PO details | ❌ NOT TESTED |
| `/stock/purchase-orders/[id]/edit` | Edit PO | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/purchase-orders` | ❌ NOT TESTED |
| GET | `/api/purchase-orders/{id}` | ❌ NOT TESTED |
| GET | `/api/purchase-orders/by-supplier/{supplierId}` | ❌ NOT TESTED |
| GET | `/api/purchase-orders/by-status/{status}` | ❌ NOT TESTED |
| POST | `/api/purchase-orders` | ❌ NOT TESTED |
| PUT | `/api/purchase-orders/{id}` | ❌ NOT TESTED |
| POST | `/api/purchase-orders/{id}/confirm` | ❌ NOT TESTED |
| POST | `/api/purchase-orders/{id}/cancel` | ❌ NOT TESTED |
| DELETE | `/api/purchase-orders/{id}` | ❌ NOT TESTED |

**Workflow Actions:**
| Action | Test Status |
|--------|-------------|
| Create Draft PO | ❌ NOT TESTED |
| Confirm PO | ❌ NOT TESTED |
| Cancel PO | ❌ NOT TESTED |
| Edit Draft PO | ❌ NOT TESTED |
| Delete Draft PO | ❌ NOT TESTED |
| Receive goods against PO | ❌ NOT TESTED |
| Track partial receipt | ❌ NOT TESTED |

---

### 1.9 Goods Receipts Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/goods-receipts` | List GRNs | ❌ NOT TESTED |
| `/stock/goods-receipts/new` | Create GRN form | ❌ NOT TESTED |
| `/stock/goods-receipts/[id]` | View GRN details | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/goods-receipts` | ❌ NOT TESTED |
| GET | `/api/goods-receipts/{id}` | ❌ NOT TESTED |
| GET | `/api/goods-receipts/by-supplier/{supplierId}` | ❌ NOT TESTED |
| GET | `/api/goods-receipts/by-po/{purchaseOrderId}` | ❌ NOT TESTED |
| POST | `/api/goods-receipts` | ❌ NOT TESTED |
| DELETE | `/api/goods-receipts/{id}` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status | Notes |
|-----------|-------------|-------|
| Create standalone GRN | ❌ NOT TESTED | |
| Create GRN linked to PO | ❌ NOT TESTED | |
| Add batch/lot numbers | ❌ NOT TESTED | |
| Set expiry dates | ❌ NOT TESTED | |
| Record rejected quantities | ❌ NOT TESTED | |
| Assign bay locations | ❌ NOT TESTED | |
| Verify stock increase | ❌ NOT TESTED | Critical gap! |

---

### 1.10 Stocktakes Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/stocktakes` | List stocktakes | ❌ NOT TESTED |
| `/stock/stocktakes/new` | Create stocktake | ❌ NOT TESTED |
| `/stock/stocktakes/[id]` | Conduct count | ❌ NOT TESTED |
| `/stock/stocktakes/[id]/print` | Print count sheets | ❌ NOT TESTED |
| `/stock/stocktakes/[id]/count/[lineId]` | Mobile count entry | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/stocktakes` | ❌ NOT TESTED |
| GET | `/api/stocktakes/{id}` | ❌ NOT TESTED |
| GET | `/api/stocktakes/by-location/{locationId}` | ❌ NOT TESTED |
| POST | `/api/stocktakes` | ❌ NOT TESTED |
| POST | `/api/stocktakes/{id}/start` | ❌ NOT TESTED |
| PUT | `/api/stocktakes/{id}/lines/{lineId}` | ❌ NOT TESTED |
| POST | `/api/stocktakes/{id}/complete` | ❌ NOT TESTED |
| POST | `/api/stocktakes/{id}/cancel` | ❌ NOT TESTED |
| DELETE | `/api/stocktakes/{id}` | ❌ NOT TESTED |

**Workflow Actions:**
| Action | Test Status |
|--------|-------------|
| Create stocktake | ❌ NOT TESTED |
| Start count (Draft → InProgress) | ❌ NOT TESTED |
| Update counted quantities | ❌ NOT TESTED |
| Complete stocktake | ❌ NOT TESTED |
| Variance calculation | ❌ NOT TESTED |
| Stock adjustment creation | ❌ NOT TESTED |
| QR code count sheet | ❌ NOT TESTED |
| Mobile scanning workflow | ❌ NOT TESTED |

---

### 1.11 Product Kits Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/product-kits` | List kits | ❌ NOT TESTED |
| `/stock/product-kits/new` | Create kit | ❌ NOT TESTED |
| `/stock/product-kits/[id]` | View kit details | ❌ NOT TESTED |
| `/stock/product-kits/[id]/edit` | Edit kit | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/product-kits` | ❌ NOT TESTED |
| GET | `/api/product-kits/{id}` | ❌ NOT TESTED |
| POST | `/api/product-kits` | ❌ NOT TESTED |
| PUT | `/api/product-kits/{id}` | ❌ NOT TESTED |
| DELETE | `/api/product-kits/{id}` | ❌ NOT TESTED |

---

### 1.12 Reports Module

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock/reports` | Reports landing | ❌ NOT TESTED |
| `/stock/reports/valuation` | Stock valuation report | ❌ NOT TESTED |

**API Endpoints:**
| Method | Endpoint | Test Status |
|--------|----------|-------------|
| GET | `/api/stock/reports/products-by-month` | ❌ NOT TESTED |
| GET | `/api/stock/reports/products-by-site` | ❌ NOT TESTED |
| GET | `/api/stock/reports/products-by-week` | ❌ NOT TESTED |
| GET | `/api/stock/reports/valuation` | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status |
|-----------|-------------|
| View valuation report | ❌ NOT TESTED |
| Filter by location | ❌ NOT TESTED |
| Filter by category | ❌ NOT TESTED |
| Print report | ❌ NOT TESTED |
| Permission check (ViewCostings) | ❌ NOT TESTED |

---

### 1.13 Stock Dashboard

**Frontend Pages:**
| Route | Description | Test Status |
|-------|-------------|-------------|
| `/stock` | Dashboard with KPIs and charts | ❌ NOT TESTED |

**Business Operations:**
| Operation | Test Status |
|-----------|-------------|
| View summary cards | ❌ NOT TESTED |
| View products by month chart | ❌ NOT TESTED |
| View products by site chart | ❌ NOT TESTED |
| View weekly trends chart | ❌ NOT TESTED |
| Quick action buttons | ❌ NOT TESTED |

---

## 2. Critical Gaps (Priority for Testing)

### 🔴 High Priority (Core Business Operations)

| # | Gap | Impact | Effort |
|---|-----|--------|--------|
| 1 | **Purchase Order workflow** | Core procurement process untested | High |
| 2 | **Goods Receipt workflow** | Stock increases never verified | High |
| 3 | **Stocktake workflow** | Inventory accuracy process untested | High |
| 4 | **Stock level verification** | No proof stock quantities change correctly | Medium |
| 5 | **Edit Product** | Cannot verify updates work | Low |
| 6 | **Cancel Stock Order** | Critical workflow action untested | Low |
| 7 | **Delete operations** | No soft-delete verification | Low |

### 🟡 Medium Priority (Important Workflows)

| # | Gap | Impact | Effort |
|---|-----|--------|--------|
| 8 | **Categories CRUD** | Product organization untested | Medium |
| 9 | **Suppliers CRUD** | Vendor management untested | Medium |
| 10 | **Bay Locations CRUD** | Warehouse organization untested | Medium |
| 11 | **Print docket content** | Docket correctness unverified | Low |
| 12 | **Search/filter functionality** | Current tests have no assertions | Low |
| 13 | **Permission-based access** | Role restrictions unverified | Medium |

### 🟢 Low Priority (Nice to Have)

| # | Gap | Impact | Effort |
|---|-----|--------|--------|
| 14 | **Product Kits** | Template feature untested | Medium |
| 15 | **Reports** | Analytics accuracy untested | Medium |
| 16 | **Dashboard KPIs** | Metrics accuracy untested | Low |
| 17 | **Product images** | Upload/delete untested | Low |
| 18 | **Pagination** | Multi-page lists untested | Low |
| 19 | **Sort functionality** | Column sorting untested | Low |

---

## 3. Test Quality Issues

### 3.1 Tests Without Assertions (Always Pass) ❌

These tests will **always pass** regardless of whether the feature works:

| File | Line | Test Name | Issue |
|------|------|-----------|-------|
| `products.spec.ts` | 14-21 | should search products | Searches but never verifies results |
| `stock-orders.spec.ts` | 18-24 | should filter orders by status | Filters but never checks results match filter |
| `stock-orders.spec.ts` | 54-61 | should not see approve button | Never actually checks button is hidden |
| `stock-order-workflow.spec.ts` | 362-376 | can search orders by reference | Searches but never verifies results |

### 3.2 Tests With Weak Assertions (Only Check Visibility) ⚠️

These tests pass if the page loads, but don't verify functionality:

| File | Line | Test Name | Only Checks |
|------|------|-----------|-------------|
| `products.spec.ts` | 6-12 | should display product list | Table visible |
| `products.spec.ts` | 23-29 | should navigate to create product | URL matches |
| `products.spec.ts` | 31-53 | should create a new product | URL redirect only |
| `products.spec.ts` | 57-62 | should be able to view products | Table visible |
| `stock-orders.spec.ts` | 10-16 | should display stock order list | Table visible |
| `stock-orders.spec.ts` | 26-32 | should navigate to create order | URL matches |
| `stock-orders.spec.ts` | 36-43 | should create a new stock order | Form element visible (incomplete) |
| `stock-orders.spec.ts` | 47-52 | should be able to create orders | Button visible |
| `stock-orders.spec.ts` | 65-70 | should see orders and workflow actions | Table visible |
| `stock-order-workflow.spec.ts` | 292-326 | can view print preview for order | Page not empty |

### 3.3 Tests With Good Assertions ✅

These tests have meaningful assertions:

| File | Line | Test Name | Verifies |
|------|------|-----------|----------|
| `stock-order-workflow.spec.ts` | 25-89 | can create a new stock order | Success message OR redirect to detail page |
| `stock-order-workflow.spec.ts` | 95-126 | can submit draft order for approval | Status badge changes |
| `stock-order-workflow.spec.ts` | 128-165 | can approve a submitted order | Status becomes "Approved" |
| `stock-order-workflow.spec.ts` | 167-198 | can mark order as ready for collection | Status badge visible |
| `stock-order-workflow.spec.ts` | 200-237 | can complete order collection | Status becomes "Collected" |

---

## 4. Missing Feature Categories

### 4.1 Completely Untested Sub-Modules (0% Coverage)

| Module | CRUD Operations | Workflow Actions | Total Features | Tests |
|--------|-----------------|------------------|----------------|-------|
| Categories | 5 | 0 | 5 | 0 |
| Suppliers | 5 | 0 | 5 | 0 |
| Bay Locations | 6 | 0 | 6 | 0 |
| Stock Locations | 5 | 0 | 5 | 0 |
| Stock Levels | 5 | 0 | 5 | 0 |
| Purchase Orders | 5 | 4 | 9 | 0 |
| Goods Receipts | 4 | 3 | 7 | 0 |
| Stocktakes | 5 | 4 | 9 | 0 |
| Product Kits | 5 | 0 | 5 | 0 |
| Reports | 4 | 0 | 4 | 0 |
| Dashboard | 0 | 0 | 5 | 0 |
| **Total** | **49** | **11** | **65** | **0** |

### 4.2 Partially Tested Sub-Modules

| Module | CRUD Operations | Workflow Actions | Features Tested | Coverage |
|--------|-----------------|------------------|-----------------|----------|
| Products | 8 | 0 | 2 (weak) | 25% |
| Stock Orders | 7 | 7 | 8 (5 good) | 57% |

---

## 5. Data Quality Checks Needed

### 5.1 Missing Validations

| Category | Tests Needed |
|----------|-------------|
| **Date format validation** | UTC vs local timezone handling |
| **Numeric field validation** | Negative quantities, zero values, decimal precision |
| **Required field validation** | All required fields on all forms |
| **Business rule enforcement** | Cannot approve own orders, cannot delete completed orders |
| **Calculated fields** | Line totals, order totals, VAT calculations |
| **Audit trail verification** | StockTransaction records created correctly |

### 5.2 Missing Error Path Tests

| Error Scenario | Test Needed |
|----------------|-------------|
| Duplicate product code | Should show validation error |
| Missing required fields | Should highlight missing fields |
| Invalid email/phone format | Should show format error |
| Insufficient stock for order | Should prevent approval |
| Order already approved | Should prevent duplicate approval |
| Network error handling | Should show error toast |

---

## 6. Recommended Test Priorities

### Phase 1: Critical Fixes (Immediate)

**Goal:** Fix broken tests and add critical missing coverage

| Task | Files | Effort |
|------|-------|--------|
| 1. Fix 4 tests with no assertions | `products.spec.ts`, `stock-orders.spec.ts`, `stock-order-workflow.spec.ts` | 2 hours |
| 2. Add stock level verification to workflow tests | `stock-order-workflow.spec.ts` | 4 hours |
| 3. Add Cancel Stock Order test | New or existing file | 2 hours |
| 4. Add Edit/Delete Product tests | `products.spec.ts` | 3 hours |

### Phase 2: Purchase Order & Goods Receipt (Sprint 1)

**Goal:** Cover the core procurement workflow

| Task | New Test File | Effort |
|------|---------------|--------|
| 1. Create Purchase Order | `purchase-orders.spec.ts` | 4 hours |
| 2. Confirm Purchase Order | `purchase-orders.spec.ts` | 2 hours |
| 3. Create Goods Receipt | `goods-receipts.spec.ts` | 4 hours |
| 4. Verify stock increase on receipt | `goods-receipts.spec.ts` | 3 hours |
| 5. Link GRN to PO | `goods-receipt-workflow.spec.ts` | 3 hours |

### Phase 3: Stocktake Workflow (Sprint 2)

**Goal:** Cover inventory count process

| Task | New Test File | Effort |
|------|---------------|--------|
| 1. Create Stocktake | `stocktakes.spec.ts` | 3 hours |
| 2. Start Count | `stocktakes.spec.ts` | 2 hours |
| 3. Update counted quantities | `stocktakes.spec.ts` | 3 hours |
| 4. Complete with adjustment | `stocktake-workflow.spec.ts` | 4 hours |
| 5. Variance verification | `stocktake-workflow.spec.ts` | 3 hours |

### Phase 4: Supporting Modules (Sprint 3)

**Goal:** Cover reference data management

| Task | New Test File | Effort |
|------|---------------|--------|
| 1. Categories CRUD | `categories.spec.ts` | 4 hours |
| 2. Suppliers CRUD | `suppliers.spec.ts` | 4 hours |
| 3. Bay Locations CRUD | `bay-locations.spec.ts` | 4 hours |

### Phase 5: Reports & Analytics (Sprint 4)

**Goal:** Verify reporting accuracy

| Task | New Test File | Effort |
|------|---------------|--------|
| 1. Stock Valuation Report | `stock-reports.spec.ts` | 4 hours |
| 2. Dashboard KPIs | `stock-dashboard.spec.ts` | 3 hours |
| 3. Permission-based access | Various files | 4 hours |

---

## 7. Coverage Statistics

### By Sub-Module

| Module | Total Features | Tested Features | Coverage % |
|--------|---------------|-----------------|------------|
| Products | 16 | 3 | 19% |
| Categories | 5 | 0 | 0% |
| Suppliers | 5 | 0 | 0% |
| Bay Locations | 6 | 0 | 0% |
| Stock Locations | 5 | 0 | 0% |
| Stock Levels | 5 | 0 | 0% |
| Stock Orders | 14 | 8 | 57% |
| Purchase Orders | 9 | 0 | 0% |
| Goods Receipts | 7 | 0 | 0% |
| Stocktakes | 9 | 0 | 0% |
| Product Kits | 5 | 0 | 0% |
| Reports | 4 | 0 | 0% |
| Dashboard | 5 | 0 | 0% |
| **TOTAL** | **95** | **11** | **12%** |

### By Test Quality

| Quality Level | Count | Percentage |
|---------------|-------|------------|
| ✅ GOOD (meaningful assertions) | 5 | 24% |
| ⚠️ WEAK (visibility only) | 12 | 57% |
| ❌ NONE (no assertions) | 4 | 19% |
| **TOTAL** | **21** | 100% |

### Overall Stock Management Coverage

```
┌─────────────────────────────────────────────────────────┐
│         STOCK MANAGEMENT E2E TEST COVERAGE              │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Features Tested:    11 / 95    (12%)                  │
│  Tests with Good Assertions:     5 / 21    (24%)       │
│  Sub-modules with ANY coverage:  2 / 13    (15%)       │
│                                                         │
│  ████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  12%        │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 8. Appendix

### A. Test File Locations

| File | Path | Tests |
|------|------|-------|
| Products | `tests/e2e/stock-management/products.spec.ts` | 5 |
| Stock Orders | `tests/e2e/stock-management/stock-orders.spec.ts` | 7 |
| Stock Order Workflow | `tests/e2e/critical-flows/stock-order-workflow.spec.ts` | 9 |

### B. API Controller Locations

| Controller | Path |
|------------|------|
| Categories | `src/Rascor.API/Controllers/CategoriesController.cs` |
| Products | `src/Rascor.API/Controllers/ProductsController.cs` |
| Suppliers | `src/Rascor.API/Controllers/SuppliersController.cs` |
| Stock Locations | `src/Rascor.API/Controllers/StockLocationsController.cs` |
| Bay Locations | `src/Rascor.API/Controllers/BayLocationsController.cs` |
| Stock Levels | `src/Rascor.API/Controllers/StockLevelsController.cs` |
| Stock Orders | `src/Rascor.API/Controllers/StockOrdersController.cs` |
| Purchase Orders | `src/Rascor.API/Controllers/PurchaseOrdersController.cs` |
| Goods Receipts | `src/Rascor.API/Controllers/GoodsReceiptsController.cs` |
| Stocktakes | `src/Rascor.API/Controllers/StocktakesController.cs` |
| Stock Reports | `src/Rascor.API/Controllers/StockReportsController.cs` |

### C. Frontend Page Locations

All stock management pages are under:
`web/src/app/(authenticated)/stock/`

---

*Generated by Claude Code - Gap Analysis Tool*
