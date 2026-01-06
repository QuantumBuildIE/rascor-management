# RASCOR Stock Management - End-to-End Testing Checklist

**Date:** _________________  
**Tester:** _________________  
**Environment:** localhost / staging / production

---

## 1. Authentication

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| Login with admin@rascor.ie / Admin123! | | | |
| Verify dashboard loads with all module cards | | | |
| Logout and verify redirect to login | | | |
| Login with warehouse@rascor.ie / Warehouse123! | | | |
| Verify limited module cards based on permissions | | | |
| Logout | | | |

---

## 2. Products (login as admin)

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| View products list - pagination works | | | |
| View products list - sorting works | | | |
| View products list - search works | | | |
| Create new product with all fields | | | |
| Verify CostPrice, SellPrice, ProductType save correctly | | | |
| Verify Margin Amount calculated correctly | | | |
| Verify Margin % calculated correctly | | | |
| Upload product image | | | |
| Verify image displays in list (thumbnail) | | | |
| Edit product - change fields, verify save | | | |
| Delete product image | | | |
| Delete product | | | |
| Export products to Excel - verify download | | | |
| Export products to PDF - verify download | | | |

---

## 3. Categories & Suppliers

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| Create a category | | | |
| Edit a category | | | |
| Delete a category | | | |
| Create a supplier | | | |
| Edit a supplier | | | |
| Delete a supplier | | | |

---

## 4. Stock Locations & Bay Locations

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| View stock locations list | | | |
| Create a new stock location | | | |
| Create a new bay location | | | |
| Assign bay to correct stock location | | | |
| Edit bay location | | | |
| Verify bay dropdown filters by stock location | | | |

---

## 5. Stock Levels

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| View stock levels list | | | |
| Filter by location | | | |
| Assign a bay location to a stock level | | | |
| Export to Excel | | | |
| Export to PDF | | | |

---

## 6. Stock Order Workflow (Critical Path)

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| **Create Order** | | | |
| Create new stock order | | | |
| Select Site | | | |
| Select Source Location | | | |
| Add line items (multiple products) | | | |
| Save as Draft | | | |
| Edit the draft order | | | |
| **Submit & Approve** | | | |
| Submit order → status = Submitted | | | |
| Approve order → status = Approved | | | |
| Verify QuantityReserved increased in Stock Levels | | | |
| **Collection** | | | |
| Mark Ready for Collection → status = ReadyForCollection | | | |
| Print Order Docket | | | |
| Verify docket shows bay locations | | | |
| Collect order → status = Collected | | | |
| **Verify Stock Updates** | | | |
| Verify QuantityOnHand decreased | | | |
| Verify QuantityReserved decreased | | | |
| Verify Stock Transaction created | | | |
| **Export** | | | |
| Export orders to Excel | | | |
| Export orders to PDF | | | |

---

## 7. Purchase Order Workflow

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| Create new purchase order | | | |
| Select Supplier | | | |
| Add line items with quantities and prices | | | |
| Save as Draft | | | |
| Submit/Confirm PO | | | |
| Verify total value calculated correctly | | | |
| Export to Excel | | | |
| Export to PDF | | | |

---

## 8. Goods Receipt Workflow

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| **Create GRN** | | | |
| Create new GRN | | | |
| Link to Purchase Order | | | |
| Verify lines auto-populate from PO | | | |
| Enter Delivery Note Ref | | | |
| Set quantities received | | | |
| Set quantity rejected with reason (one line) | | | |
| Assign bay locations to lines | | | |
| Enter batch number for one line | | | |
| Enter expiry date for one line | | | |
| **Complete GRN** | | | |
| Complete/Confirm GRN | | | |
| Verify QuantityOnHand increased | | | |
| Verify PO status updated | | | |
| Verify Stock Transaction created | | | |

---

## 9. Stocktake Workflow

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| **Create Stocktake** | | | |
| Create new stocktake | | | |
| Select Location | | | |
| Save → verify lines auto-populated | | | |
| Verify bay locations copied to lines | | | |
| **Print Count Sheet** | | | |
| Navigate to Print Count Sheet | | | |
| Verify QR codes display | | | |
| Verify sorted by bay | | | |
| Verify print layout (Ctrl+P) | | | |
| **Quick Count** | | | |
| Navigate to Quick Count (scan QR or manual) | | | |
| Enter counted quantity | | | |
| Save → verify line updated | | | |
| **Enter Counts with Variances** | | | |
| Enter count OVER expected (found extra) | | | |
| Enter count UNDER expected (missing) | | | |
| Add variance reason for each | | | |
| **Complete Stocktake** | | | |
| Complete stocktake | | | |
| Verify QuantityOnHand adjusted | | | |
| Verify adjustment transactions created | | | |

---

## 10. Stock Valuation Report

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| Navigate to Reports → Stock Valuation | | | |
| Verify summary totals display correctly | | | |
| Filter by location → verify data filters | | | |
| Filter by category → verify data filters | | | |
| Export to Excel → verify download | | | |
| Export to PDF → verify download | | | |
| Print → verify print layout | | | |

---

## 11. Admin Module

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| **Sites** | | | |
| Create a site | | | |
| Edit a site | | | |
| Delete a site | | | |
| **Employees** | | | |
| Create employee with site assignment | | | |
| Edit employee | | | |
| Delete employee | | | |
| **Companies** | | | |
| Create company | | | |
| Add contacts to company | | | |
| Edit company | | | |
| Delete company | | | |
| **Users** | | | |
| View users list | | | |
| Create new user with roles | | | |
| Edit user | | | |
| Reset password | | | |

---

## 12. Mobile Responsiveness

Test on mobile device or browser dev tools (375px width):

| Test | Pass | Fail | Notes |
|------|------|------|-------|
| Login page usable | | | |
| Dashboard cards stack correctly | | | |
| Navigation menu works (hamburger) | | | |
| Products table scrolls horizontally | | | |
| Product form fields stack vertically | | | |
| Stock order form usable | | | |
| Quick Count page easy to use | | | |
| Large touch targets on buttons | | | |
| Print pages readable | | | |

---

## Issues Found

| # | Page/Feature | Description | Severity | Fixed |
|---|--------------|-------------|----------|-------|
| 1 | | | | |
| 2 | | | | |
| 3 | | | | |
| 4 | | | | |
| 5 | | | | |
| 6 | | | | |
| 7 | | | | |
| 8 | | | | |
| 9 | | | | |
| 10 | | | | |

---

## Sign-Off

**Testing Completed:** Yes / No  
**Overall Status:** Pass / Fail / Pass with Issues  

**Notes:**
_____________________________________________
_____________________________________________
_____________________________________________

**Signed:** _________________  
**Date:** _________________
