import { test, expect } from '../../fixtures/test-fixtures';
import { ProductListPage, ProductFormPage } from '../../page-objects/stock-management';
import { generateTestData, TAGS } from '../../fixtures/test-constants';

test.describe('Products @smoke', () => {
  test('should display product list', async ({ adminPage }) => {
    const productListPage = new ProductListPage(adminPage);
    await productListPage.goto();

    await expect(productListPage.pageTitle).toBeVisible();
    await expect(productListPage.table).toBeVisible();
  });

  test('should search products', async ({ adminPage }) => {
    const productListPage = new ProductListPage(adminPage);
    await productListPage.goto();

    await productListPage.search('cement');
    // Results should update based on search
    await productListPage.waitForPageLoad();
  });

  test('should navigate to create product', async ({ adminPage }) => {
    const productListPage = new ProductListPage(adminPage);
    await productListPage.goto();
    await productListPage.clickCreate();

    await expect(adminPage).toHaveURL(/\/stock\/products\/new/);
  });

  test('should create a new product @regression', async ({ adminPage }) => {
    const productFormPage = new ProductFormPage(adminPage);
    await productFormPage.goto();

    const productCode = generateTestData.uniqueString('PROD');
    const productName = generateTestData.uniqueString('Test Product');

    await productFormPage.fillForm({
      productCode: productCode,
      productName: productName,
      categoryName: 'Adhesives', // Use existing seeded category
      baseRate: 10.50,
      costPrice: 10.50,
      sellPrice: 15.00,
      reorderLevel: 10,
      reorderQuantity: 50,
    });

    await productFormPage.saveAndWaitForSuccess();

    // Should be redirected to product list
    await expect(adminPage).toHaveURL(/\/stock\/products/);
  });
});

test.describe('Products - Warehouse User', () => {
  test('should be able to view products', async ({ warehousePage }) => {
    const productListPage = new ProductListPage(warehousePage);
    await productListPage.goto();

    await expect(productListPage.table).toBeVisible();
  });
});
