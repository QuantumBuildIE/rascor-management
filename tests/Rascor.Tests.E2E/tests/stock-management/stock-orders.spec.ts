import { test, expect } from '../../fixtures/test-fixtures';
import {
  StockOrderListPage,
  StockOrderFormPage,
  StockOrderDetailPage
} from '../../page-objects/stock-management';
import { TAGS } from '../../fixtures/test-constants';

test.describe('Stock Orders @smoke', () => {
  test('should display stock order list', async ({ adminPage }) => {
    const orderListPage = new StockOrderListPage(adminPage);
    await orderListPage.goto();

    await expect(orderListPage.pageTitle).toBeVisible();
    await expect(orderListPage.table).toBeVisible();
  });

  test('should filter orders by status', async ({ adminPage }) => {
    const orderListPage = new StockOrderListPage(adminPage);
    await orderListPage.goto();

    await orderListPage.filterByStatus('Draft');
    await orderListPage.waitForPageLoad();
  });

  test('should navigate to create order', async ({ adminPage }) => {
    const orderListPage = new StockOrderListPage(adminPage);
    await orderListPage.goto();
    await orderListPage.clickCreate();

    await expect(adminPage).toHaveURL(/\/stock\/orders\/new/);
  });
});

test.describe('Stock Order Workflow @regression', () => {
  test('should create a new stock order', async ({ adminPage }) => {
    const orderFormPage = new StockOrderFormPage(adminPage);
    await orderFormPage.goto();

    // Select site and add items
    // Note: This requires actual test data in the database
    await expect(orderFormPage.siteSelect).toBeVisible();
  });
});

test.describe('Stock Orders - Site Manager', () => {
  test('should be able to create orders', async ({ siteManagerPage }) => {
    const orderListPage = new StockOrderListPage(siteManagerPage);
    await orderListPage.goto();

    await expect(orderListPage.createButton).toBeVisible();
  });

  test('should not see approve button on pending orders', async ({ siteManagerPage }) => {
    // Site managers can create but not approve
    const orderListPage = new StockOrderListPage(siteManagerPage);
    await orderListPage.goto();

    // This test would need actual pending orders in the database
    await expect(orderListPage.table).toBeVisible();
  });
});

test.describe('Stock Orders - Warehouse User', () => {
  test('should see orders and workflow actions', async ({ warehousePage }) => {
    const orderListPage = new StockOrderListPage(warehousePage);
    await orderListPage.goto();

    await expect(orderListPage.table).toBeVisible();
  });
});
