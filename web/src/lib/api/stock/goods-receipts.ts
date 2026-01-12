import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { GoodsReceipt } from "@/types/stock";

export interface CreateGoodsReceiptLineDto {
  productId: string;
  quantityReceived: number;
  notes?: string;
  quantityRejected?: number;
  rejectionReason?: string;
  batchNumber?: string;
  expiryDate?: string;
  bayLocationId?: string;
}

export interface CreateGoodsReceiptDto {
  supplierId: string;
  deliveryNoteRef?: string;
  purchaseOrderId?: string;
  locationId: string;
  receivedBy: string;
  notes?: string;
  lines: CreateGoodsReceiptLineDto[];
}

export async function getGoodsReceipts(): Promise<GoodsReceipt[]> {
  const response = await apiClient.get<ApiResponse<GoodsReceipt[]>>("/goods-receipts");
  return response.data.data ?? [];
}

export async function getGoodsReceipt(id: string): Promise<GoodsReceipt> {
  const response = await apiClient.get<ApiResponse<GoodsReceipt>>(
    `/goods-receipts/${id}`
  );
  return response.data.data;
}

export async function getGoodsReceiptsBySupplier(supplierId: string): Promise<GoodsReceipt[]> {
  const response = await apiClient.get<ApiResponse<GoodsReceipt[]>>(
    `/goods-receipts/by-supplier/${supplierId}`
  );
  return response.data.data ?? [];
}

export async function getGoodsReceiptsByPurchaseOrder(purchaseOrderId: string): Promise<GoodsReceipt[]> {
  const response = await apiClient.get<ApiResponse<GoodsReceipt[]>>(
    `/goods-receipts/by-purchase-order/${purchaseOrderId}`
  );
  return response.data.data ?? [];
}

export async function createGoodsReceipt(
  data: CreateGoodsReceiptDto
): Promise<GoodsReceipt> {
  const response = await apiClient.post<ApiResponse<GoodsReceipt>>(
    "/goods-receipts",
    data
  );
  return response.data.data;
}

export async function deleteGoodsReceipt(id: string): Promise<void> {
  await apiClient.delete(`/goods-receipts/${id}`);
}
