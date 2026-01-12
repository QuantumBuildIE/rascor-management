import { apiClient } from "@/lib/api/client";
import type { ApiResponse } from "@/types/auth";
import type { Supplier } from "@/types/stock";

export interface CreateSupplierDto {
  supplierCode: string;
  supplierName: string;
  contactName?: string;
  email?: string;
  phone?: string;
  address?: string;
  paymentTerms?: string;
  isActive?: boolean;
}

export interface UpdateSupplierDto {
  supplierCode?: string;
  supplierName?: string;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  paymentTerms?: string | null;
  isActive?: boolean;
}

export async function getSuppliers(): Promise<Supplier[]> {
  const response = await apiClient.get<ApiResponse<Supplier[]>>("/suppliers");
  return response.data.data ?? [];
}

export async function getSupplier(id: string): Promise<Supplier> {
  const response = await apiClient.get<ApiResponse<Supplier>>(
    `/suppliers/${id}`
  );
  return response.data.data;
}

export async function createSupplier(
  data: CreateSupplierDto
): Promise<Supplier> {
  const response = await apiClient.post<ApiResponse<Supplier>>(
    "/suppliers",
    data
  );
  return response.data.data;
}

export async function updateSupplier(
  id: string,
  data: UpdateSupplierDto
): Promise<Supplier> {
  const response = await apiClient.put<ApiResponse<Supplier>>(
    `/suppliers/${id}`,
    data
  );
  return response.data.data;
}

export async function deleteSupplier(id: string): Promise<void> {
  await apiClient.delete(`/suppliers/${id}`);
}
