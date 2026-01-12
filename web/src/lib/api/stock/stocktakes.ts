import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { Stocktake } from "@/types/stock";

export interface CreateStocktakeDto {
  locationId: string;
  countedBy: string;
  notes?: string;
}

export interface UpdateStocktakeLineDto {
  countedQuantity: number | null;
  varianceReason?: string | null;
}

export async function getStocktakes(): Promise<Stocktake[]> {
  const response = await apiClient.get<ApiResponse<Stocktake[]>>("/stocktakes");
  return response.data.data ?? [];
}

export async function getStocktake(id: string): Promise<Stocktake> {
  const response = await apiClient.get<ApiResponse<Stocktake>>(
    `/stocktakes/${id}`
  );
  return response.data.data;
}

export async function getStocktakesByLocation(locationId: string): Promise<Stocktake[]> {
  const response = await apiClient.get<ApiResponse<Stocktake[]>>(
    `/stocktakes/by-location/${locationId}`
  );
  return response.data.data ?? [];
}

export async function createStocktake(
  data: CreateStocktakeDto
): Promise<Stocktake> {
  const response = await apiClient.post<ApiResponse<Stocktake>>(
    "/stocktakes",
    data
  );
  return response.data.data;
}

export async function startStocktake(id: string): Promise<Stocktake> {
  const response = await apiClient.post<ApiResponse<Stocktake>>(
    `/stocktakes/${id}/start`
  );
  return response.data.data;
}

export async function updateStocktakeLine(
  stocktakeId: string,
  lineId: string,
  data: UpdateStocktakeLineDto
): Promise<Stocktake> {
  const response = await apiClient.put<ApiResponse<Stocktake>>(
    `/stocktakes/${stocktakeId}/lines/${lineId}`,
    data
  );
  return response.data.data;
}

export async function completeStocktake(id: string): Promise<Stocktake> {
  const response = await apiClient.post<ApiResponse<Stocktake>>(
    `/stocktakes/${id}/complete`
  );
  return response.data.data;
}

export async function cancelStocktake(id: string): Promise<Stocktake> {
  const response = await apiClient.post<ApiResponse<Stocktake>>(
    `/stocktakes/${id}/cancel`
  );
  return response.data.data;
}

export async function deleteStocktake(id: string): Promise<void> {
  await apiClient.delete(`/stocktakes/${id}`);
}
