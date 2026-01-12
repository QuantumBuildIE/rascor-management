import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getBayLocations,
  getBayLocation,
  getBayLocationsByLocation,
  createBayLocation,
  updateBayLocation,
  deleteBayLocation,
  type CreateBayLocationDto,
  type UpdateBayLocationDto,
} from "./bay-locations";

export const BAY_LOCATIONS_KEY = ["bay-locations"];

export function useBayLocations(stockLocationId?: string) {
  return useQuery({
    queryKey: stockLocationId ? [...BAY_LOCATIONS_KEY, { stockLocationId }] : BAY_LOCATIONS_KEY,
    queryFn: () => getBayLocations(stockLocationId),
  });
}

export function useBayLocation(id: string) {
  return useQuery({
    queryKey: [...BAY_LOCATIONS_KEY, id],
    queryFn: () => getBayLocation(id),
    enabled: !!id,
  });
}

export function useBayLocationsByLocation(stockLocationId: string) {
  return useQuery({
    queryKey: [...BAY_LOCATIONS_KEY, "by-location", stockLocationId],
    queryFn: () => getBayLocationsByLocation(stockLocationId),
    enabled: !!stockLocationId,
  });
}

export function useCreateBayLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBayLocationDto) => createBayLocation(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: BAY_LOCATIONS_KEY });
    },
  });
}

export function useUpdateBayLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateBayLocationDto }) =>
      updateBayLocation(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: BAY_LOCATIONS_KEY });
    },
  });
}

export function useDeleteBayLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteBayLocation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: BAY_LOCATIONS_KEY });
    },
  });
}

export type { CreateBayLocationDto, UpdateBayLocationDto } from "./bay-locations";
