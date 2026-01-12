import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { PurchaseOrder } from "@/types/stock";

export const PURCHASE_ORDERS_KEY = ["purchase-orders"];

export function usePurchaseOrders() {
  return useQuery({
    queryKey: PURCHASE_ORDERS_KEY,
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<PurchaseOrder[]>>("/purchase-orders");
      return response.data.data;
    },
  });
}

export function usePurchaseOrder(id: string) {
  return useQuery({
    queryKey: [...PURCHASE_ORDERS_KEY, id],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<PurchaseOrder>>(`/purchase-orders/${id}`);
      return response.data.data;
    },
    enabled: !!id,
  });
}

export function usePurchaseOrdersBySupplier(supplierId: string) {
  return useQuery({
    queryKey: [...PURCHASE_ORDERS_KEY, "by-supplier", supplierId],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<PurchaseOrder[]>>(
        `/purchase-orders/by-supplier/${supplierId}`
      );
      return response.data.data;
    },
    enabled: !!supplierId,
  });
}

export function usePurchaseOrdersByStatus(status: string) {
  return useQuery({
    queryKey: [...PURCHASE_ORDERS_KEY, "by-status", status],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<PurchaseOrder[]>>(
        `/purchase-orders/by-status/${status}`
      );
      return response.data.data;
    },
    enabled: !!status,
  });
}

interface CreatePurchaseOrderLineDto {
  productId: string;
  quantityOrdered: number;
  unitPrice: number;
}

interface CreatePurchaseOrderDto {
  supplierId: string;
  expectedDate?: string;
  notes?: string;
  lines: CreatePurchaseOrderLineDto[];
}

interface UpdatePurchaseOrderLineDto {
  id?: string;
  productId: string;
  quantityOrdered: number;
  unitPrice: number;
}

interface UpdatePurchaseOrderDto {
  expectedDate?: string;
  notes?: string;
  lines?: UpdatePurchaseOrderLineDto[];
}

export function useCreatePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreatePurchaseOrderDto) => {
      const response = await apiClient.post<ApiResponse<PurchaseOrder>>("/purchase-orders", data);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}

export function useUpdatePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdatePurchaseOrderDto }) => {
      const response = await apiClient.put<ApiResponse<PurchaseOrder>>(
        `/purchase-orders/${id}`,
        data
      );
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}

export function useConfirmPurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.post<ApiResponse<PurchaseOrder>>(
        `/purchase-orders/${id}/confirm`
      );
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}

export function useCancelPurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.post<ApiResponse<PurchaseOrder>>(
        `/purchase-orders/${id}/cancel`
      );
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}

export function useDeletePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/purchase-orders/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PURCHASE_ORDERS_KEY });
    },
  });
}
