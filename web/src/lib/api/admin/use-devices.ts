import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getDevices,
  getDevice,
  linkDevice,
  unlinkDevice,
  deactivateDevice,
  reactivateDevice,
  type GetDevicesParams,
  type LinkDeviceRequest,
  type UnlinkDeviceRequest,
} from "./devices";

export const DEVICES_KEY = ["admin-devices"];

export function useDevices(params?: GetDevicesParams) {
  return useQuery({
    queryKey: [...DEVICES_KEY, params],
    queryFn: () => getDevices(params),
  });
}

export function useDevice(id: string) {
  return useQuery({
    queryKey: [...DEVICES_KEY, id],
    queryFn: () => getDevice(id),
    enabled: !!id,
  });
}

export function useLinkDevice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      deviceId,
      data,
    }: {
      deviceId: string;
      data: LinkDeviceRequest;
    }) => linkDevice(deviceId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEVICES_KEY });
    },
  });
}

export function useUnlinkDevice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      deviceId,
      data,
    }: {
      deviceId: string;
      data: UnlinkDeviceRequest;
    }) => unlinkDevice(deviceId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEVICES_KEY });
    },
  });
}

export function useDeactivateDevice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (deviceId: string) => deactivateDevice(deviceId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEVICES_KEY });
    },
  });
}

export function useReactivateDevice() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (deviceId: string) => reactivateDevice(deviceId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DEVICES_KEY });
    },
  });
}

export type {
  GetDevicesParams,
  LinkDeviceRequest,
  UnlinkDeviceRequest,
  PaginatedResponse,
} from "./devices";
export type { AdminDevice, AdminDeviceDetail } from "@/types/admin";
