import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getProducts,
  getAllProducts,
  getProduct,
  createProduct,
  updateProduct,
  deleteProduct,
  type CreateProductDto,
  type UpdateProductDto,
  type GetProductsParams,
} from "./products";

export const PRODUCTS_KEY = ["products"];

export function useProducts(params?: GetProductsParams) {
  return useQuery({
    queryKey: [...PRODUCTS_KEY, params],
    queryFn: () => getProducts(params),
  });
}

export function useAllProducts() {
  return useQuery({
    queryKey: [...PRODUCTS_KEY, "all"],
    queryFn: getAllProducts,
  });
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: [...PRODUCTS_KEY, id],
    queryFn: () => getProduct(id),
    enabled: !!id,
  });
}

export function useCreateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProductDto) => createProduct(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PRODUCTS_KEY });
    },
  });
}

export function useUpdateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductDto }) =>
      updateProduct(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PRODUCTS_KEY });
    },
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PRODUCTS_KEY });
    },
  });
}

export type { CreateProductDto, UpdateProductDto, GetProductsParams } from "./products";
export type { PaginatedResponse } from "./products";
