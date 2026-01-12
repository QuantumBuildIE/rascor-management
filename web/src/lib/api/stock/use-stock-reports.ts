import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type {
  ProductValueByMonth,
  ProductValueBySite,
  ProductValueByWeek,
  StockValuationReport,
} from "@/types/stock";

const STOCK_REPORTS_KEY = ["stock-reports"];

export function useProductsByMonth(months = 4, topN = 10) {
  return useQuery({
    queryKey: [...STOCK_REPORTS_KEY, "products-by-month", months, topN],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<ProductValueByMonth[]>>(
        "/stock/reports/products-by-month",
        { params: { months, topN } }
      );
      return response.data.data;
    },
  });
}

export function useProductsBySite(topN = 10) {
  return useQuery({
    queryKey: [...STOCK_REPORTS_KEY, "products-by-site", topN],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<ProductValueBySite[]>>(
        "/stock/reports/products-by-site",
        { params: { topN } }
      );
      return response.data.data;
    },
  });
}

export function useProductsByWeek(weeks = 12, topN = 10) {
  return useQuery({
    queryKey: [...STOCK_REPORTS_KEY, "products-by-week", weeks, topN],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<ProductValueByWeek[]>>(
        "/stock/reports/products-by-week",
        { params: { weeks, topN } }
      );
      return response.data.data;
    },
  });
}

export interface StockValuationParams {
  locationId?: string;
  categoryId?: string;
}

export function useStockValuation(params: StockValuationParams = {}) {
  return useQuery({
    queryKey: [...STOCK_REPORTS_KEY, "valuation", params.locationId, params.categoryId],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StockValuationReport>>(
        "/stock/reports/valuation",
        { params }
      );
      return response.data.data;
    },
  });
}
