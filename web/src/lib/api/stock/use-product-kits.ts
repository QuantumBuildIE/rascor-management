import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getProductKits,
  getProductKit,
  createProductKit,
  updateProductKit,
  deleteProductKit,
  addProductKitItem,
  updateProductKitItem,
  deleteProductKitItem,
  type CreateProductKitDto,
  type UpdateProductKitDto,
  type CreateProductKitItemDto,
  type UpdateProductKitItemDto,
  type GetProductKitsParams,
} from "./product-kits";

export const PRODUCT_KITS_KEY = ["product-kits"];

export function useProductKits(params?: GetProductKitsParams) {
  return useQuery({
    queryKey: [...PRODUCT_KITS_KEY, params],
    queryFn: () => getProductKits(params),
  });
}

export function useProductKit(id: string) {
  return useQuery({
    queryKey: [...PRODUCT_KITS_KEY, id],
    queryFn: () => getProductKit(id),
    enabled: !!id,
  });
}

export function useCreateProductKit() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProductKitDto) => createProductKit(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PRODUCT_KITS_KEY });
    },
  });
}

export function useUpdateProductKit() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductKitDto }) =>
      updateProductKit(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: PRODUCT_KITS_KEY });
      queryClient.invalidateQueries({ queryKey: [...PRODUCT_KITS_KEY, id] });
    },
  });
}

export function useDeleteProductKit() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteProductKit(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PRODUCT_KITS_KEY });
    },
  });
}

export function useAddProductKitItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ kitId, data }: { kitId: string; data: CreateProductKitItemDto }) =>
      addProductKitItem(kitId, data),
    onSuccess: (_, { kitId }) => {
      queryClient.invalidateQueries({ queryKey: [...PRODUCT_KITS_KEY, kitId] });
      queryClient.invalidateQueries({ queryKey: PRODUCT_KITS_KEY });
    },
  });
}

export function useUpdateProductKitItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      itemId,
      kitId,
      data,
    }: {
      itemId: string;
      kitId: string;
      data: UpdateProductKitItemDto;
    }) => updateProductKitItem(itemId, data),
    onSuccess: (_, { kitId }) => {
      queryClient.invalidateQueries({ queryKey: [...PRODUCT_KITS_KEY, kitId] });
      queryClient.invalidateQueries({ queryKey: PRODUCT_KITS_KEY });
    },
  });
}

export function useDeleteProductKitItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ itemId }: { itemId: string; kitId: string }) =>
      deleteProductKitItem(itemId),
    onSuccess: (_, { kitId }) => {
      queryClient.invalidateQueries({ queryKey: [...PRODUCT_KITS_KEY, kitId] });
      queryClient.invalidateQueries({ queryKey: PRODUCT_KITS_KEY });
    },
  });
}

export type {
  CreateProductKitDto,
  UpdateProductKitDto,
  CreateProductKitItemDto,
  UpdateProductKitItemDto,
  GetProductKitsParams,
} from "./product-kits";

export type {
  ProductKit,
  ProductKitItem,
  ProductKitListItem,
} from "./product-kits";
