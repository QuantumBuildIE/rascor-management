import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type { ApiResponse } from '@/types/auth';
import type {
  RamsDocument,
  RamsDocumentListItem,
  CreateRamsDocumentDto,
  UpdateRamsDocumentDto,
  GetRamsParams,
  RamsStatus,
  RiskAssessmentDto,
  CreateRiskAssessmentDto,
  UpdateRiskAssessmentDto,
  MethodStepDto,
  CreateMethodStepDto,
  UpdateMethodStepDto,
  HazardLibraryDto,
  CreateHazardLibraryDto,
  UpdateHazardLibraryDto,
  ControlMeasureLibraryDto,
  CreateControlMeasureLibraryDto,
  UpdateControlMeasureLibraryDto,
  LegislationReferenceDto,
  CreateLegislationReferenceDto,
  UpdateLegislationReferenceDto,
  SopReferenceDto,
  CreateSopReferenceDto,
  UpdateSopReferenceDto,
  HazardCategory,
  ControlHierarchy,
} from '@/types/rams';

// Paginated response type
export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Query keys
export const RAMS_KEY = ['rams'];

// API functions
async function getRamsDocuments(params?: GetRamsParams): Promise<PaginatedResponse<RamsDocumentListItem>> {
  const queryParams = new URLSearchParams();

  if (params?.pageNumber) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize) {
    queryParams.append('pageSize', String(params.pageSize));
  }
  if (params?.sortColumn) {
    queryParams.append('sortColumn', params.sortColumn);
  }
  if (params?.sortDirection) {
    queryParams.append('sortDirection', params.sortDirection);
  }
  if (params?.search) {
    queryParams.append('search', params.search);
  }
  if (params?.status) {
    queryParams.append('status', params.status);
  }
  if (params?.projectType) {
    queryParams.append('projectType', params.projectType);
  }
  if (params?.siteId) {
    queryParams.append('siteId', params.siteId);
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/rams?${queryString}` : '/rams';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<RamsDocumentListItem>>>(url);

  const data = response.data.data;
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

async function getRamsDocument(id: string): Promise<RamsDocument> {
  const response = await apiClient.get<RamsDocument>(`/rams/${id}`);
  return response.data;
}

async function createRamsDocument(data: CreateRamsDocumentDto): Promise<RamsDocument> {
  const response = await apiClient.post<RamsDocument>('/rams', data);
  return response.data;
}

async function updateRamsDocument(id: string, data: UpdateRamsDocumentDto): Promise<RamsDocument> {
  const response = await apiClient.put<RamsDocument>(`/rams/${id}`, data);
  return response.data;
}

async function deleteRamsDocument(id: string): Promise<void> {
  await apiClient.delete(`/rams/${id}`);
}

// Workflow actions
async function submitRamsDocument(id: string): Promise<RamsDocument> {
  const response = await apiClient.post<RamsDocument>(`/rams/${id}/submit`);
  return response.data;
}

async function approveRamsDocument(id: string, comments?: string): Promise<RamsDocument> {
  const response = await apiClient.post<RamsDocument>(`/rams/${id}/approve`, { comments });
  return response.data;
}

async function rejectRamsDocument(id: string, comments: string): Promise<RamsDocument> {
  const response = await apiClient.post<RamsDocument>(`/rams/${id}/reject`, { comments });
  return response.data;
}

// Query hooks
export function useRamsDocuments(params?: GetRamsParams) {
  return useQuery({
    queryKey: [...RAMS_KEY, params],
    queryFn: () => getRamsDocuments(params),
  });
}

export function useRamsDocument(id: string) {
  return useQuery({
    queryKey: [...RAMS_KEY, id],
    queryFn: () => getRamsDocument(id),
    enabled: !!id,
  });
}

// Mutation hooks
export function useCreateRamsDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRamsDocumentDto) => createRamsDocument(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: RAMS_KEY });
    },
  });
}

export function useUpdateRamsDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRamsDocumentDto }) => updateRamsDocument(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: RAMS_KEY });
    },
  });
}

export function useDeleteRamsDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteRamsDocument(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: RAMS_KEY });
    },
  });
}

// Workflow mutation hooks
export function useSubmitRamsDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => submitRamsDocument(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: RAMS_KEY });
    },
  });
}

export function useApproveRamsDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, comments }: { id: string; comments?: string }) => approveRamsDocument(id, comments),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: RAMS_KEY });
    },
  });
}

export function useRejectRamsDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, comments }: { id: string; comments: string }) => rejectRamsDocument(id, comments),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: RAMS_KEY });
    },
  });
}

// Helper function for status badge color
export function getStatusColor(status: RamsStatus): string {
  const colors: Record<RamsStatus, string> = {
    Draft: 'bg-gray-100 text-gray-800',
    PendingReview: 'bg-yellow-100 text-yellow-800',
    Approved: 'bg-green-100 text-green-800',
    Rejected: 'bg-red-100 text-red-800',
    Archived: 'bg-purple-100 text-purple-800',
  };
  return colors[status] || 'bg-gray-100 text-gray-800';
}

// Helper for status display name
export function getStatusDisplayName(status: RamsStatus): string {
  const names: Record<RamsStatus, string> = {
    Draft: 'Draft',
    PendingReview: 'Pending Review',
    Approved: 'Approved',
    Rejected: 'Rejected',
    Archived: 'Archived',
  };
  return names[status] || status;
}

// ============================================
// Risk Assessments API
// ============================================

export const RISK_ASSESSMENTS_KEY = ['risk-assessments'];

async function getRiskAssessments(ramsDocumentId: string): Promise<RiskAssessmentDto[]> {
  const response = await apiClient.get<RiskAssessmentDto[]>(`/rams/${ramsDocumentId}/risk-assessments`);
  return response.data;
}

async function getRiskAssessment(ramsDocumentId: string, id: string): Promise<RiskAssessmentDto> {
  const response = await apiClient.get<RiskAssessmentDto>(`/rams/${ramsDocumentId}/risk-assessments/${id}`);
  return response.data;
}

async function createRiskAssessment(ramsDocumentId: string, data: CreateRiskAssessmentDto): Promise<RiskAssessmentDto> {
  const response = await apiClient.post<RiskAssessmentDto>(`/rams/${ramsDocumentId}/risk-assessments`, data);
  return response.data;
}

async function updateRiskAssessment(ramsDocumentId: string, id: string, data: UpdateRiskAssessmentDto): Promise<RiskAssessmentDto> {
  const response = await apiClient.put<RiskAssessmentDto>(`/rams/${ramsDocumentId}/risk-assessments/${id}`, data);
  return response.data;
}

async function deleteRiskAssessment(ramsDocumentId: string, id: string): Promise<void> {
  await apiClient.delete(`/rams/${ramsDocumentId}/risk-assessments/${id}`);
}

async function reorderRiskAssessments(ramsDocumentId: string, orderedIds: string[]): Promise<RiskAssessmentDto[]> {
  const response = await apiClient.post<RiskAssessmentDto[]>(`/rams/${ramsDocumentId}/risk-assessments/reorder`, { orderedIds });
  return response.data;
}

// Risk Assessments Query Hooks
export function useRiskAssessments(ramsDocumentId: string) {
  return useQuery({
    queryKey: [...RISK_ASSESSMENTS_KEY, ramsDocumentId],
    queryFn: () => getRiskAssessments(ramsDocumentId),
    enabled: !!ramsDocumentId,
  });
}

export function useRiskAssessment(ramsDocumentId: string, id: string) {
  return useQuery({
    queryKey: [...RISK_ASSESSMENTS_KEY, ramsDocumentId, id],
    queryFn: () => getRiskAssessment(ramsDocumentId, id),
    enabled: !!ramsDocumentId && !!id,
  });
}

// Risk Assessments Mutation Hooks
export function useCreateRiskAssessment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, data }: { ramsDocumentId: string; data: CreateRiskAssessmentDto }) =>
      createRiskAssessment(ramsDocumentId, data),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...RISK_ASSESSMENTS_KEY, ramsDocumentId] });
      queryClient.invalidateQueries({ queryKey: [...RAMS_KEY, ramsDocumentId] });
    },
  });
}

export function useUpdateRiskAssessment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, id, data }: { ramsDocumentId: string; id: string; data: UpdateRiskAssessmentDto }) =>
      updateRiskAssessment(ramsDocumentId, id, data),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...RISK_ASSESSMENTS_KEY, ramsDocumentId] });
      queryClient.invalidateQueries({ queryKey: [...RAMS_KEY, ramsDocumentId] });
    },
  });
}

export function useDeleteRiskAssessment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, id }: { ramsDocumentId: string; id: string }) =>
      deleteRiskAssessment(ramsDocumentId, id),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...RISK_ASSESSMENTS_KEY, ramsDocumentId] });
      queryClient.invalidateQueries({ queryKey: [...RAMS_KEY, ramsDocumentId] });
    },
  });
}

export function useReorderRiskAssessments() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, orderedIds }: { ramsDocumentId: string; orderedIds: string[] }) =>
      reorderRiskAssessments(ramsDocumentId, orderedIds),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...RISK_ASSESSMENTS_KEY, ramsDocumentId] });
    },
  });
}

// ============================================
// Method Steps API
// ============================================

export const METHOD_STEPS_KEY = ['method-steps'];

async function getMethodSteps(ramsDocumentId: string): Promise<MethodStepDto[]> {
  const response = await apiClient.get<MethodStepDto[]>(`/rams/${ramsDocumentId}/method-steps`);
  return response.data;
}

async function getMethodStep(ramsDocumentId: string, id: string): Promise<MethodStepDto> {
  const response = await apiClient.get<MethodStepDto>(`/rams/${ramsDocumentId}/method-steps/${id}`);
  return response.data;
}

async function createMethodStep(ramsDocumentId: string, data: CreateMethodStepDto): Promise<MethodStepDto> {
  const response = await apiClient.post<MethodStepDto>(`/rams/${ramsDocumentId}/method-steps`, data);
  return response.data;
}

async function updateMethodStep(ramsDocumentId: string, id: string, data: UpdateMethodStepDto): Promise<MethodStepDto> {
  const response = await apiClient.put<MethodStepDto>(`/rams/${ramsDocumentId}/method-steps/${id}`, data);
  return response.data;
}

async function deleteMethodStep(ramsDocumentId: string, id: string): Promise<void> {
  await apiClient.delete(`/rams/${ramsDocumentId}/method-steps/${id}`);
}

async function reorderMethodSteps(ramsDocumentId: string, orderedIds: string[]): Promise<MethodStepDto[]> {
  const response = await apiClient.post<MethodStepDto[]>(`/rams/${ramsDocumentId}/method-steps/reorder`, { orderedIds });
  return response.data;
}

// Method Steps Query Hooks
export function useMethodSteps(ramsDocumentId: string) {
  return useQuery({
    queryKey: [...METHOD_STEPS_KEY, ramsDocumentId],
    queryFn: () => getMethodSteps(ramsDocumentId),
    enabled: !!ramsDocumentId,
  });
}

export function useMethodStep(ramsDocumentId: string, id: string) {
  return useQuery({
    queryKey: [...METHOD_STEPS_KEY, ramsDocumentId, id],
    queryFn: () => getMethodStep(ramsDocumentId, id),
    enabled: !!ramsDocumentId && !!id,
  });
}

// Method Steps Mutation Hooks
export function useCreateMethodStep() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, data }: { ramsDocumentId: string; data: CreateMethodStepDto }) =>
      createMethodStep(ramsDocumentId, data),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...METHOD_STEPS_KEY, ramsDocumentId] });
      queryClient.invalidateQueries({ queryKey: [...RAMS_KEY, ramsDocumentId] });
    },
  });
}

export function useUpdateMethodStep() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, id, data }: { ramsDocumentId: string; id: string; data: UpdateMethodStepDto }) =>
      updateMethodStep(ramsDocumentId, id, data),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...METHOD_STEPS_KEY, ramsDocumentId] });
      queryClient.invalidateQueries({ queryKey: [...RAMS_KEY, ramsDocumentId] });
    },
  });
}

export function useDeleteMethodStep() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, id }: { ramsDocumentId: string; id: string }) =>
      deleteMethodStep(ramsDocumentId, id),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...METHOD_STEPS_KEY, ramsDocumentId] });
      queryClient.invalidateQueries({ queryKey: [...RAMS_KEY, ramsDocumentId] });
    },
  });
}

export function useReorderMethodSteps() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ ramsDocumentId, orderedIds }: { ramsDocumentId: string; orderedIds: string[] }) =>
      reorderMethodSteps(ramsDocumentId, orderedIds),
    onSuccess: (_, { ramsDocumentId }) => {
      queryClient.invalidateQueries({ queryKey: [...METHOD_STEPS_KEY, ramsDocumentId] });
    },
  });
}

// ============================================
// Hazard Library API
// ============================================

export const HAZARD_LIBRARY_KEY = ['hazard-library'];

interface HazardLibraryParams {
  includeInactive?: boolean;
  category?: HazardCategory;
  search?: string;
}

async function getHazardLibrary(params?: HazardLibraryParams): Promise<HazardLibraryDto[]> {
  const searchParams = new URLSearchParams();
  if (params?.includeInactive) searchParams.append('includeInactive', 'true');
  if (params?.category !== undefined) searchParams.append('category', params.category.toString());
  if (params?.search) searchParams.append('search', params.search);
  const query = searchParams.toString();
  const response = await apiClient.get<HazardLibraryDto[]>(`/rams/library/hazards${query ? `?${query}` : ''}`);
  return response.data;
}

async function getHazardLibraryItem(id: string): Promise<HazardLibraryDto> {
  const response = await apiClient.get<HazardLibraryDto>(`/rams/library/hazards/${id}`);
  return response.data;
}

async function createHazardLibraryItem(data: CreateHazardLibraryDto): Promise<HazardLibraryDto> {
  const response = await apiClient.post<HazardLibraryDto>('/rams/library/hazards', data);
  return response.data;
}

async function updateHazardLibraryItem(id: string, data: UpdateHazardLibraryDto): Promise<HazardLibraryDto> {
  const response = await apiClient.put<HazardLibraryDto>(`/rams/library/hazards/${id}`, data);
  return response.data;
}

async function deleteHazardLibraryItem(id: string): Promise<void> {
  await apiClient.delete(`/rams/library/hazards/${id}`);
}

// Hazard Library Query Hooks
export function useHazardLibrary(params?: HazardLibraryParams) {
  return useQuery({
    queryKey: [...HAZARD_LIBRARY_KEY, params],
    queryFn: () => getHazardLibrary(params),
  });
}

export function useHazardLibraryItem(id: string) {
  return useQuery({
    queryKey: [...HAZARD_LIBRARY_KEY, id],
    queryFn: () => getHazardLibraryItem(id),
    enabled: !!id,
  });
}

// Hazard Library Mutation Hooks
export function useCreateHazardLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateHazardLibraryDto) => createHazardLibraryItem(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HAZARD_LIBRARY_KEY });
    },
  });
}

export function useUpdateHazardLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateHazardLibraryDto }) => updateHazardLibraryItem(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HAZARD_LIBRARY_KEY });
    },
  });
}

export function useDeleteHazardLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteHazardLibraryItem(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HAZARD_LIBRARY_KEY });
    },
  });
}

// ============================================
// Control Measure Library API
// ============================================

export const CONTROL_LIBRARY_KEY = ['control-library'];

interface ControlLibraryParams {
  includeInactive?: boolean;
  category?: HazardCategory;
  hierarchy?: ControlHierarchy;
  search?: string;
}

async function getControlLibrary(params?: ControlLibraryParams): Promise<ControlMeasureLibraryDto[]> {
  const searchParams = new URLSearchParams();
  if (params?.includeInactive) searchParams.append('includeInactive', 'true');
  if (params?.category !== undefined) searchParams.append('category', params.category.toString());
  if (params?.hierarchy !== undefined) searchParams.append('hierarchy', params.hierarchy.toString());
  if (params?.search) searchParams.append('search', params.search);
  const query = searchParams.toString();
  const response = await apiClient.get<ControlMeasureLibraryDto[]>(`/rams/library/controls${query ? `?${query}` : ''}`);
  return response.data;
}

async function getControlLibraryItem(id: string): Promise<ControlMeasureLibraryDto> {
  const response = await apiClient.get<ControlMeasureLibraryDto>(`/rams/library/controls/${id}`);
  return response.data;
}

async function createControlLibraryItem(data: CreateControlMeasureLibraryDto): Promise<ControlMeasureLibraryDto> {
  const response = await apiClient.post<ControlMeasureLibraryDto>('/rams/library/controls', data);
  return response.data;
}

async function updateControlLibraryItem(id: string, data: UpdateControlMeasureLibraryDto): Promise<ControlMeasureLibraryDto> {
  const response = await apiClient.put<ControlMeasureLibraryDto>(`/rams/library/controls/${id}`, data);
  return response.data;
}

async function deleteControlLibraryItem(id: string): Promise<void> {
  await apiClient.delete(`/rams/library/controls/${id}`);
}

// Control Library Query Hooks
export function useControlLibrary(params?: ControlLibraryParams) {
  return useQuery({
    queryKey: [...CONTROL_LIBRARY_KEY, params],
    queryFn: () => getControlLibrary(params),
  });
}

export function useControlLibraryItem(id: string) {
  return useQuery({
    queryKey: [...CONTROL_LIBRARY_KEY, id],
    queryFn: () => getControlLibraryItem(id),
    enabled: !!id,
  });
}

// Control Library Mutation Hooks
export function useCreateControlLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateControlMeasureLibraryDto) => createControlLibraryItem(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CONTROL_LIBRARY_KEY });
    },
  });
}

export function useUpdateControlLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateControlMeasureLibraryDto }) => updateControlLibraryItem(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CONTROL_LIBRARY_KEY });
    },
  });
}

export function useDeleteControlLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteControlLibraryItem(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CONTROL_LIBRARY_KEY });
    },
  });
}

// ============================================
// Legislation Reference Library API
// ============================================

export const LEGISLATION_LIBRARY_KEY = ['legislation-library'];

interface LegislationLibraryParams {
  includeInactive?: boolean;
  jurisdiction?: string;
  search?: string;
}

async function getLegislationLibrary(params?: LegislationLibraryParams): Promise<LegislationReferenceDto[]> {
  const searchParams = new URLSearchParams();
  if (params?.includeInactive) searchParams.append('includeInactive', 'true');
  if (params?.jurisdiction) searchParams.append('jurisdiction', params.jurisdiction);
  if (params?.search) searchParams.append('search', params.search);
  const query = searchParams.toString();
  const response = await apiClient.get<LegislationReferenceDto[]>(`/rams/library/legislation${query ? `?${query}` : ''}`);
  return response.data;
}

async function getLegislationLibraryItem(id: string): Promise<LegislationReferenceDto> {
  const response = await apiClient.get<LegislationReferenceDto>(`/rams/library/legislation/${id}`);
  return response.data;
}

async function createLegislationLibraryItem(data: CreateLegislationReferenceDto): Promise<LegislationReferenceDto> {
  const response = await apiClient.post<LegislationReferenceDto>('/rams/library/legislation', data);
  return response.data;
}

async function updateLegislationLibraryItem(id: string, data: UpdateLegislationReferenceDto): Promise<LegislationReferenceDto> {
  const response = await apiClient.put<LegislationReferenceDto>(`/rams/library/legislation/${id}`, data);
  return response.data;
}

async function deleteLegislationLibraryItem(id: string): Promise<void> {
  await apiClient.delete(`/rams/library/legislation/${id}`);
}

// Legislation Library Query Hooks
export function useLegislationLibrary(params?: LegislationLibraryParams) {
  return useQuery({
    queryKey: [...LEGISLATION_LIBRARY_KEY, params],
    queryFn: () => getLegislationLibrary(params),
  });
}

export function useLegislationLibraryItem(id: string) {
  return useQuery({
    queryKey: [...LEGISLATION_LIBRARY_KEY, id],
    queryFn: () => getLegislationLibraryItem(id),
    enabled: !!id,
  });
}

// Legislation Library Mutation Hooks
export function useCreateLegislationLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateLegislationReferenceDto) => createLegislationLibraryItem(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: LEGISLATION_LIBRARY_KEY });
    },
  });
}

export function useUpdateLegislationLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateLegislationReferenceDto }) => updateLegislationLibraryItem(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: LEGISLATION_LIBRARY_KEY });
    },
  });
}

export function useDeleteLegislationLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteLegislationLibraryItem(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: LEGISLATION_LIBRARY_KEY });
    },
  });
}

// ============================================
// SOP Reference Library API
// ============================================

export const SOP_LIBRARY_KEY = ['sop-library'];

interface SopLibraryParams {
  includeInactive?: boolean;
  search?: string;
}

async function getSopLibrary(params?: SopLibraryParams): Promise<SopReferenceDto[]> {
  const searchParams = new URLSearchParams();
  if (params?.includeInactive) searchParams.append('includeInactive', 'true');
  if (params?.search) searchParams.append('search', params.search);
  const query = searchParams.toString();
  const response = await apiClient.get<SopReferenceDto[]>(`/rams/library/sops${query ? `?${query}` : ''}`);
  return response.data;
}

async function getSopLibraryItem(id: string): Promise<SopReferenceDto> {
  const response = await apiClient.get<SopReferenceDto>(`/rams/library/sops/${id}`);
  return response.data;
}

async function createSopLibraryItem(data: CreateSopReferenceDto): Promise<SopReferenceDto> {
  const response = await apiClient.post<SopReferenceDto>('/rams/library/sops', data);
  return response.data;
}

async function updateSopLibraryItem(id: string, data: UpdateSopReferenceDto): Promise<SopReferenceDto> {
  const response = await apiClient.put<SopReferenceDto>(`/rams/library/sops/${id}`, data);
  return response.data;
}

async function deleteSopLibraryItem(id: string): Promise<void> {
  await apiClient.delete(`/rams/library/sops/${id}`);
}

// SOP Library Query Hooks
export function useSopLibrary(params?: SopLibraryParams) {
  return useQuery({
    queryKey: [...SOP_LIBRARY_KEY, params],
    queryFn: () => getSopLibrary(params),
  });
}

export function useSopLibraryItem(id: string) {
  return useQuery({
    queryKey: [...SOP_LIBRARY_KEY, id],
    queryFn: () => getSopLibraryItem(id),
    enabled: !!id,
  });
}

// SOP Library Mutation Hooks
export function useCreateSopLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateSopReferenceDto) => createSopLibraryItem(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SOP_LIBRARY_KEY });
    },
  });
}

export function useUpdateSopLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSopReferenceDto }) => updateSopLibraryItem(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SOP_LIBRARY_KEY });
    },
  });
}

export function useDeleteSopLibraryItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteSopLibraryItem(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SOP_LIBRARY_KEY });
    },
  });
}

// ============================================
// AI Suggestions API
// ============================================

import type {
  ControlMeasureSuggestionRequest,
  ControlMeasureSuggestionResponse,
  AcceptSuggestionDto,
  HazardMatch,
  ControlMatch,
  LegislationMatch,
  SopMatch,
} from '@/types/rams';

export const AI_SUGGESTIONS_KEY = ['ai-suggestions'];

async function suggestControls(
  request: ControlMeasureSuggestionRequest,
  ramsDocumentId?: string,
  riskAssessmentId?: string
): Promise<ControlMeasureSuggestionResponse> {
  const params = new URLSearchParams();
  if (ramsDocumentId) params.append('ramsDocumentId', ramsDocumentId);
  if (riskAssessmentId) params.append('riskAssessmentId', riskAssessmentId);
  const query = params.toString();

  const response = await apiClient.post<ControlMeasureSuggestionResponse>(
    `/rams/ai/suggest-controls${query ? `?${query}` : ''}`,
    request
  );
  return response.data;
}

async function acceptSuggestion(auditLogId: string, accepted: boolean): Promise<void> {
  await apiClient.post('/rams/ai/accept-suggestion', { auditLogId, accepted });
}

// AI Suggestions Mutation Hooks
export function useSuggestControls() {
  return useMutation({
    mutationFn: ({
      request,
      ramsDocumentId,
      riskAssessmentId,
    }: {
      request: ControlMeasureSuggestionRequest;
      ramsDocumentId?: string;
      riskAssessmentId?: string;
    }) => suggestControls(request, ramsDocumentId, riskAssessmentId),
  });
}

export function useAcceptSuggestion() {
  return useMutation({
    mutationFn: ({ auditLogId, accepted }: { auditLogId: string; accepted: boolean }) =>
      acceptSuggestion(auditLogId, accepted),
  });
}

// ============================================
// Dashboard API
// ============================================

import type {
  RamsDashboard,
  RamsPendingApproval,
  RamsOverdueDocument,
  RamsExportRequest,
} from '@/types/rams';

export const RAMS_DASHBOARD_KEY = ['rams-dashboard'];

async function getRamsDashboard(): Promise<RamsDashboard> {
  const response = await apiClient.get<RamsDashboard>('/rams/dashboard');
  return response.data;
}

async function getPendingApprovals(): Promise<RamsPendingApproval[]> {
  const response = await apiClient.get<RamsPendingApproval[]>('/rams/dashboard/pending-approvals');
  return response.data;
}

async function getOverdueDocuments(): Promise<RamsOverdueDocument[]> {
  const response = await apiClient.get<RamsOverdueDocument[]>('/rams/dashboard/overdue');
  return response.data;
}

async function exportRamsToExcel(request: RamsExportRequest): Promise<Blob> {
  const response = await apiClient.post('/rams/dashboard/export', request, {
    responseType: 'blob',
  });
  return response.data;
}

// Dashboard Query Hooks
export function useRamsDashboard() {
  return useQuery({
    queryKey: RAMS_DASHBOARD_KEY,
    queryFn: getRamsDashboard,
  });
}

export function usePendingApprovals() {
  return useQuery({
    queryKey: [...RAMS_DASHBOARD_KEY, 'pending-approvals'],
    queryFn: getPendingApprovals,
  });
}

export function useOverdueDocuments() {
  return useQuery({
    queryKey: [...RAMS_DASHBOARD_KEY, 'overdue'],
    queryFn: getOverdueDocuments,
  });
}

// Export Mutation Hook
export function useExportRamsToExcel() {
  return useMutation({
    mutationFn: (request: RamsExportRequest) => exportRamsToExcel(request),
  });
}

// Export types for convenience
export type {
  CreateRamsDocumentDto,
  UpdateRamsDocumentDto,
  GetRamsParams,
  RamsDocumentListItem,
  RiskAssessmentDto,
  CreateRiskAssessmentDto,
  UpdateRiskAssessmentDto,
  MethodStepDto,
  CreateMethodStepDto,
  UpdateMethodStepDto,
  HazardLibraryDto,
  CreateHazardLibraryDto,
  UpdateHazardLibraryDto,
  ControlMeasureLibraryDto,
  CreateControlMeasureLibraryDto,
  UpdateControlMeasureLibraryDto,
  LegislationReferenceDto,
  CreateLegislationReferenceDto,
  UpdateLegislationReferenceDto,
  SopReferenceDto,
  CreateSopReferenceDto,
  UpdateSopReferenceDto,
  ControlMeasureSuggestionRequest,
  ControlMeasureSuggestionResponse,
  AcceptSuggestionDto,
  HazardMatch,
  ControlMatch,
  LegislationMatch,
  SopMatch,
  RamsDashboard,
  RamsPendingApproval,
  RamsOverdueDocument,
  RamsExportRequest,
};
