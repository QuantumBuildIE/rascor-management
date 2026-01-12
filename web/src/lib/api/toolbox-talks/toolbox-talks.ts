import { apiClient } from '@/lib/api/client';
import type { ApiResponse } from '@/types/auth';
import type {
  ToolboxTalk,
  ToolboxTalkListItem,
  ToolboxTalkDashboard,
  ToolboxTalkSettings,
  CreateToolboxTalkRequest,
  UpdateToolboxTalkRequest,
  UpdateToolboxTalkSettingsRequest,
  GetToolboxTalksParams,
} from '@/types/toolbox-talks';

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ============================================
// Toolbox Talks CRUD
// ============================================

export async function getToolboxTalks(
  params?: GetToolboxTalksParams
): Promise<PaginatedResponse<ToolboxTalkListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.searchTerm) {
    queryParams.append('searchTerm', params.searchTerm);
  }
  if (params?.frequency) {
    queryParams.append('frequency', params.frequency);
  }
  if (params?.isActive !== undefined) {
    queryParams.append('isActive', String(params.isActive));
  }
  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/toolbox-talks?${queryString}` : '/toolbox-talks';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<ToolboxTalkListItem>>>(url);

  const data = response.data.data;
  if (!data) {
    return {
      items: [],
      pageNumber: params?.pageNumber || 1,
      pageSize: params?.pageSize || 10,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  return data;
}

export async function getToolboxTalk(id: string): Promise<ToolboxTalk> {
  const response = await apiClient.get<ToolboxTalk>(`/toolbox-talks/${id}`);
  return response.data;
}

export async function createToolboxTalk(data: CreateToolboxTalkRequest): Promise<ToolboxTalk> {
  const response = await apiClient.post<ToolboxTalk>('/toolbox-talks', data);
  return response.data;
}

export async function updateToolboxTalk(
  id: string,
  data: UpdateToolboxTalkRequest
): Promise<ToolboxTalk> {
  const response = await apiClient.put<ToolboxTalk>(`/toolbox-talks/${id}`, {
    ...data,
    id,
  });
  return response.data;
}

export async function deleteToolboxTalk(id: string): Promise<void> {
  await apiClient.delete(`/toolbox-talks/${id}`);
}

// ============================================
// Dashboard
// ============================================

export async function getToolboxTalkDashboard(): Promise<ToolboxTalkDashboard> {
  const response = await apiClient.get<ApiResponse<ToolboxTalkDashboard>>(
    '/toolbox-talks/dashboard'
  );
  return (
    response.data.data ?? {
      totalTalks: 0,
      activeTalks: 0,
      inactiveTalks: 0,
      totalAssignments: 0,
      pendingCount: 0,
      inProgressCount: 0,
      completedCount: 0,
      overdueCount: 0,
      completionRate: 0,
      overdueRate: 0,
      averageCompletionTimeHours: 0,
      averageQuizScore: 0,
      quizPassRate: 0,
      talksByStatus: {},
      talksByFrequency: {},
      recentCompletions: [],
      overdueAssignments: [],
      upcomingSchedules: [],
    }
  );
}

// ============================================
// Settings
// ============================================

export async function getToolboxTalkSettings(): Promise<ToolboxTalkSettings> {
  const response = await apiClient.get<ApiResponse<ToolboxTalkSettings>>(
    '/toolbox-talks/settings'
  );
  return response.data.data;
}

export async function updateToolboxTalkSettings(
  data: UpdateToolboxTalkSettingsRequest
): Promise<ToolboxTalkSettings> {
  const response = await apiClient.put<ApiResponse<ToolboxTalkSettings>>(
    '/toolbox-talks/settings',
    data
  );
  return response.data.data;
}
