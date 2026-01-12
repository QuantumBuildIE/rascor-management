import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { Product } from "@/types/stock";

export interface CreateProductDto {
  productCode: string;
  productName: string;
  categoryId: string;
  supplierId?: string;
  unitType: string;
  baseRate: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  leadTimeDays?: number;
  isActive?: boolean;
  costPrice?: number | null;
  sellPrice?: number | null;
  productType?: string | null;
}

export interface UpdateProductDto {
  productCode?: string;
  productName?: string;
  categoryId?: string;
  supplierId?: string | null;
  unitType?: string;
  baseRate?: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  leadTimeDays?: number;
  isActive?: boolean;
  costPrice?: number | null;
  sellPrice?: number | null;
  productType?: string | null;
}

export interface GetProductsParams {
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: "asc" | "desc";
  search?: string;
  isActive?: boolean;
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

export async function getProducts(
  params?: GetProductsParams
): Promise<PaginatedResponse<Product>> {
  const queryParams = new URLSearchParams();

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
  if (params?.search) {
    queryParams.append("search", params.search);
  }
  if (params?.isActive !== undefined) {
    queryParams.append("isActive", String(params.isActive));
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/products?${queryString}` : "/products";

  const response = await apiClient.get<ApiResponse<PaginatedResponse<Product>>>(url);

  // Handle cases where data might be null or have unexpected structure
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

export async function getAllProducts(): Promise<Product[]> {
  const response = await apiClient.get<ApiResponse<Product[]>>("/products/all");
  return response.data.data ?? [];
}

export async function getProduct(id: string): Promise<Product> {
  const response = await apiClient.get<ApiResponse<Product>>(`/products/${id}`);
  return response.data.data;
}

export async function createProduct(data: CreateProductDto): Promise<Product> {
  const response = await apiClient.post<ApiResponse<Product>>("/products", data);
  return response.data.data;
}

export async function updateProduct(
  id: string,
  data: UpdateProductDto
): Promise<Product> {
  const response = await apiClient.put<ApiResponse<Product>>(
    `/products/${id}`,
    data
  );
  return response.data.data;
}

export async function deleteProduct(id: string): Promise<void> {
  await apiClient.delete(`/products/${id}`);
}

export async function uploadProductImage(
  id: string,
  file: File
): Promise<Product> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await apiClient.post<ApiResponse<Product>>(
    `/products/${id}/image`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );
  return response.data.data;
}

export async function deleteProductImage(id: string): Promise<Product> {
  const response = await apiClient.delete<ApiResponse<Product>>(
    `/products/${id}/image`
  );
  return response.data.data;
}
