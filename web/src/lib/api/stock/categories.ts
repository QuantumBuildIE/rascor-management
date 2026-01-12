import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { Category } from "@/types/stock";

export interface CreateCategoryDto {
  categoryName: string;
  sortOrder?: number;
  isActive?: boolean;
}

export interface UpdateCategoryDto {
  categoryName?: string;
  sortOrder?: number;
  isActive?: boolean;
}

export async function getCategories(): Promise<Category[]> {
  const response = await apiClient.get<ApiResponse<Category[]>>("/categories");
  return response.data.data ?? [];
}

export async function getCategory(id: string): Promise<Category> {
  const response = await apiClient.get<ApiResponse<Category>>(
    `/categories/${id}`
  );
  return response.data.data;
}

export async function createCategory(
  data: CreateCategoryDto
): Promise<Category> {
  const response = await apiClient.post<ApiResponse<Category>>(
    "/categories",
    data
  );
  return response.data.data;
}

export async function updateCategory(
  id: string,
  data: UpdateCategoryDto
): Promise<Category> {
  const response = await apiClient.put<ApiResponse<Category>>(
    `/categories/${id}`,
    data
  );
  return response.data.data;
}

export async function deleteCategory(id: string): Promise<void> {
  await apiClient.delete(`/categories/${id}`);
}
