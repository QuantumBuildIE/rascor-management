import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { BayLocation } from "@/types/stock";

export interface CreateBayLocationDto {
  bayCode: string;
  bayName?: string;
  stockLocationId: string;
  capacity?: number;
  isActive?: boolean;
  notes?: string;
}

export interface UpdateBayLocationDto {
  bayCode?: string;
  bayName?: string;
  stockLocationId?: string;
  capacity?: number;
  isActive?: boolean;
  notes?: string;
}

export async function getBayLocations(stockLocationId?: string): Promise<BayLocation[]> {
  const params = stockLocationId ? { stockLocationId } : {};
  const response = await apiClient.get<ApiResponse<BayLocation[]>>("/bay-locations", { params });
  return response.data.data ?? [];
}

export async function getBayLocation(id: string): Promise<BayLocation> {
  const response = await apiClient.get<ApiResponse<BayLocation>>(
    `/bay-locations/${id}`
  );
  return response.data.data;
}

export async function getBayLocationsByLocation(stockLocationId: string): Promise<BayLocation[]> {
  const response = await apiClient.get<ApiResponse<BayLocation[]>>(
    `/bay-locations/by-location/${stockLocationId}`
  );
  return response.data.data ?? [];
}

export async function createBayLocation(
  data: CreateBayLocationDto
): Promise<BayLocation> {
  const response = await apiClient.post<ApiResponse<BayLocation>>(
    "/bay-locations",
    data
  );
  return response.data.data;
}

export async function updateBayLocation(
  id: string,
  data: UpdateBayLocationDto
): Promise<BayLocation> {
  const response = await apiClient.put<ApiResponse<BayLocation>>(
    `/bay-locations/${id}`,
    data
  );
  return response.data.data;
}

export async function deleteBayLocation(id: string): Promise<void> {
  await apiClient.delete(`/bay-locations/${id}`);
}
