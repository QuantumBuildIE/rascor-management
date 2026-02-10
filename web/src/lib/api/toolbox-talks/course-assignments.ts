import { apiClient } from '@/lib/api/client';
import type { ApiResponse } from '@/types/auth';

// ============================================
// Course Assignment DTOs
// ============================================

export interface ToolboxTalkCourseAssignmentDto {
  id: string;
  courseId: string;
  courseTitle: string;
  courseDescription?: string;
  employeeId: string;
  employeeName: string;
  employeeCode?: string;
  assignedAt: string;
  dueDate?: string;
  startedAt?: string;
  completedAt?: string;
  status: string;
  isRefresher: boolean;
  totalTalks: number;
  completedTalks: number;
  progressPercent: number;
  scheduledTalks: CourseScheduledTalkDto[];
}

export interface CourseScheduledTalkDto {
  scheduledTalkId: string;
  toolboxTalkId: string;
  talkTitle: string;
  orderIndex: number;
  isRequired: boolean;
  status: string;
  completedAt?: string;
  isLocked: boolean;
  lockedReason?: string;
}

export interface AssignCourseDto {
  courseId: string;
  employeeIds: string[];
  dueDate?: string;
}

export interface CourseAssignmentListDto {
  id: string;
  courseId: string;
  courseTitle: string;
  employeeId: string;
  employeeName: string;
  dueDate?: string;
  status: string;
  totalTalks: number;
  completedTalks: number;
  assignedAt: string;
  completedAt?: string;
}

// ============================================
// API Functions
// ============================================

export async function assignCourse(
  data: AssignCourseDto
): Promise<ToolboxTalkCourseAssignmentDto[]> {
  const response = await apiClient.post<ApiResponse<ToolboxTalkCourseAssignmentDto[]>>(
    '/toolbox-talks/course-assignments',
    data
  );
  return response.data.data ?? [];
}

export async function getCourseAssignments(
  courseId: string
): Promise<CourseAssignmentListDto[]> {
  const response = await apiClient.get<ApiResponse<CourseAssignmentListDto[]>>(
    `/toolbox-talks/course-assignments/by-course/${courseId}`
  );
  return response.data.data ?? [];
}

export async function getCourseAssignment(
  id: string
): Promise<ToolboxTalkCourseAssignmentDto> {
  const response = await apiClient.get<ToolboxTalkCourseAssignmentDto>(
    `/toolbox-talks/course-assignments/${id}`
  );
  return response.data;
}

export async function deleteCourseAssignment(id: string): Promise<void> {
  await apiClient.delete(`/toolbox-talks/course-assignments/${id}`);
}

export async function getMyCourseAssignment(
  id: string
): Promise<ToolboxTalkCourseAssignmentDto> {
  const response = await apiClient.get<ToolboxTalkCourseAssignmentDto>(
    `/my/toolbox-talks/courses/${id}`
  );
  return response.data;
}

export async function getMyCourseAssignments(): Promise<ToolboxTalkCourseAssignmentDto[]> {
  const response = await apiClient.get<ApiResponse<ToolboxTalkCourseAssignmentDto[]>>(
    '/my/toolbox-talks/courses'
  );
  return response.data.data ?? [];
}
