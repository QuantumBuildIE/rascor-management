import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getUsers,
  getAllUsers,
  getUser,
  createUser,
  updateUser,
  deleteUser,
  resetPassword,
  changePassword,
  type CreateUserDto,
  type UpdateUserDto,
  type ResetPasswordDto,
  type ChangePasswordDto,
  type GetUsersParams,
} from "./users";

export const USERS_KEY = ["users"];

export function useUsers(params?: GetUsersParams) {
  return useQuery({
    queryKey: [...USERS_KEY, params],
    queryFn: () => getUsers(params),
  });
}

export function useAllUsers() {
  return useQuery({
    queryKey: [...USERS_KEY, "all"],
    queryFn: getAllUsers,
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: [...USERS_KEY, id],
    queryFn: () => getUser(id),
    enabled: !!id,
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateUserDto) => createUser(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: USERS_KEY });
      queryClient.invalidateQueries({ queryKey: ["employees"] });
    },
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUserDto }) =>
      updateUser(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: USERS_KEY });
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: USERS_KEY });
    },
  });
}

export function useResetPassword() {
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ResetPasswordDto }) =>
      resetPassword(id, data),
  });
}

export function useChangePassword() {
  return useMutation({
    mutationFn: (data: ChangePasswordDto) => changePassword(data),
  });
}

export type { CreateUserDto, UpdateUserDto, ResetPasswordDto, ChangePasswordDto, GetUsersParams } from "./users";
export type { PaginatedResponse, User, UserRole } from "./users";
