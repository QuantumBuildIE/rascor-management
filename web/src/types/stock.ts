export interface Category {
  id: string;
  categoryName: string;
  sortOrder: number;
  isActive: boolean;
}

export interface Product {
  id: string;
  productCode: string;
  productName: string;
  categoryId: string;
  categoryName: string;
  supplierId: string | null;
  supplierName: string | null;
  unitType: string;
  baseRate: number;
  reorderLevel: number;
  reorderQuantity: number;
  leadTimeDays: number;
  isActive: boolean;
  qrCodeData: string | null;
  costPrice: number | null;
  sellPrice: number | null;
  productType: string | null;
  marginAmount: number | null;
  marginPercent: number | null;
  imageFileName: string | null;
  imageUrl: string | null;
}

export interface StockLevel {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  locationId: string;
  locationCode: string;
  locationName: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  quantityOnOrder: number;
  binLocation: string | null;
  bayLocationId: string | null;
  bayCode: string | null;
  bayName: string | null;
  lastMovementDate: string | null;
  lastCountDate: string | null;
  reorderLevel: number;
}

export interface StockOrderLine {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  quantityRequested: number;
  quantityIssued: number;
  unitPrice: number;
  lineTotal: number;
  bayCode: string | null;
}

export interface StockOrder {
  id: string;
  orderNumber: string;
  siteId: string;
  siteName: string;
  orderDate: string;
  requiredDate: string | null;
  status: StockOrderStatus;
  orderTotal: number;
  requestedBy: string;
  approvedBy: string | null;
  approvedDate: string | null;
  collectedDate: string | null;
  notes: string | null;
  sourceLocationId: string;
  sourceLocationName: string;
  lines: StockOrderLine[];
  sourceProposalId?: string;
  sourceProposalNumber?: string;
}

export type StockOrderStatus =
  | "Draft"
  | "PendingApproval"
  | "Approved"
  | "AwaitingPick"
  | "ReadyForCollection"
  | "Collected"
  | "Cancelled";

export interface PurchaseOrderLine {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitPrice: number;
  unitType: string;
  lineTotal: number;
  lineStatus: string;
}

export interface PurchaseOrder {
  id: string;
  poNumber: string;
  supplierId: string;
  supplierName: string;
  orderDate: string;
  expectedDate: string | null;
  status: PurchaseOrderStatus;
  totalValue: number;
  notes: string | null;
  lines: PurchaseOrderLine[];
}

export type PurchaseOrderStatus =
  | "Draft"
  | "Confirmed"
  | "PartiallyReceived"
  | "FullyReceived"
  | "Cancelled";

export interface Supplier {
  id: string;
  supplierCode: string;
  supplierName: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  paymentTerms: string | null;
  isActive: boolean;
}

export interface StockLocation {
  id: string;
  locationCode: string;
  locationName: string;
  locationType: string;
  isActive: boolean;
}

export interface BayLocation {
  id: string;
  bayCode: string;
  bayName: string | null;
  stockLocationId: string;
  stockLocationCode: string;
  stockLocationName: string;
  capacity: number | null;
  isActive: boolean;
  notes: string | null;
}

export interface GoodsReceiptLine {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  purchaseOrderLineId: string | null;
  quantityReceived: number;
  notes: string | null;
  quantityRejected: number;
  rejectionReason: string | null;
  batchNumber: string | null;
  expiryDate: string | null;
  bayLocationId: string | null;
  bayCode: string | null;
}

export interface GoodsReceipt {
  id: string;
  grnNumber: string;
  supplierId: string;
  supplierName: string;
  deliveryNoteRef: string | null;
  purchaseOrderId: string | null;
  poNumber: string | null;
  locationId: string;
  locationName: string;
  receiptDate: string;
  receivedBy: string;
  notes: string | null;
  lines: GoodsReceiptLine[];
}

export interface StocktakeLine {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  systemQuantity: number;
  countedQuantity: number | null;
  variance: number | null;
  adjustmentCreated: boolean;
  varianceReason: string | null;
  bayLocationId: string | null;
  bayCode: string | null;
}

export interface Stocktake {
  id: string;
  stocktakeNumber: string;
  locationId: string;
  locationCode: string;
  locationName: string;
  countDate: string;
  status: StocktakeStatus;
  countedBy: string;
  notes: string | null;
  lines: StocktakeLine[];
}

export type StocktakeStatus =
  | "Draft"
  | "InProgress"
  | "Completed"
  | "Cancelled";

// Report types
export interface ProductValueByMonth {
  month: string;
  productName: string;
  value: number;
}

export interface ProductValueBySite {
  siteName: string;
  productName: string;
  value: number;
}

export interface ProductValueByWeek {
  weekStartDate: string;
  productName: string;
  value: number;
}

// Stock Valuation Report types
export interface StockValuationItem {
  productId: string;
  productCode: string;
  productName: string;
  categoryId: string | null;
  categoryName: string | null;
  locationId: string;
  locationName: string;
  bayCode: string | null;
  quantityOnHand: number;
  costPrice: number | null;
  totalValue: number;
}

export interface StockValuationReport {
  items: StockValuationItem[];
  totalProducts: number;
  totalQuantity: number;
  totalValue: number;
  generatedAt: string;
}
