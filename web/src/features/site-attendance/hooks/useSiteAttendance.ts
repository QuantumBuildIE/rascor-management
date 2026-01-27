import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getDashboardKpis,
  getEmployeePerformance,
  getEvents,
  getSummaries,
  getSettings,
  updateSettings,
  getBankHolidays,
  createBankHoliday,
  deleteBankHoliday,
  getSpaRecords,
  getSpaRecord,
  createSpa,
  updateSpa,
  uploadSpaImage,
  uploadSpaSignature,
  type GetEventsParams,
  type GetSummariesParams,
  type GetKpisParams,
  type GetPerformanceParams,
  type AttendanceSettings,
  type GetSpaParams,
  type CreateSpaRequest,
  type UpdateSpaRequest,
} from '../api/siteAttendanceApi';

// Query keys
export const ATTENDANCE_KEYS = {
  all: ['site-attendance'] as const,
  kpis: (params?: GetKpisParams) => [...ATTENDANCE_KEYS.all, 'kpis', params] as const,
  performance: (params?: GetPerformanceParams) => [...ATTENDANCE_KEYS.all, 'performance', params] as const,
  events: (params?: GetEventsParams) => [...ATTENDANCE_KEYS.all, 'events', params] as const,
  summaries: (params?: GetSummariesParams) => [...ATTENDANCE_KEYS.all, 'summaries', params] as const,
  settings: () => [...ATTENDANCE_KEYS.all, 'settings'] as const,
  bankHolidays: () => [...ATTENDANCE_KEYS.all, 'bank-holidays'] as const,
  spa: (params?: GetSpaParams) => [...ATTENDANCE_KEYS.all, 'spa', params] as const,
  spaDetail: (id: string) => [...ATTENDANCE_KEYS.all, 'spa', id] as const,
};

// Dashboard hooks
export function useAttendanceKpis(params?: GetKpisParams) {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.kpis(params),
    queryFn: () => getDashboardKpis(params),
  });
}

export function useEmployeePerformance(params?: GetPerformanceParams) {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.performance(params),
    queryFn: () => getEmployeePerformance(params),
  });
}

// Events hook
export function useAttendanceEvents(params?: GetEventsParams) {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.events(params),
    queryFn: () => getEvents(params),
  });
}

// Summaries hook
export function useAttendanceSummaries(params?: GetSummariesParams) {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.summaries(params),
    queryFn: () => getSummaries(params),
  });
}

// Settings hooks
export function useAttendanceSettings() {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.settings(),
    queryFn: getSettings,
  });
}

export function useUpdateAttendanceSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: Partial<AttendanceSettings>) => updateSettings(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.settings() });
    },
  });
}

// Bank Holidays hooks
export function useBankHolidays() {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.bankHolidays(),
    queryFn: getBankHolidays,
  });
}

export function useCreateBankHoliday() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { date: string; name?: string }) => createBankHoliday(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.bankHolidays() });
    },
  });
}

export function useDeleteBankHoliday() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteBankHoliday(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.bankHolidays() });
    },
  });
}

// SPA hooks
export function useSpaRecords(params?: GetSpaParams) {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.spa(params),
    queryFn: () => getSpaRecords(params),
  });
}

export function useSpaRecord(id: string) {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.spaDetail(id),
    queryFn: () => getSpaRecord(id),
    enabled: !!id,
  });
}

export function useCreateSpa() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateSpaRequest) => createSpa(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.all });
    },
  });
}

export function useUpdateSpa() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSpaRequest }) => updateSpa(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.all });
    },
  });
}

export function useUploadSpaImage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, file, onProgress }: { id: string; file: File; onProgress?: (progress: number) => void }) =>
      uploadSpaImage(id, file, onProgress),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.all });
    },
  });
}

export function useUploadSpaSignature() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, file, onProgress }: { id: string; file: File; onProgress?: (progress: number) => void }) =>
      uploadSpaSignature(id, file, onProgress),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.all });
    },
  });
}

// Re-export types for convenience
export type {
  AttendanceEvent,
  AttendanceSummary,
  DashboardKpis,
  EmployeePerformance,
  AttendanceSettings,
  BankHoliday,
  PaginatedResponse,
  GetEventsParams,
  GetSummariesParams,
  GetKpisParams,
  GetPerformanceParams,
  SitePhotoAttendance,
  CreateSpaRequest,
  UpdateSpaRequest,
  GetSpaParams,
} from '../api/siteAttendanceApi';
