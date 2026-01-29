import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getFloatStatus,
  triggerFloatSync,
  triggerSpaCheck,
  getUnmatchedSummary,
  getUnmatchedItems,
  linkFloatPerson,
  linkFloatProject,
  ignoreUnmatchedItem,
  getAvailableEmployees,
  getAvailableSites,
  getLinkingSummary,
  getLinkedEmployees,
  getLinkedSites,
  unlinkEmployee,
  unlinkSite,
  type GetUnmatchedItemsParams,
} from "./float";

export const FLOAT_KEY = ["float"];
export const FLOAT_STATUS_KEY = [...FLOAT_KEY, "status"];
export const FLOAT_UNMATCHED_KEY = [...FLOAT_KEY, "unmatched"];
export const FLOAT_UNMATCHED_SUMMARY_KEY = [...FLOAT_UNMATCHED_KEY, "summary"];
export const FLOAT_AVAILABLE_EMPLOYEES_KEY = [
  ...FLOAT_KEY,
  "available-employees",
];
export const FLOAT_AVAILABLE_SITES_KEY = [...FLOAT_KEY, "available-sites"];
export const FLOAT_LINKING_SUMMARY_KEY = [...FLOAT_KEY, "linking-summary"];
export const FLOAT_LINKED_EMPLOYEES_KEY = [...FLOAT_KEY, "linked-employees"];
export const FLOAT_LINKED_SITES_KEY = [...FLOAT_KEY, "linked-sites"];

export function useFloatStatus() {
  return useQuery({
    queryKey: FLOAT_STATUS_KEY,
    queryFn: getFloatStatus,
  });
}

export function useFloatSync() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: triggerFloatSync,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FLOAT_UNMATCHED_KEY });
      queryClient.invalidateQueries({ queryKey: FLOAT_STATUS_KEY });
    },
  });
}

export function useSpaCheck() {
  return useMutation({
    mutationFn: (date?: string) => triggerSpaCheck(date),
  });
}

export function useUnmatchedSummary() {
  return useQuery({
    queryKey: FLOAT_UNMATCHED_SUMMARY_KEY,
    queryFn: getUnmatchedSummary,
  });
}

export function useUnmatchedItems(params?: GetUnmatchedItemsParams) {
  return useQuery({
    queryKey: [...FLOAT_UNMATCHED_KEY, params],
    queryFn: () => getUnmatchedItems(params),
  });
}

export function useLinkFloatPerson() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, employeeId }: { id: string; employeeId: string }) =>
      linkFloatPerson(id, employeeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FLOAT_UNMATCHED_KEY });
      queryClient.invalidateQueries({ queryKey: FLOAT_AVAILABLE_EMPLOYEES_KEY });
    },
  });
}

export function useLinkFloatProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, siteId }: { id: string; siteId: string }) =>
      linkFloatProject(id, siteId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FLOAT_UNMATCHED_KEY });
      queryClient.invalidateQueries({ queryKey: FLOAT_AVAILABLE_SITES_KEY });
    },
  });
}

export function useIgnoreUnmatchedItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => ignoreUnmatchedItem(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FLOAT_UNMATCHED_KEY });
    },
  });
}

export function useAvailableEmployees(search?: string) {
  return useQuery({
    queryKey: [...FLOAT_AVAILABLE_EMPLOYEES_KEY, search],
    queryFn: () => getAvailableEmployees(search),
  });
}

export function useAvailableSites(search?: string) {
  return useQuery({
    queryKey: [...FLOAT_AVAILABLE_SITES_KEY, search],
    queryFn: () => getAvailableSites(search),
  });
}

export function useLinkingSummary() {
  return useQuery({
    queryKey: FLOAT_LINKING_SUMMARY_KEY,
    queryFn: getLinkingSummary,
  });
}

export function useLinkedEmployees() {
  return useQuery({
    queryKey: FLOAT_LINKED_EMPLOYEES_KEY,
    queryFn: getLinkedEmployees,
  });
}

export function useLinkedSites() {
  return useQuery({
    queryKey: FLOAT_LINKED_SITES_KEY,
    queryFn: getLinkedSites,
  });
}

export function useUnlinkEmployee() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (employeeId: string) => unlinkEmployee(employeeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FLOAT_KEY });
    },
  });
}

export function useUnlinkSite() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (siteId: string) => unlinkSite(siteId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FLOAT_KEY });
    },
  });
}

// Re-export types
export type {
  FloatUnmatchedItem,
  FloatUnmatchedSummary,
  FloatUnmatchedItemsResponse,
  AvailableEmployee,
  AvailableSite,
  FloatSyncResult,
  SpaCheckResult,
  FloatStatusResponse,
  GetUnmatchedItemsParams,
  FloatLinkedEmployee,
  FloatLinkedSite,
  FloatLinkingSummary,
} from "./float";
