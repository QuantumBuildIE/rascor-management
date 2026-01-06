import { test, expect } from '../../fixtures/test-fixtures';
import { ProductListPage, ProductFormPage } from '../../page-objects/stock-management';
import { generateTestData, TAGS } from '../../fixtures/test-constants';

test.describe('Products @smoke', () => {
  test.use({ storageState: 'playwright/.auth/admin.json' });

  test('should display product list', async ({ page }) => {
    const productListPage = new ProductListPage(page);
    await productListPage.goto();

    await expect(productListPage.pageTitle).toBeVisible();
    await expect(productListPage.table).toBeVisible();
  });

  test('should search products', async ({ page }) => {
    const productListPage = new ProductListPage(page);
    await productListPage.goto();

    await productListPage.search('cement');
    // Results should update based on search
    await productListPage.waitForPageLoad();
  });

  test('should navigate to create product', async ({ page }) => {
    const productListPage = new ProductListPage(page);
    await productListPage.goto();
    await productListPage.clickCreate();

    await expect(page).toHaveURL(/\/stock\/products\/new/);
  });

  test('should create a new product @regression', async ({ page }) => {
    const productFormPage = new ProductFormPage(page);
    await productFormPage.goto();

    const productName = generateTestData.uniqueString('Product');

    await productFormPage.fillForm({
      name: productName,
      sku: generateTestData.uniqueString('SKU'),
      unit: 'Each',
      costPrice: 10.50,
      sellPrice: 15.00,
      reorderLevel: 10,
      reorderQuantity: 50,
    });

    await productFormPage.saveAndWaitForSuccess();

    // Should be redirected to product list
    await expect(page).toHaveURL(/\/stock\/products/);
  });
});

test.describe('Products - Warehouse User', () => {
  test.use({ storageState: 'playwright/.auth/warehouse.json' });

  test('should be able to view products', async ({ page }) => {
    const productListPage = new ProductListPage(page);
    await productListPage.goto();

    await expect(productListPage.table).toBeVisible();
  });
});
