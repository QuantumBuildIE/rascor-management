import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getEmployees,
  getAllEmployees,
  getUnlinkedEmployees,
  getEmployee,
  createEmployee,
  updateEmployee,
  deleteEmployee,
  type CreateEmployeeDto,
  type UpdateEmployeeDto,
  type GetEmployeesParams,
} from "./employees";

export const EMPLOYEES_KEY = ["employees"];

export function useEmployees(params?: GetEmployeesParams) {
  return useQuery({
    queryKey: [...EMPLOYEES_KEY, params],
    queryFn: () => getEmployees(params),
  });
}

export function useAllEmployees() {
  return useQuery({
    queryKey: [...EMPLOYEES_KEY, "all"],
    queryFn: getAllEmployees,
  });
}

export function useUnlinkedEmployees() {
  return useQuery({
    queryKey: [...EMPLOYEES_KEY, "unlinked"],
    queryFn: getUnlinkedEmployees,
  });
}

export function useEmployee(id: string) {
  return useQuery({
    queryKey: [...EMPLOYEES_KEY, id],
    queryFn: () => getEmployee(id),
    enabled: !!id,
  });
}

export function useCreateEmployee() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateEmployeeDto) => createEmployee(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: EMPLOYEES_KEY });
    },
  });
}

export function useUpdateEmployee() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateEmployeeDto }) =>
      updateEmployee(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: EMPLOYEES_KEY });
    },
  });
}

export function useDeleteEmployee() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteEmployee(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: EMPLOYEES_KEY });
    },
  });
}

export type { CreateEmployeeDto, UpdateEmployeeDto, GetEmployeesParams } from "./employees";
export type { PaginatedResponse } from "./employees";
