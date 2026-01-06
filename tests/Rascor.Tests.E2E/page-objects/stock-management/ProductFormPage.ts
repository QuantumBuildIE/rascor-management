import { Page, Locator } from '@playwright/test';
import { BasePage } from '../BasePage';

/**
 * Product form page object (create/edit)
 */
export class ProductFormPage extends BasePage {
  readonly pageTitle: Locator;
  readonly nameInput: Locator;
  readonly skuInput: Locator;
  readonly descriptionInput: Locator;
  readonly categorySelect: Locator;
  readonly supplierSelect: Locator;
  readonly unitInput: Locator;
  readonly costPriceInput: Locator;
  readonly sellPriceInput: Locator;
  readonly reorderLevelInput: Locator;
  readonly reorderQuantityInput: Locator;
  readonly isActiveCheckbox: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
    this.pageTitle = page.locator('h1');
    this.nameInput = page.locator('[name="name"], #name');
    this.skuInput = page.locator('[name="sku"], #sku');
    this.descriptionInput = page.locator('[name="description"], #description');
    this.categorySelect = page.locator('[name="categoryId"], #categoryId');
    this.supplierSelect = page.locator('[name="supplierId"], #supplierId');
    this.unitInput = page.locator('[name="unit"], #unit');
    this.costPriceInput = page.locator('[name="costPrice"], #costPrice');
    this.sellPriceInput = page.locator('[name="sellPrice"], #sellPrice');
    this.reorderLevelInput = page.locator('[name="reorderLevel"], #reorderLevel');
    this.reorderQuantityInput = page.locator('[name="reorderQuantity"], #reorderQuantity');
    this.isActiveCheckbox = page.locator('[name="isActive"], #isActive');
    this.saveButton = page.locator('button[type="submit"], button:has-text("Save")');
    this.cancelButton = page.locator('button:has-text("Cancel"), a:has-text("Cancel")');
  }

  /**
   * Navigate to create new product
   */
  async goto(): Promise<void> {
    await this.page.goto('/stock/products/new');
    await this.waitForPageLoad();
  }

  /**
   * Navigate to edit product
   */
  async gotoEdit(id: string): Promise<void> {
    await this.page.goto(`/stock/products/${id}/edit`);
    await this.waitForPageLoad();
  }

  /**
   * Fill the product form
   */
  async fillForm(data: {
    name: string;
    sku?: string;
    description?: string;
    categoryId?: string;
    supplierId?: string;
    unit?: string;
    costPrice?: number;
    sellPrice?: number;
    reorderLevel?: number;
    reorderQuantity?: number;
  }): Promise<void> {
    await this.nameInput.fill(data.name);

    if (data.sku) {
      await this.skuInput.fill(data.sku);
    }
    if (data.description) {
      await this.descriptionInput.fill(data.description);
    }
    if (data.categoryId) {
      await this.selectOption(this.categorySelect, data.categoryId);
    }
    if (data.supplierId) {
      await this.selectOption(this.supplierSelect, data.supplierId);
    }
    if (data.unit) {
      await this.unitInput.fill(data.unit);
    }
    if (data.costPrice !== undefined) {
      await this.costPriceInput.fill(data.costPrice.toString());
    }
    if (data.sellPrice !== undefined) {
      await this.sellPriceInput.fill(data.sellPrice.toString());
    }
    if (data.reorderLevel !== undefined) {
      await this.reorderLevelInput.fill(data.reorderLevel.toString());
    }
    if (data.reorderQuantity !== undefined) {
      await this.reorderQuantityInput.fill(data.reorderQuantity.toString());
    }
  }

  /**
   * Save the form
   */
  async save(): Promise<void> {
    await this.saveButton.click();
  }

  /**
   * Save and wait for success
   */
  async saveAndWaitForSuccess(): Promise<void> {
    await this.save();
    await this.waitForToastSuccess();
  }

  /**
   * Cancel form
   */
  async cancel(): Promise<void> {
    await this.cancelButton.click();
  }
}
