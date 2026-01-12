import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { StockLocation } from "@/types/stock";

export const LOCATIONS_KEY = ["stock-locations"];

export function useLocations() {
  return useQuery({
    queryKey: LOCATIONS_KEY,
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockLocation[]>>(
        "/stock-locations"
      );
      return response.data.data ?? [];
    },
  });
}

export function useLocation(id: string) {
  return useQuery({
    queryKey: [...LOCATIONS_KEY, id],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockLocation>>(
        `/stock-locations/${id}`
      );
      return response.data.data;
    },
    enabled: !!id,
  });
}
