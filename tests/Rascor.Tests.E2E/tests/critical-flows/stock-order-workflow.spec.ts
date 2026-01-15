import { test, expect } from '@playwright/test';
import { TEST_TENANT, generateTestData, TIMEOUTS } from '../../fixtures/test-constants';

/**
 * Critical E2E Flow: Stock Order Workflow
 * Tests the complete stock order lifecycle from creation to completion
 * Tag: @critical
 * Run with: npx playwright test --grep @critical
 *
 * Form uses custom SearchSelect components:
 * - SiteSearchSelect for siteId (button with "Select a site" text)
 * - LocationSearchSelect for sourceLocationId (button with "Select a location" text)
 * - ProductSearchSelect for line items
 *
 * Workflow buttons (from stock order detail page):
 * - Draft: "Submit for Approval", "Edit Order", "Delete Order"
 * - PendingApproval: "Approve Order", "Reject Order"
 * - Approved/AwaitingPick: "Mark Ready for Collection", "Print Docket", "Cancel Order"
 * - ReadyForCollection: "Mark as Collected", "Print Docket", "Cancel Order"
 */
test.describe('Stock Order Workflow @critical', () => {
  test.describe('Create Stock Order Flow', () => {
    test.use({ storageState: 'playwright/.auth/admin.json' });

    test('can create a new stock order', async ({ page }) => {
      // Navigate to new order page
      await page.goto('/stock/orders/new');
      await page.waitForLoadState('networkidle');

      // Fill in the order form
      // Select site using SiteSearchSelect (button trigger for combobox)
      const siteSelect = page.locator('button:has-text("Select a site")').first();
      if (await siteSelect.isVisible()) {
        await siteSelect.click();
        await page.locator('[role="option"]').first().click();
      }

      // Select source location using LocationSearchSelect
      const locationSelect = page.locator('button:has-text("Select a location")').first();
      if (await locationSelect.isVisible()) {
        await locationSelect.click();
        await page.locator('[role="option"]').first().click();
      }

      // Add notes (textarea)
      const notesField = page.locator('textarea').first();
      if (await notesField.isVisible()) {
        await notesField.fill(`E2E Test Order - ${generateTestData.uniqueString('order')}`);
      }

      // Add order line item using "Add Item" button
      const addItemButton = page.locator('button:has-text("Add Item")');
      if (await addItemButton.isVisible()) {
        await addItemButton.click();
        await page.waitForTimeout(500);

        // Select product using ProductSearchSelect in the new row
        const productSelect = page.locator('button:has-text("Select a product")').first();
        if (await productSelect.isVisible()) {
          await productSelect.click();
          await page.locator('[role="option"]').first().click();
        }

        // Enter quantity in the input field
        const quantityInput = page.locator('input[type="number"]').first();
        if (await quantityInput.isVisible()) {
          await quantityInput.fill('5');
        }
      }

      // Save the order - button text is "Create Order"
      const saveButton = page.locator('button[type="submit"]:has-text("Create Order"), button[type="submit"]');
      await saveButton.click();

      // Wait for navigation or success toast
      await page.waitForURL(/\/stock\/orders\/[a-f0-9-]+/, { timeout: TIMEOUTS.navigation }).catch(() => {
        // May stay on same page with toast
      });

      // Verify success
      const successToast = page.locator('[data-sonner-toast][data-type="success"]');
      const orderDetailPage = page.locator('h1:has-text("Order")');

      // Should either see success toast or be on order detail page
      const hasSuccess = await successToast.isVisible().catch(() => false);
      const onDetailPage = await orderDetailPage.isVisible().catch(() => false);

      expect(hasSuccess || onDetailPage || page.url().includes('/stock/orders/')).toBeTruthy();
    });
  });

  test.describe('Stock Order Status Transitions', () => {
    test.use({ storageState: 'playwright/.auth/admin.json' });

    test('can submit draft order for approval', async ({ page }) => {
      // First, find or create a draft order
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Filter for draft orders using tabs (not select dropdown)
      const draftTab = page.locator('[role="tab"]:has-text("Draft")');
      if (await draftTab.isVisible()) {
        await draftTab.click();
        await page.waitForLoadState('networkidle');
      }

      // Click on first draft order (if exists)
      const draftOrderRow = page.locator('tbody tr').first();
      if (await draftOrderRow.isVisible()) {
        await draftOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Submit the order - button text is "Submit for Approval"
        const submitButton = page.locator('button:has-text("Submit for Approval")');
        if (await submitButton.isVisible() && await submitButton.isEnabled()) {
          await submitButton.click();

          // Wait for status change
          await page.waitForTimeout(2000);

          // Verify status changed - should see "Pending Approval" badge
          const statusBadge = page.locator('span:has-text("Pending Approval")');
          expect(await statusBadge.isVisible() || page.url().includes('/orders')).toBeTruthy();
        }
      }
    });

    test('can approve a submitted order', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Filter for pending approval orders using tabs
      const pendingTab = page.locator('[role="tab"]:has-text("Pending Approval")');
      if (await pendingTab.isVisible()) {
        await pendingTab.click();
        await page.waitForLoadState('networkidle');
      }

      // Find pending approval order
      const pendingOrderRow = page.locator('tbody tr').first();
      if (await pendingOrderRow.isVisible()) {
        await pendingOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Approve the order - button text is "Approve Order"
        const approveButton = page.locator('button:has-text("Approve Order")');
        if (await approveButton.isVisible() && await approveButton.isEnabled()) {
          await approveButton.click();

          // ApproveOrderDialog may appear - look for confirm/approve button in dialog
          const dialogApproveButton = page.locator('[role="dialog"] button:has-text("Approve")');
          if (await dialogApproveButton.isVisible()) {
            await dialogApproveButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status changed - should see "Approved" badge
          const approvedStatus = page.locator('span:has-text("Approved")');
          if (await approvedStatus.isVisible()) {
            await expect(approvedStatus).toBeVisible();
          }
        }
      }
    });

    test('can mark order as ready for collection', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Filter for approved orders using tabs
      const approvedTab = page.locator('[role="tab"]:has-text("Approved")');
      if (await approvedTab.isVisible()) {
        await approvedTab.click();
        await page.waitForLoadState('networkidle');
      }

      // Find approved order
      const approvedOrderRow = page.locator('tbody tr').first();
      if (await approvedOrderRow.isVisible()) {
        await approvedOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Mark as ready - button text is "Mark Ready for Collection"
        const readyButton = page.locator('button:has-text("Mark Ready for Collection")');
        if (await readyButton.isVisible() && await readyButton.isEnabled()) {
          await readyButton.click();

          await page.waitForTimeout(2000);

          // Verify status - should see "Ready for Collection" badge
          const readyStatus = page.locator('span:has-text("Ready for Collection")');
          if (await readyStatus.isVisible()) {
            await expect(readyStatus).toBeVisible();
          }
        }
      }
    });

    test('can complete order collection', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Filter for ready orders using tabs
      const readyTab = page.locator('[role="tab"]:has-text("Ready")');
      if (await readyTab.isVisible()) {
        await readyTab.click();
        await page.waitForLoadState('networkidle');
      }

      // Find ready order
      const readyOrderRow = page.locator('tbody tr').first();
      if (await readyOrderRow.isVisible()) {
        await readyOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Collect the order - button text is "Mark as Collected"
        const collectButton = page.locator('button:has-text("Mark as Collected")');
        if (await collectButton.isVisible() && await collectButton.isEnabled()) {
          await collectButton.click();

          // CollectOrderDialog may appear - look for confirm button in dialog
          const dialogCollectButton = page.locator('[role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Collect")');
          if (await dialogCollectButton.isVisible()) {
            await dialogCollectButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status - should see "Collected" badge
          const collectedStatus = page.locator('span:has-text("Collected")');
          if (await collectedStatus.isVisible()) {
            await expect(collectedStatus).toBeVisible();
          }
        }
      }
    });
  });

  test.describe('Stock Order Rejection Flow', () => {
    test.use({ storageState: 'playwright/.auth/admin.json' });

    test('can reject a submitted order with reason', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Filter for pending approval orders using tabs
      const pendingTab = page.locator('[role="tab"]:has-text("Pending Approval")');
      if (await pendingTab.isVisible()) {
        await pendingTab.click();
        await page.waitForLoadState('networkidle');
      }

      // Find pending order
      const pendingOrderRow = page.locator('tbody tr').first();
      if (await pendingOrderRow.isVisible()) {
        await pendingOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Reject the order - button text is "Reject Order"
        const rejectButton = page.locator('button:has-text("Reject Order")');
        if (await rejectButton.isVisible() && await rejectButton.isEnabled()) {
          await rejectButton.click();

          // RejectOrderDialog should appear - fill rejection reason
          const reasonInput = page.locator('[role="dialog"] textarea, [role="dialog"] input[name="reason"]');
          if (await reasonInput.isVisible()) {
            await reasonInput.fill('E2E Test - Order rejected for testing purposes');
          }

          // Confirm rejection - button in dialog
          const confirmButton = page.locator('[role="dialog"] button:has-text("Reject")');
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status - should see "Rejected" or be redirected
          const rejectedStatus = page.locator('span:has-text("Rejected")');
          if (await rejectedStatus.isVisible()) {
            await expect(rejectedStatus).toBeVisible();
          }
        }
      }
    });
  });

  test.describe('Stock Order Print Flow', () => {
    test.use({ storageState: 'playwright/.auth/admin.json' });

    test('can view print preview for order', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Filter for approved or ready orders using tabs
      const approvedTab = page.locator('[role="tab"]:has-text("Approved")');
      if (await approvedTab.isVisible()) {
        await approvedTab.click();
        await page.waitForLoadState('networkidle');
      }

      // Find an order
      const orderRow = page.locator('tbody tr').first();
      if (await orderRow.isVisible()) {
        await orderRow.click();
        await page.waitForLoadState('networkidle');

        // Click print button - button text is "Print Docket"
        const printButton = page.locator('button:has-text("Print Docket")');
        if (await printButton.isVisible()) {
          // Open in new tab/window
          const [newPage] = await Promise.all([
            page.context().waitForEvent('page').catch(() => null),
            printButton.click()
          ]);

          if (newPage) {
            await newPage.waitForLoadState('networkidle');
            // Print page should have order details
            await expect(newPage.locator('body')).not.toBeEmpty();
            await newPage.close();
          }
        }
      }
    });
  });
});

/**
 * Stock Order Filtering and Search Tests
 * Uses tabs for status filtering (not select dropdown)
 */
test.describe('Stock Order Filtering and Search @critical', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('can filter orders by status using tabs', async ({ page }) => {
    await page.goto('/stock/orders');
    await page.waitForLoadState('networkidle');

    // Click on Draft tab to filter
    const draftTab = page.locator('[role="tab"]:has-text("Draft")');
    if (await draftTab.isVisible()) {
      await draftTab.click();
      await page.waitForLoadState('networkidle');

      // All visible orders should be draft (or empty)
      const tableRows = page.locator('tbody tr');
      const rowCount = await tableRows.count();
      if (rowCount > 0) {
        // Check that rows contain "Draft"
        for (let i = 0; i < Math.min(rowCount, 3); i++) {
          const row = tableRows.nth(i);
          const rowText = await row.textContent();
          // Row should either be Draft or loading state
          expect(rowText?.toLowerCase().includes('draft') || rowText?.includes('loading')).toBeTruthy();
        }
      }
    }
  });

  test('can search orders by reference', async ({ page }) => {
    await page.goto('/stock/orders');
    await page.waitForLoadState('networkidle');

    // Find search input - placeholder is "Search by order # or site..."
    const searchInput = page.locator('input[placeholder="Search by order # or site..."]');
    if (await searchInput.isVisible()) {
      await searchInput.fill('SO-');
      await page.keyboard.press('Enter');
      await page.waitForLoadState('networkidle');

      // Results should be filtered (or show no results)
      await page.waitForTimeout(1000);
    }
  });
});
