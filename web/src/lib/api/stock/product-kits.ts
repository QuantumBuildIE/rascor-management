import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";

export interface ProductKitItem {
  id: string;
  productKitId: string;
  productId: string;
  productCode: string;
  productName: string;
  unit: string;
  defaultQuantity: number;
  unitCost: number;
  unitPrice: number;
  lineCost: number;
  linePrice: number;
  sortOrder: number;
  notes?: string;
}

export interface ProductKit {
  id: string;
  kitCode: string;
  kitName: string;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  isActive: boolean;
  notes?: string;
  totalCost: number;
  totalPrice: number;
  itemCount: number;
  items: ProductKitItem[];
  createdAt: string;
  updatedAt?: string;
}

export interface ProductKitListItem {
  id: string;
  kitCode: string;
  kitName: string;
  categoryName?: string;
  isActive: boolean;
  totalCost: number;
  totalPrice: number;
  itemCount: number;
}

export interface CreateProductKitDto {
  kitCode: string;
  kitName: string;
  description?: string;
  categoryId?: string;
  isActive: boolean;
  notes?: string;
  items?: CreateProductKitItemDto[];
}

export interface UpdateProductKitDto {
  kitCode: string;
  kitName: string;
  description?: string;
  categoryId?: string;
  isActive: boolean;
  notes?: string;
}

export interface CreateProductKitItemDto {
  productId: string;
  defaultQuantity: number;
  sortOrder: number;
  notes?: string;
}

export interface UpdateProductKitItemDto {
  productId: string;
  defaultQuantity: number;
  sortOrder: number;
  notes?: string;
}

export interface GetProductKitsParams {
  search?: string;
  categoryId?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: "asc" | "desc";
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export async function getProductKits(
  params?: GetProductKitsParams
): Promise<PaginatedResponse<ProductKitListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.search) {
    queryParams.append("search", params.search);
  }
  if (params?.categoryId) {
    queryParams.append("categoryId", params.categoryId);
  }
  if (params?.isActive !== undefined) {
    queryParams.append("isActive", String(params.isActive));
  }
  if (params?.pageNumber) {
    queryParams.append("pageNumber", String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append("pageSize", String(params.pageSize));
  }
  if (params?.sortColumn) {
    queryParams.append("sortColumn", params.sortColumn);
  }
  if (params?.sortDirection) {
    queryParams.append("sortDirection", params.sortDirection);
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/stockmanagement/productkits?${queryString}` : "/stockmanagement/productkits";

  const response = await apiClient.get<ApiResponse<PaginatedResponse<ProductKitListItem>>>(url);

  const data = response.data.data;
  if (!data) {
    return {
      items: [],
      pageNumber: params?.pageNumber || 1,
      pageSize: params?.pageSize || 20,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  return data;
}

export async function getProductKit(id: string): Promise<ProductKit> {
  const response = await apiClient.get<ApiResponse<ProductKit>>(
    `/stockmanagement/productkits/${id}`
  );
  return response.data.data;
}

export async function createProductKit(
  data: CreateProductKitDto
): Promise<ProductKit> {
  const response = await apiClient.post<ApiResponse<ProductKit>>(
    "/stockmanagement/productkits",
    data
  );
  return response.data.data;
}

export async function updateProductKit(
  id: string,
  data: UpdateProductKitDto
): Promise<ProductKit> {
  const response = await apiClient.put<ApiResponse<ProductKit>>(
    `/stockmanagement/productkits/${id}`,
    data
  );
  return response.data.data;
}

export async function deleteProductKit(id: string): Promise<void> {
  await apiClient.delete(`/stockmanagement/productkits/${id}`);
}

export async function addProductKitItem(
  kitId: string,
  data: CreateProductKitItemDto
): Promise<ProductKitItem> {
  const response = await apiClient.post<ApiResponse<ProductKitItem>>(
    `/stockmanagement/productkits/${kitId}/items`,
    data
  );
  return response.data.data;
}

export async function updateProductKitItem(
  itemId: string,
  data: UpdateProductKitItemDto
): Promise<ProductKitItem> {
  const response = await apiClient.put<ApiResponse<ProductKitItem>>(
    `/stockmanagement/productkits/items/${itemId}`,
    data
  );
  return response.data.data;
}

export async function deleteProductKitItem(itemId: string): Promise<void> {
  await apiClient.delete(`/stockmanagement/productkits/items/${itemId}`);
}
