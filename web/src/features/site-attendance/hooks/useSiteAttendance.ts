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
  type GetEventsParams,
  type GetSummariesParams,
  type GetKpisParams,
  type GetPerformanceParams,
  type AttendanceSettings,
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
} from '../api/siteAttendanceApi';
