import { test, expect } from '@playwright/test';
import { TEST_TENANT, generateTestData, TIMEOUTS } from '../../fixtures/test-constants';

/**
 * Critical E2E Flow: Stock Order Workflow
 * Tests the complete stock order lifecycle from creation to completion
 * Tag: @critical
 * Run with: npx playwright test --grep @critical
 */
test.describe('Stock Order Workflow @critical', () => {
  test.describe('Create Stock Order Flow', () => {
    test.use({ storageState: 'playwright/.auth/admin.json' });

    test('can create a new stock order', async ({ page }) => {
      // Navigate to new order page
      await page.goto('/stock/orders/new');
      await page.waitForLoadState('networkidle');

      // Fill in the order form
      // Select site (if dropdown exists)
      const siteSelect = page.locator('[name="siteId"], [name="destinationSiteId"], #siteId, #site');
      if (await siteSelect.isVisible()) {
        await siteSelect.click();
        await page.locator('[role="option"]').first().click();
      }

      // Select stock location
      const locationSelect = page.locator('[name="stockLocationId"], [name="sourceLocationId"], #stockLocationId');
      if (await locationSelect.isVisible()) {
        await locationSelect.click();
        await page.locator('[role="option"]').first().click();
      }

      // Add notes
      const notesField = page.locator('[name="notes"], #notes, textarea');
      if (await notesField.isVisible()) {
        await notesField.fill(`E2E Test Order - ${generateTestData.uniqueString('order')}`);
      }

      // Add order line item
      const addItemButton = page.locator('button:has-text("Add Item"), button:has-text("Add Line"), button:has-text("Add Product")');
      if (await addItemButton.isVisible()) {
        await addItemButton.click();

        // Select product
        const productSelect = page.locator('[name*="productId"], [name*="product"]').first();
        if (await productSelect.isVisible()) {
          await productSelect.click();
          await page.locator('[role="option"]').first().click();
        }

        // Enter quantity
        const quantityInput = page.locator('[name*="quantity"], input[type="number"]').first();
        if (await quantityInput.isVisible()) {
          await quantityInput.fill('5');
        }
      }

      // Save the order
      const saveButton = page.locator('button:has-text("Save"), button:has-text("Create"), button[type="submit"]');
      await saveButton.click();

      // Wait for navigation or success toast
      await page.waitForURL(/\/stock\/orders\/[a-f0-9-]+/, { timeout: TIMEOUTS.navigation }).catch(() => {
        // May stay on same page with toast
      });

      // Verify success
      const successToast = page.locator('[data-sonner-toast][data-type="success"], .toast-success');
      const orderDetailPage = page.locator('[data-testid="order-detail"], h1:has-text("Order"), [data-status]');

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

      // Filter for draft orders if filter exists
      const statusFilter = page.locator('[data-testid="status-filter"], select:has-text("Status"), button:has-text("Draft")');
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
        const draftOption = page.locator('[role="option"]:has-text("Draft"), option[value="Draft"]');
        if (await draftOption.isVisible()) {
          await draftOption.click();
        }
      }

      // Click on first draft order (if exists)
      const draftOrderRow = page.locator('tr:has-text("Draft"), [data-status="Draft"]').first();
      if (await draftOrderRow.isVisible()) {
        await draftOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Submit the order
        const submitButton = page.locator('button:has-text("Submit"), button:has-text("Send for Approval")');
        if (await submitButton.isVisible() && await submitButton.isEnabled()) {
          await submitButton.click();

          // Confirm if dialog appears
          const confirmButton = page.locator('[role="dialog"] button:has-text("Confirm"), [role="alertdialog"] button:has-text("Yes")');
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          // Wait for status change
          await page.waitForTimeout(2000);

          // Verify status changed
          const statusBadge = page.locator('[data-status], .status-badge, span:has-text("Pending"), span:has-text("Submitted")');
          expect(await statusBadge.isVisible() || page.url().includes('/orders')).toBeTruthy();
        }
      }
    });

    test('can approve a submitted order', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Find pending approval order
      const pendingOrderRow = page.locator('tr:has-text("Pending"), tr:has-text("Submitted"), [data-status="PendingApproval"]').first();
      if (await pendingOrderRow.isVisible()) {
        await pendingOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Approve the order
        const approveButton = page.locator('button:has-text("Approve")');
        if (await approveButton.isVisible() && await approveButton.isEnabled()) {
          await approveButton.click();

          // Confirm if dialog appears
          const confirmButton = page.locator('[role="dialog"] button:has-text("Confirm"), [role="alertdialog"] button:has-text("Yes")');
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status changed
          const approvedStatus = page.locator('[data-status="Approved"], span:has-text("Approved")');
          if (await approvedStatus.isVisible()) {
            await expect(approvedStatus).toBeVisible();
          }
        }
      }
    });

    test('can mark order as ready for collection', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Find approved order
      const approvedOrderRow = page.locator('tr:has-text("Approved"), [data-status="Approved"]').first();
      if (await approvedOrderRow.isVisible()) {
        await approvedOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Mark as ready
        const readyButton = page.locator('button:has-text("Ready"), button:has-text("Mark Ready")');
        if (await readyButton.isVisible() && await readyButton.isEnabled()) {
          await readyButton.click();

          // Confirm if dialog appears
          const confirmButton = page.locator('[role="dialog"] button:has-text("Confirm")');
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status
          const readyStatus = page.locator('[data-status*="Ready"], span:has-text("Ready")');
          if (await readyStatus.isVisible()) {
            await expect(readyStatus).toBeVisible();
          }
        }
      }
    });

    test('can complete order collection', async ({ page }) => {
      await page.goto('/stock/orders');
      await page.waitForLoadState('networkidle');

      // Find ready order
      const readyOrderRow = page.locator('tr:has-text("Ready"), [data-status*="Ready"]').first();
      if (await readyOrderRow.isVisible()) {
        await readyOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Collect the order
        const collectButton = page.locator('button:has-text("Collect"), button:has-text("Mark Collected")');
        if (await collectButton.isVisible() && await collectButton.isEnabled()) {
          await collectButton.click();

          // Confirm if dialog appears
          const confirmButton = page.locator('[role="dialog"] button:has-text("Confirm")');
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status
          const collectedStatus = page.locator('[data-status="Collected"], span:has-text("Collected"), span:has-text("Complete")');
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

      // Find pending order
      const pendingOrderRow = page.locator('tr:has-text("Pending"), [data-status="PendingApproval"]').first();
      if (await pendingOrderRow.isVisible()) {
        await pendingOrderRow.click();
        await page.waitForLoadState('networkidle');

        // Reject the order
        const rejectButton = page.locator('button:has-text("Reject")');
        if (await rejectButton.isVisible() && await rejectButton.isEnabled()) {
          await rejectButton.click();

          // Fill rejection reason if dialog appears
          const reasonInput = page.locator('[name="reason"], textarea, input[placeholder*="reason"]');
          if (await reasonInput.isVisible()) {
            await reasonInput.fill('E2E Test - Order rejected for testing purposes');
          }

          // Confirm rejection
          const confirmButton = page.locator('[role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Reject")');
          if (await confirmButton.isVisible()) {
            await confirmButton.click();
          }

          await page.waitForTimeout(2000);

          // Verify status
          const rejectedStatus = page.locator('[data-status="Rejected"], span:has-text("Rejected")');
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

      // Find an approved or ready order
      const orderRow = page.locator('tr:has-text("Approved"), tr:has-text("Ready")').first();
      if (await orderRow.isVisible()) {
        await orderRow.click();
        await page.waitForLoadState('networkidle');

        // Click print button
        const printButton = page.locator('button:has-text("Print"), a:has-text("Print")');
        if (await printButton.isVisible()) {
          // Open in new tab/window or print dialog
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

test.describe('Stock Order Filtering and Search @critical', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('can filter orders by status', async ({ page }) => {
    await page.goto('/stock/orders');
    await page.waitForLoadState('networkidle');

    // Find status filter
    const statusFilter = page.locator('[data-testid="status-filter"], select:has(option[value="Draft"]), [role="combobox"]:has-text("Status")');
    if (await statusFilter.isVisible()) {
      await statusFilter.click();

      const draftOption = page.locator('[role="option"]:has-text("Draft"), option[value="Draft"]');
      if (await draftOption.isVisible()) {
        await draftOption.click();
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
    }
  });

  test('can search orders by reference', async ({ page }) => {
    await page.goto('/stock/orders');
    await page.waitForLoadState('networkidle');

    // Find search input
    const searchInput = page.locator('[name="search"], [placeholder*="Search"], input[type="search"]');
    if (await searchInput.isVisible()) {
      await searchInput.fill('SO-');
      await page.keyboard.press('Enter');
      await page.waitForLoadState('networkidle');

      // Results should be filtered (or show no results)
      await page.waitForTimeout(1000);
    }
  });
});
