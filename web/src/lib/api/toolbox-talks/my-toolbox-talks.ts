import { apiClient } from '@/lib/api/client';
import type { ApiResponse } from '@/types/auth';
import type {
  MyToolboxTalk,
  MyToolboxTalkListItem,
  MyTrainingSummary,
  ScheduledTalkSectionProgress,
  ScheduledTalkCompletion,
  QuizResult,
  VideoProgress,
  GetMyToolboxTalksParams,
  MarkSectionReadRequest,
  SubmitQuizRequest,
  UpdateVideoProgressRequest,
  CompleteToolboxTalkRequest,
} from '@/types/toolbox-talks';
import type { PaginatedResponse } from './toolbox-talks';

// ============================================
// My Toolbox Talks (Employee Portal)
// ============================================

export async function getMyToolboxTalks(
  params?: GetMyToolboxTalksParams
): Promise<PaginatedResponse<MyToolboxTalkListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.status) {
    queryParams.append('status', params.status);
  }
  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/my/toolbox-talks?${queryString}` : '/my/toolbox-talks';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<MyToolboxTalkListItem>>>(
    url
  );

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

export async function getMyToolboxTalkById(id: string): Promise<MyToolboxTalk> {
  const response = await apiClient.get<MyToolboxTalk>(`/my/toolbox-talks/${id}`);
  return response.data;
}

// Status-filtered convenience functions
export async function getMyPendingToolboxTalks(
  params?: Omit<GetMyToolboxTalksParams, 'status'>
): Promise<PaginatedResponse<MyToolboxTalkListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString
    ? `/my/toolbox-talks/pending?${queryString}`
    : '/my/toolbox-talks/pending';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<MyToolboxTalkListItem>>>(
    url
  );

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

export async function getMyInProgressToolboxTalks(
  params?: Omit<GetMyToolboxTalksParams, 'status'>
): Promise<PaginatedResponse<MyToolboxTalkListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString
    ? `/my/toolbox-talks/in-progress?${queryString}`
    : '/my/toolbox-talks/in-progress';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<MyToolboxTalkListItem>>>(
    url
  );

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

export async function getMyOverdueToolboxTalks(
  params?: Omit<GetMyToolboxTalksParams, 'status'>
): Promise<PaginatedResponse<MyToolboxTalkListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString
    ? `/my/toolbox-talks/overdue?${queryString}`
    : '/my/toolbox-talks/overdue';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<MyToolboxTalkListItem>>>(
    url
  );

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

export async function getMyCompletedToolboxTalks(
  params?: Omit<GetMyToolboxTalksParams, 'status'>
): Promise<PaginatedResponse<MyToolboxTalkListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString
    ? `/my/toolbox-talks/completed?${queryString}`
    : '/my/toolbox-talks/completed';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<MyToolboxTalkListItem>>>(
    url
  );

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

// ============================================
// Summary (for banner/badge count)
// ============================================

export async function getMyTrainingSummary(): Promise<MyTrainingSummary> {
  const response = await apiClient.get<MyTrainingSummary>('/my/toolbox-talks/summary');
  return response.data;
}

// ============================================
// Progress Tracking
// ============================================

export async function markSectionRead(
  scheduledTalkId: string,
  sectionId: string,
  data?: MarkSectionReadRequest
): Promise<ScheduledTalkSectionProgress> {
  const response = await apiClient.post<ScheduledTalkSectionProgress>(
    `/my/toolbox-talks/${scheduledTalkId}/sections/${sectionId}/read`,
    data || {}
  );
  return response.data;
}

export async function updateVideoProgress(
  scheduledTalkId: string,
  data: UpdateVideoProgressRequest
): Promise<VideoProgress> {
  const response = await apiClient.post<VideoProgress>(
    `/my/toolbox-talks/${scheduledTalkId}/video-progress`,
    data
  );
  return response.data;
}

// ============================================
// Quiz
// ============================================

export async function submitQuizAnswers(
  scheduledTalkId: string,
  data: SubmitQuizRequest
): Promise<QuizResult> {
  const response = await apiClient.post<QuizResult>(
    `/my/toolbox-talks/${scheduledTalkId}/quiz/submit`,
    data
  );
  return response.data;
}

// ============================================
// Completion
// ============================================

export async function completeToolboxTalk(
  scheduledTalkId: string,
  data: CompleteToolboxTalkRequest
): Promise<ScheduledTalkCompletion> {
  const response = await apiClient.post<ScheduledTalkCompletion>(
    `/my/toolbox-talks/${scheduledTalkId}/complete`,
    data
  );
  return response.data;
}
