import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getStocktakes,
  getStocktake,
  getStocktakesByLocation,
  createStocktake,
  startStocktake,
  updateStocktakeLine,
  completeStocktake,
  cancelStocktake,
  deleteStocktake,
  type CreateStocktakeDto,
  type UpdateStocktakeLineDto,
} from "./stocktakes";

const STOCKTAKES_KEY = ["stocktakes"];

export function useStocktakes() {
  return useQuery({
    queryKey: STOCKTAKES_KEY,
    queryFn: getStocktakes,
  });
}

export function useStocktake(id: string) {
  return useQuery({
    queryKey: [...STOCKTAKES_KEY, id],
    queryFn: () => getStocktake(id),
    enabled: !!id,
  });
}

export function useStocktakesByLocation(locationId: string) {
  return useQuery({
    queryKey: [...STOCKTAKES_KEY, "by-location", locationId],
    queryFn: () => getStocktakesByLocation(locationId),
    enabled: !!locationId,
  });
}

export function useCreateStocktake() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateStocktakeDto) => createStocktake(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCKTAKES_KEY });
    },
  });
}

export function useStartStocktake() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => startStocktake(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCKTAKES_KEY });
    },
  });
}

export function useUpdateStocktakeLine() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      stocktakeId,
      lineId,
      data,
    }: {
      stocktakeId: string;
      lineId: string;
      data: UpdateStocktakeLineDto;
    }) => updateStocktakeLine(stocktakeId, lineId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: [...STOCKTAKES_KEY, variables.stocktakeId],
      });
    },
  });
}

export function useCompleteStocktake() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => completeStocktake(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCKTAKES_KEY });
    },
  });
}

export function useCancelStocktake() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => cancelStocktake(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCKTAKES_KEY });
    },
  });
}

export function useDeleteStocktake() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteStocktake(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STOCKTAKES_KEY });
    },
  });
}
