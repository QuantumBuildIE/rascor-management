import { useQuery } from "@tanstack/react-query";
import {
  getStockLevels,
  getStockLevel,
  getStockLevelsByLocation,
  getLowStockLevels,
  getStockLevelByProductAndLocation,
  type GetStockLevelsParams,
} from "./stock-levels";

export const STOCK_LEVELS_KEY = ["stock-levels"];

export function useStockLevels(params?: GetStockLevelsParams) {
  return useQuery({
    queryKey: [...STOCK_LEVELS_KEY, params],
    queryFn: () => getStockLevels(params),
  });
}

export function useStockLevel(id: string) {
  return useQuery({
    queryKey: [...STOCK_LEVELS_KEY, id],
    queryFn: () => getStockLevel(id),
    enabled: !!id,
  });
}

export function useStockLevelsByLocation(locationId: string) {
  return useQuery({
    queryKey: [...STOCK_LEVELS_KEY, "by-location", locationId],
    queryFn: () => getStockLevelsByLocation(locationId),
    enabled: !!locationId,
  });
}

export function useLowStockLevels() {
  return useQuery({
    queryKey: [...STOCK_LEVELS_KEY, "low-stock"],
    queryFn: getLowStockLevels,
  });
}

export function useStockLevelByProductAndLocation(
  productId: string,
  locationId: string
) {
  return useQuery({
    queryKey: [
      ...STOCK_LEVELS_KEY,
      "by-product",
      productId,
      "location",
      locationId,
    ],
    queryFn: () => getStockLevelByProductAndLocation(productId, locationId),
    enabled: !!productId && !!locationId,
  });
}

export type { GetStockLevelsParams } from "./stock-levels";
