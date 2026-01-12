import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { StockLevel } from "@/types/stock";

export interface GetStockLevelsParams {
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: "asc" | "desc";
  search?: string;
  locationId?: string;
}

export async function getStockLevels(
  params?: GetStockLevelsParams
): Promise<StockLevel[]> {
  let url = "/stock-levels";

  // If locationId is provided, use the by-location endpoint
  if (params?.locationId) {
    url = `/stock-levels/by-location/${params.locationId}`;
  }

  const response = await apiClient.get<ApiResponse<StockLevel[]>>(url);
  let data = response.data.data ?? [];

  // Client-side search filtering since the API doesn't support it
  if (params?.search) {
    const searchLower = params.search.toLowerCase();
    data = data.filter(
      (item) =>
        item.productCode.toLowerCase().includes(searchLower) ||
        item.productName.toLowerCase().includes(searchLower)
    );
  }

  // Client-side sorting
  if (params?.sortColumn) {
    const sortCol = params.sortColumn as keyof StockLevel;
    const sortDir = params.sortDirection === "desc" ? -1 : 1;

    data = [...data].sort((a, b) => {
      const aValue = a[sortCol];
      const bValue = b[sortCol];

      if (aValue === null || aValue === undefined) return 1;
      if (bValue === null || bValue === undefined) return -1;

      if (typeof aValue === "string" && typeof bValue === "string") {
        return aValue.localeCompare(bValue) * sortDir;
      }
      if (typeof aValue === "number" && typeof bValue === "number") {
        return (aValue - bValue) * sortDir;
      }
      return String(aValue).localeCompare(String(bValue)) * sortDir;
    });
  }

  return data;
}

export async function getStockLevel(id: string): Promise<StockLevel> {
  const response = await apiClient.get<ApiResponse<StockLevel>>(
    `/stock-levels/${id}`
  );
  return response.data.data;
}

export async function getStockLevelsByLocation(
  locationId: string
): Promise<StockLevel[]> {
  const response = await apiClient.get<ApiResponse<StockLevel[]>>(
    `/stock-levels/by-location/${locationId}`
  );
  return response.data.data ?? [];
}

export async function getLowStockLevels(): Promise<StockLevel[]> {
  const response = await apiClient.get<ApiResponse<StockLevel[]>>(
    "/stock-levels/low-stock"
  );
  return response.data.data ?? [];
}

export async function getStockLevelByProductAndLocation(
  productId: string,
  locationId: string
): Promise<StockLevel> {
  const response = await apiClient.get<ApiResponse<StockLevel>>(
    `/stock-levels/by-product/${productId}/location/${locationId}`
  );
  return response.data.data;
}
