import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getGoodsReceipts,
  getGoodsReceipt,
  getGoodsReceiptsBySupplier,
  getGoodsReceiptsByPurchaseOrder,
  createGoodsReceipt,
  deleteGoodsReceipt,
  type CreateGoodsReceiptDto,
} from "./goods-receipts";
import { PURCHASE_ORDERS_KEY } from "./use-purchase-orders";

export const GOODS_RECEIPTS_KEY = ["goods-receipts"];

export function useGoodsReceipts() {
  return useQuery({
    queryKey: GOODS_RECEIPTS_KEY,
    queryFn: getGoodsReceipts,
  });
}

export function useGoodsReceipt(id: string) {
  return useQuery({
    queryKey: [...GOODS_RECEIPTS_KEY, id],
    queryFn: () => getGoodsReceipt(id),
    enabled: !!id,
  });
}

export function useGoodsReceiptsBySupplier(supplierId: string) {
  return useQuery({
    queryKey: [...GOODS_RECEIPTS_KEY, "by-supplier", supplierId],
    queryFn: () => getGoodsReceiptsBySupplier(supplierId),
    enabled: !!supplierId,
  });
}

export function useGoodsReceiptsByPurchaseOrder(purchaseOrderId: string) {
  return useQuery({
    queryKey: [...GOODS_RECEIPTS_KEY, "by-purchase-order", purchaseOrderId],
    queryFn: () => getGoodsReceiptsByPurchaseOrder(purchaseOrderId),
    enabled: !!purchaseOrderId,
  });
}

export function useCreateGoodsReceipt() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateGoodsReceiptDto) => createGoodsReceipt(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GOODS_RECEIPTS_KEY });
      // Also invalidate purchase orders as receiving goods updates PO status
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}

export function useDeleteGoodsReceipt() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteGoodsReceipt(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GOODS_RECEIPTS_KEY });
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}

export type { CreateGoodsReceiptDto, CreateGoodsReceiptLineDto } from "./goods-receipts";
