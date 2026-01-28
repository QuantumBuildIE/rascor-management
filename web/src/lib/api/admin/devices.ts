import { apiClient } from "@/lib/api/client";
import type { AdminDevice, AdminDeviceDetail } from "@/types/admin";

export interface GetDevicesParams {
  page?: number;
  pageSize?: number;
  isLinked?: boolean | null;
  isActive?: boolean | null;
  search?: string;
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

export interface LinkDeviceRequest {
  employeeId: string;
}

export interface UnlinkDeviceRequest {
  reason: string;
}

export async function getDevices(
  params?: GetDevicesParams
): Promise<PaginatedResponse<AdminDevice>> {
  const queryParams = new URLSearchParams();

  if (params?.page) {
    queryParams.append("page", String(params.page));
  }
  if (params?.pageSize) {
    queryParams.append("pageSize", String(params.pageSize));
  }
  if (params?.isLinked !== null && params?.isLinked !== undefined) {
    queryParams.append("isLinked", String(params.isLinked));
  }
  if (params?.isActive !== null && params?.isActive !== undefined) {
    queryParams.append("isActive", String(params.isActive));
  }
  if (params?.search) {
    queryParams.append("search", params.search);
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/admin/devices?${queryString}` : "/admin/devices";

  const response = await apiClient.get<PaginatedResponse<AdminDevice>>(url);

  // The backend returns data directly, not wrapped in ApiResponse
  const data = response.data;
  if (!data) {
    return {
      items: [],
      pageNumber: params?.page || 1,
      pageSize: params?.pageSize || 20,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  return data;
}

export async function getDevice(id: string): Promise<AdminDeviceDetail> {
  const response = await apiClient.get<AdminDeviceDetail>(`/admin/devices/${id}`);
  return response.data;
}

export async function linkDevice(
  deviceId: string,
  data: LinkDeviceRequest
): Promise<void> {
  await apiClient.post(`/admin/devices/${deviceId}/link`, data);
}

export async function unlinkDevice(
  deviceId: string,
  data: UnlinkDeviceRequest
): Promise<void> {
  await apiClient.post(`/admin/devices/${deviceId}/unlink`, data);
}

export async function deactivateDevice(deviceId: string): Promise<void> {
  await apiClient.post(`/admin/devices/${deviceId}/deactivate`);
}

export async function reactivateDevice(deviceId: string): Promise<void> {
  await apiClient.post(`/admin/devices/${deviceId}/reactivate`);
}
