import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { StockOrder } from "@/types/stock";

const STOCK_ORDERS_KEY = ["stock-orders"];

export function useStockOrders() {
  return useQuery({
    queryKey: STOCK_ORDERS_KEY,
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockOrder[]>>("/stock-orders");
      return response.data.data;
    },
  });
}

export function useStockOrder(id: string) {
  return useQuery({
    queryKey: [...STOCK_ORDERS_KEY, id],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockOrder>>(`/stock-orders/${id}`);
      return response.data.data;
    },
    enabled: !!id,
  });
}

export function useStockOrderForDocket(id: string, warehouseLocationId: string | undefined) {
  return useQuery({
    queryKey: [...STOCK_ORDERS_KEY, id, "docket", warehouseLocationId],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockOrder>>(
        `/stock-orders/${id}/docket`,
        { params: { warehouseLocationId } }
      );
      return response.data.data;
    },
    enabled: !!id && !!warehouseLocationId,
  });
}

export function useStockOrdersBySite(siteId: string) {
  return useQuery({
    queryKey: [...STOCK_ORDERS_KEY, "by-site", siteId],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockOrder[]>>(
        `/stock-orders/by-site/${siteId}`
      );
      return response.data.data;
    },
    enabled: !!siteId,
  });
}

export function useStockOrdersByStatus(status: string) {
  return useQuery({
    queryKey: [...STOCK_ORDERS_KEY, "by-status", status],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockOrder[]>>(
        `/stock-orders/by-status/${status}`
      );
      return response.data.data;
    },
    enabled: !!status,
  });
}

interface CreateStockOrderLineDto {
  productId: string;
  quantityRequested: number;
}

interface CreateStockOrderDto {
  siteId: string;
  siteName: string;
  orderDate: string;
  sourceLocationId: string;
  requiredDate?: string;
  requestedBy: string;
  notes?: string;
  lines: CreateStockOrderLineDto[];
}

interface UpdateStockOrderLineDto {
  productId: string;
  quantityRequested: number;
}

interface UpdateStockOrderDto {
  siteId?: string;
  siteName?: string;
  sourceLocationId?: string;
  requiredDate?: string;
  notes?: string;
  lines?: UpdateStockOrderLineDto[];
}

export function useCreateStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateStockOrderDto) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>("/stock-orders", data);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useUpdateStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateStockOrderDto }) => {
      const response = await apiClient.put<ApiResponse<StockOrder>>(`/stock-orders/${id}`, data);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useSubmitStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>(`/stock-orders/${id}/submit`);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useApproveStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      approvedBy,
      warehouseLocationId,
    }: {
      id: string;
      approvedBy: string;
      warehouseLocationId: string;
    }) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>(`/stock-orders/${id}/approve`, {
        approvedBy,
        warehouseLocationId,
      });
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useRejectStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, rejectedBy, reason }: { id: string; rejectedBy: string; reason: string }) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>(`/stock-orders/${id}/reject`, {
        rejectedBy,
        reason,
      });
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useReadyForCollectionStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>(
        `/stock-orders/${id}/ready-for-collection`
      );
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useCollectStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, warehouseLocationId }: { id: string; warehouseLocationId: string }) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>(`/stock-orders/${id}/collect`, {
        warehouseLocationId,
      });
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useCancelStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, warehouseLocationId }: { id: string; warehouseLocationId?: string }) => {
      const response = await apiClient.post<ApiResponse<StockOrder>>(`/stock-orders/${id}/cancel`, {
        warehouseLocationId,
      });
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}

export function useDeleteStockOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/stock-orders/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCK_ORDERS_KEY });
    },
  });
}
