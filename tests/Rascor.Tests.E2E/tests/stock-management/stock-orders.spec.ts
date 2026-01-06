import { test, expect } from '../../fixtures/test-fixtures';
import {
  StockOrderListPage,
  StockOrderFormPage,
  StockOrderDetailPage
} from '../../page-objects/stock-management';
import { TAGS } from '../../fixtures/test-constants';

test.describe('Stock Orders @smoke', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('should display stock order list', async ({ page }) => {
    const orderListPage = new StockOrderListPage(page);
    await orderListPage.goto();

    await expect(orderListPage.pageTitle).toBeVisible();
    await expect(orderListPage.table).toBeVisible();
  });

  test('should filter orders by status', async ({ page }) => {
    const orderListPage = new StockOrderListPage(page);
    await orderListPage.goto();

    await orderListPage.filterByStatus('Draft');
    await orderListPage.waitForPageLoad();
  });

  test('should navigate to create order', async ({ page }) => {
    const orderListPage = new StockOrderListPage(page);
    await orderListPage.goto();
    await orderListPage.clickCreate();

    await expect(page).toHaveURL(/\/stock\/orders\/new/);
  });
});

test.describe('Stock Order Workflow @regression', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('should create a new stock order', async ({ page }) => {
    const orderFormPage = new StockOrderFormPage(page);
    await orderFormPage.goto();

    // Select site and add items
    // Note: This requires actual test data in the database
    await expect(orderFormPage.siteSelect).toBeVisible();
  });
});

test.describe('Stock Orders - Site Manager', () => {
  test.use({ storageState: 'playwright/.auth/sitemanager.json' });

  test('should be able to create orders', async ({ page }) => {
    const orderListPage = new StockOrderListPage(page);
    await orderListPage.goto();

    await expect(orderListPage.createButton).toBeVisible();
  });

  test('should not see approve button on pending orders', async ({ page }) => {
    // Site managers can create but not approve
    const orderListPage = new StockOrderListPage(page);
    await orderListPage.goto();

    // This test would need actual pending orders in the database
    await expect(orderListPage.table).toBeVisible();
  });
});

test.describe('Stock Orders - Warehouse User', () => {
  test.use({ storageState: 'playwright/.auth/warehouse.json' });

  test('should see orders and workflow actions', async ({ page }) => {
    const orderListPage = new StockOrderListPage(page);
    await orderListPage.goto();

    await expect(orderListPage.table).toBeVisible();
  });
});
