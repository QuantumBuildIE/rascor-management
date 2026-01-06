import { Page, Locator } from '@playwright/test';
import { BasePage } from '../BasePage';

/**
 * Stock order list page object
 */
export class StockOrderListPage extends BasePage {
  readonly pageTitle: Locator;
  readonly createButton: Locator;
  readonly searchInput: Locator;
  readonly statusFilter: Locator;
  readonly siteFilter: Locator;
  readonly table: Locator;
  readonly pagination: Locator;

  constructor(page: Page) {
    super(page);
    this.pageTitle = page.locator('h1');
    this.createButton = page.locator('a:has-text("New Order"), button:has-text("Create"), a[href*="/orders/new"]');
    this.searchInput = page.locator('input[placeholder*="Search"], input[name="search"]');
    this.statusFilter = page.locator('[data-filter="status"], select[name="status"]');
    this.siteFilter = page.locator('[data-filter="site"], select[name="siteId"]');
    this.table = page.locator('table');
    this.pagination = page.locator('[data-testid="pagination"], nav[aria-label="pagination"]');
  }

  /**
   * Navigate to stock orders list
   */
  async goto(): Promise<void> {
    await this.page.goto('/stock/orders');
    await this.waitForPageLoad();
  }

  /**
   * Click create new order
   */
  async clickCreate(): Promise<void> {
    await this.createButton.click();
    await this.page.waitForURL(/\/stock\/orders\/new/);
  }

  /**
   * Filter by status
   */
  async filterByStatus(status: string): Promise<void> {
    await this.selectOption(this.statusFilter, status);
    await this.waitForPageLoad();
  }

  /**
   * Filter by site
   */
  async filterBySite(site: string): Promise<void> {
    await this.selectOption(this.siteFilter, site);
    await this.waitForPageLoad();
  }

  /**
   * Get order row by reference
   */
  getOrderRow(reference: string): Locator {
    return this.page.locator(`tr:has-text("${reference}")`);
  }

  /**
   * Click on an order to view details
   */
  async clickOrder(reference: string): Promise<void> {
    await this.getOrderRow(reference).click();
  }

  /**
   * Get order status badge
   */
  getOrderStatus(reference: string): Locator {
    return this.getOrderRow(reference).locator('.badge, [data-status]');
  }

  /**
   * Edit an order
   */
  async editOrder(reference: string): Promise<void> {
    const row = this.getOrderRow(reference);
    await row.locator('a:has-text("Edit"), button:has-text("Edit")').click();
  }

  /**
   * Get order count
   */
  async getOrderCount(): Promise<number> {
    return await this.getTableRowCount();
  }
}
