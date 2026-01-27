import { apiClient } from '@/lib/api/client';

// Types
export interface AttendanceEvent {
  id: string;
  employeeId: string;
  employeeName: string;
  siteId: string;
  siteName: string;
  eventType: 'Enter' | 'Exit';
  timestamp: string;
  latitude?: number;
  longitude?: number;
  triggerMethod: 'Automatic' | 'Manual';
  isNoise: boolean;
  noiseDistance?: number;
}

export interface AttendanceSummary {
  id: string;
  employeeId: string;
  employeeName: string;
  siteId: string;
  siteName: string;
  date: string;
  firstEntry?: string;
  lastExit?: string;
  timeOnSiteHours: number;
  expectedHours: number;
  utilizationPercent: number;
  varianceHours: number;
  status: 'Excellent' | 'Good' | 'BelowTarget' | 'Absent' | 'Incomplete';
  entryCount: number;
  exitCount: number;
  hasSpa: boolean;
}

export interface DashboardKpis {
  overallUtilization: number;
  averageHoursPerDay: number;
  totalActiveEmployees: number;
  totalActiveSites: number;
  excellentCount: number;
  goodCount: number;
  belowTargetCount: number;
  absentCount: number;
  expectedHours: number;
  actualHours: number;
  varianceHours: number;
  workingDays: number;
  fromDate: string;
  toDate: string;
}

export interface EmployeePerformance {
  employeeId: string;
  employeeName: string;
  totalHours: number;
  expectedHours: number;
  utilizationPercent: number;
  varianceHours: number;
  status: string;
  daysPresent: number;
  daysAbsent: number;
  spaCount: number;
}

export interface AttendanceSettings {
  id: string;
  expectedHoursPerDay: number;
  workStartTime: string;
  lateThresholdMinutes: number;
  includeSaturday: boolean;
  includeSunday: boolean;
  geofenceRadiusMeters: number;
  noiseThresholdMeters: number;
  spaGracePeriodMinutes: number;
  enablePushNotifications: boolean;
  enableEmailNotifications: boolean;
  enableSmsNotifications: boolean;
  notificationTitle: string;
  notificationMessage: string;
}

export interface BankHoliday {
  id: string;
  date: string;
  name?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface GetEventsParams {
  employeeId?: string;
  siteId?: string;
  fromDate?: string;
  toDate?: string;
  eventType?: string;
  includeNoise?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface GetSummariesParams {
  employeeId?: string;
  siteId?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface GetKpisParams {
  fromDate?: string;
  toDate?: string;
  siteId?: string;
}

export interface GetPerformanceParams {
  fromDate?: string;
  toDate?: string;
}

// Helper to build query string
function buildQueryString(params: object): string {
  const queryParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      queryParams.append(key, String(value));
    }
  });

  const queryString = queryParams.toString();
  return queryString ? `?${queryString}` : '';
}

// Dashboard API
export async function getDashboardKpis(params?: GetKpisParams): Promise<DashboardKpis> {
  const queryString = buildQueryString(params || {});
  const response = await apiClient.get<DashboardKpis>(
    `/site-attendance/dashboard/kpis${queryString}`
  );
  return response.data;
}

export async function getEmployeePerformance(params?: GetPerformanceParams): Promise<EmployeePerformance[]> {
  const queryString = buildQueryString(params || {});
  const response = await apiClient.get<{ items: EmployeePerformance[] }>(
    `/site-attendance/dashboard/employee-performance${queryString}`
  );
  // Backend returns PaginatedList which has items array
  return response.data.items ?? [];
}

// Events API
export async function getEvents(params?: GetEventsParams): Promise<PaginatedResponse<AttendanceEvent>> {
  const queryString = buildQueryString(params || {});
  const response = await apiClient.get<PaginatedResponse<AttendanceEvent>>(
    `/site-attendance/events${queryString}`
  );

  const data = response.data;
  if (!data) {
    return {
      items: [],
      pageNumber: params?.pageNumber || 1,
      pageSize: params?.pageSize || 20,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  return data;
}

// Summaries API
export async function getSummaries(params?: GetSummariesParams): Promise<PaginatedResponse<AttendanceSummary>> {
  const queryString = buildQueryString(params || {});
  const response = await apiClient.get<PaginatedResponse<AttendanceSummary>>(
    `/site-attendance/summaries${queryString}`
  );

  const data = response.data;
  if (!data) {
    return {
      items: [],
      pageNumber: params?.pageNumber || 1,
      pageSize: params?.pageSize || 20,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  return data;
}

// Settings API
export async function getSettings(): Promise<AttendanceSettings> {
  const response = await apiClient.get<AttendanceSettings>('/site-attendance/settings');
  return response.data;
}

export async function updateSettings(data: Partial<AttendanceSettings>): Promise<AttendanceSettings> {
  const response = await apiClient.put<AttendanceSettings>('/site-attendance/settings', data);
  return response.data;
}

// Bank Holidays API
export async function getBankHolidays(): Promise<BankHoliday[]> {
  const response = await apiClient.get<BankHoliday[]>('/site-attendance/bank-holidays');
  return response.data ?? [];
}

export async function createBankHoliday(data: { date: string; name?: string }): Promise<BankHoliday> {
  const response = await apiClient.post<BankHoliday>('/site-attendance/bank-holidays', data);
  return response.data;
}

export async function deleteBankHoliday(id: string): Promise<void> {
  await apiClient.delete(`/site-attendance/bank-holidays/${id}`);
}

// Site Photo Attendance (SPA) Types
export interface SitePhotoAttendance {
  id: string;
  employeeId: string;
  employeeName: string;
  siteId: string;
  siteName: string;
  eventDate: string;
  imageUrl?: string;
  signatureUrl?: string;
  latitude?: number;
  longitude?: number;
  distanceToSite?: number;
  weatherConditions?: string;
  notes?: string;
  createdAt: string;
}

export interface CreateSpaRequest {
  employeeId: string;
  siteId: string;
  eventDate: string;
  latitude?: number;
  longitude?: number;
  weatherConditions?: string;
  notes?: string;
}

export interface UpdateSpaRequest {
  weatherConditions?: string;
  notes?: string;
}

export interface GetSpaParams {
  employeeId?: string;
  siteId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

// SPA API Functions
export async function createSpa(data: CreateSpaRequest): Promise<SitePhotoAttendance> {
  const response = await apiClient.post<SitePhotoAttendance>('/site-attendance/spa', data);
  return response.data;
}

export async function getSpaRecords(params?: GetSpaParams): Promise<PaginatedResponse<SitePhotoAttendance>> {
  const queryString = buildQueryString(params || {});
  const response = await apiClient.get<PaginatedResponse<SitePhotoAttendance>>(
    `/site-attendance/spa${queryString}`
  );

  const data = response.data;
  if (!data) {
    return {
      items: [],
      pageNumber: params?.page || 1,
      pageSize: params?.pageSize || 20,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  return data;
}

export async function getSpaRecord(id: string): Promise<SitePhotoAttendance> {
  const response = await apiClient.get<SitePhotoAttendance>(`/site-attendance/spa/${id}`);
  return response.data;
}

export async function updateSpa(id: string, data: UpdateSpaRequest): Promise<SitePhotoAttendance> {
  const response = await apiClient.put<SitePhotoAttendance>(`/site-attendance/spa/${id}`, data);
  return response.data;
}

export async function uploadSpaImage(
  id: string,
  file: File,
  onProgress?: (progress: number) => void
): Promise<SitePhotoAttendance> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await apiClient.post<SitePhotoAttendance>(
    `/site-attendance/spa/${id}/image`,
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          onProgress(percentCompleted);
        }
      },
    }
  );

  return response.data;
}

export async function uploadSpaSignature(
  id: string,
  file: File,
  onProgress?: (progress: number) => void
): Promise<SitePhotoAttendance> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await apiClient.post<SitePhotoAttendance>(
    `/site-attendance/spa/${id}/signature`,
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          onProgress(percentCompleted);
        }
      },
    }
  );

  return response.data;
}
