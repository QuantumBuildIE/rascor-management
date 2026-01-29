import { apiClient } from "@/lib/api/client";

// Types
export interface FloatUnmatchedItem {
  id: string;
  itemType: "Person" | "Project";
  floatId: number;
  floatName: string;
  floatEmail?: string;
  suggestedMatchId?: string;
  suggestedMatchName?: string;
  matchConfidence?: number;
  status: string;
  createdAt: string;
}

export interface FloatUnmatchedSummary {
  pendingPeople: number;
  pendingProjects: number;
  totalPending: number;
}

export interface FloatUnmatchedItemsResponse {
  items: FloatUnmatchedItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AvailableEmployee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
}

export interface AvailableSite {
  id: string;
  name: string;
  address?: string;
}

export interface FloatSyncResult {
  peopleMatched: number;
  peopleUnmatched: number;
  projectsMatched: number;
  projectsUnmatched: number;
  newUnmatchedItems: number;
  errors: string[];
}

export interface SpaCheckResult {
  checkDate: string;
  totalScheduledTasks: number;
  employeesChecked: number;
  spaSubmitted: number;
  spaMissing: number;
  remindersSent: number;
  remindersFailed: number;
  skippedUnmatchedPeople: number;
  skippedUnmatchedProjects: number;
  skippedNoEmail: number;
  skippedAlreadyNotified: number;
  errors: string[];
  duration: string;
}

export interface FloatStatusResponse {
  isEnabled: boolean;
  isConfigured: boolean;
  spaCheckCronExpression: string;
  spaCheckGracePeriodMinutes: number;
  connectionTest?: {
    success: boolean;
    peopleCount: number;
    projectsCount: number;
    error?: string;
    testedAt: string;
  };
}

export interface GetUnmatchedItemsParams {
  itemType?: "Person" | "Project";
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface FloatLinkedEmployee {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  employeeEmail?: string;
  floatPersonId: number;
  floatPersonName?: string;
  floatLinkMethod?: string;
  floatLinkedAt?: string;
}

export interface FloatLinkedSite {
  siteId: string;
  siteName: string;
  siteAddress?: string;
  floatProjectId: number;
  floatProjectName?: string;
  floatLinkMethod?: string;
  floatLinkedAt?: string;
}

export interface FloatLinkingSummary {
  linkedPeople: number;
  linkedProjects: number;
  unmatchedPeople: number;
  unmatchedProjects: number;
  floatPeopleTotal: number;
  floatProjectsTotal: number;
}

// API Functions

export async function getFloatStatus(): Promise<FloatStatusResponse> {
  const response = await apiClient.get<FloatStatusResponse>(
    "/admin/float/status"
  );
  return response.data;
}

export async function triggerFloatSync(): Promise<FloatSyncResult> {
  const response = await apiClient.post<FloatSyncResult>("/admin/float/sync");
  return response.data;
}

export async function triggerSpaCheck(date?: string): Promise<SpaCheckResult> {
  const url = date
    ? `/admin/float/spa-check?date=${date}`
    : "/admin/float/spa-check";
  const response = await apiClient.post<SpaCheckResult>(url);
  return response.data;
}

export async function getUnmatchedSummary(): Promise<FloatUnmatchedSummary> {
  const response = await apiClient.get<FloatUnmatchedSummary>(
    "/admin/float/unmatched/summary"
  );
  return response.data;
}

export async function getUnmatchedItems(
  params?: GetUnmatchedItemsParams
): Promise<FloatUnmatchedItemsResponse> {
  const queryParams = new URLSearchParams();

  if (params?.itemType) {
    queryParams.append("itemType", params.itemType);
  }
  if (params?.status) {
    queryParams.append("status", params.status);
  }
  if (params?.page) {
    queryParams.append("page", String(params.page));
  }
  if (params?.pageSize) {
    queryParams.append("pageSize", String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString
    ? `/admin/float/unmatched?${queryString}`
    : "/admin/float/unmatched";

  const response = await apiClient.get<FloatUnmatchedItemsResponse>(url);
  return response.data;
}

export async function linkFloatPerson(
  id: string,
  employeeId: string
): Promise<void> {
  await apiClient.post(`/admin/float/unmatched/${id}/link-person`, {
    targetId: employeeId,
  });
}

export async function linkFloatProject(
  id: string,
  siteId: string
): Promise<void> {
  await apiClient.post(`/admin/float/unmatched/${id}/link-project`, {
    targetId: siteId,
  });
}

export async function ignoreUnmatchedItem(id: string): Promise<void> {
  await apiClient.post(`/admin/float/unmatched/${id}/ignore`);
}

export async function getAvailableEmployees(
  search?: string
): Promise<AvailableEmployee[]> {
  const url = search
    ? `/admin/float/available-employees?search=${encodeURIComponent(search)}`
    : "/admin/float/available-employees";

  const response = await apiClient.get<AvailableEmployee[]>(url);
  return response.data;
}

export async function getAvailableSites(
  search?: string
): Promise<AvailableSite[]> {
  const url = search
    ? `/admin/float/available-sites?search=${encodeURIComponent(search)}`
    : "/admin/float/available-sites";

  const response = await apiClient.get<AvailableSite[]>(url);
  return response.data;
}

export async function getLinkingSummary(): Promise<FloatLinkingSummary> {
  const response = await apiClient.get<FloatLinkingSummary>(
    "/admin/float/summary"
  );
  return response.data;
}

export async function getLinkedEmployees(): Promise<FloatLinkedEmployee[]> {
  const response = await apiClient.get<FloatLinkedEmployee[]>(
    "/admin/float/linked/employees"
  );
  return response.data;
}

export async function getLinkedSites(): Promise<FloatLinkedSite[]> {
  const response = await apiClient.get<FloatLinkedSite[]>(
    "/admin/float/linked/sites"
  );
  return response.data;
}

export async function unlinkEmployee(employeeId: string): Promise<void> {
  await apiClient.post(`/admin/float/unlink/employee/${employeeId}`);
}

export async function unlinkSite(siteId: string): Promise<void> {
  await apiClient.post(`/admin/float/unlink/site/${siteId}`);
}
