import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type { ApiResponse } from '@/types/auth';

// Types
export type ProposalStatus =
  | 'Draft'
  | 'Submitted'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected'
  | 'Won'
  | 'Lost'
  | 'Expired'
  | 'Cancelled';

export interface ProposalContact {
  id: string;
  proposalId: string;
  contactId?: string;
  contactName: string;
  email?: string;
  phone?: string;
  role: string;
  isPrimary: boolean;
}

export interface ProposalLineItem {
  id: string;
  proposalSectionId: string;
  productId?: string;
  productCode?: string;
  description: string;
  quantity: number;
  unit: string;
  unitCost: number;
  unitPrice: number;
  lineTotal: number;
  lineCost: number;
  lineMargin: number;
  marginPercent: number;
  sortOrder: number;
  notes?: string;
}

export interface ProposalSection {
  id: string;
  proposalId: string;
  sourceKitId?: string;
  sourceKitName?: string;
  sectionName: string;
  description?: string;
  sortOrder: number;
  sectionCost: number;
  sectionTotal: number;
  sectionMargin: number;
  lineItems: ProposalLineItem[];
}

export interface Proposal {
  id: string;
  proposalNumber: string;
  version: number;
  parentProposalId?: string;
  companyId: string;
  companyName: string;
  primaryContactId?: string;
  primaryContactName?: string;
  projectName: string;
  projectAddress?: string;
  projectDescription?: string;
  proposalDate: string;
  validUntilDate?: string;
  submittedDate?: string;
  approvedDate?: string;
  approvedBy?: string;
  wonDate?: string;
  lostDate?: string;
  status: ProposalStatus;
  wonLostReason?: string;
  currency: string;
  subtotal: number;
  discountPercent: number;
  discountAmount: number;
  netTotal: number;
  vatRate: number;
  vatAmount: number;
  grandTotal: number;
  totalCost?: number;
  totalMargin?: number;
  marginPercent?: number;
  paymentTerms?: string;
  termsAndConditions?: string;
  notes?: string;
  drawingFileName?: string;
  drawingUrl?: string;
  sections: ProposalSection[];
  contacts: ProposalContact[];
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
}

export interface ProposalListItem {
  id: string;
  proposalNumber: string;
  version: number;
  projectName: string;
  companyName: string;
  proposalDate: string;
  validUntilDate?: string;
  status: ProposalStatus;
  grandTotal: number;
  currency: string;
  marginPercent?: number;
  createdAt: string;
}

export interface CreateProposalDto {
  companyId: string;
  primaryContactId?: string;
  projectName: string;
  projectAddress?: string;
  projectDescription?: string;
  proposalDate: string;
  validUntilDate?: string;
  currency?: string;
  vatRate?: number;
  discountPercent?: number;
  paymentTerms?: string;
  termsAndConditions?: string;
  notes?: string;
}

export interface UpdateProposalDto extends CreateProposalDto {}

export interface GetProposalsParams {
  search?: string;
  status?: string;
  companyId?: string;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
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

export interface ProposalSummary {
  totalCount: number;
  pipelineValue: number;
  wonThisMonthCount: number;
  wonThisMonthValue: number;
  conversionRate: number;
}

// Query keys
export const PROPOSALS_KEY = ['proposals'];

// API functions
async function getProposals(params?: GetProposalsParams): Promise<PaginatedResponse<ProposalListItem>> {
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
  if (params?.companyId) {
    queryParams.append('companyId', params.companyId);
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/proposals?${queryString}` : '/proposals';

  const response = await apiClient.get<ApiResponse<PaginatedResponse<ProposalListItem>>>(url);

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

async function getProposal(id: string, includeCosting: boolean = false): Promise<Proposal> {
  const url = includeCosting ? `/proposals/${id}?includeCosting=true` : `/proposals/${id}`;
  const response = await apiClient.get<Proposal>(url);
  return response.data;
}

async function getProposalRevisions(id: string): Promise<ProposalListItem[]> {
  const response = await apiClient.get<ProposalListItem[]>(`/proposals/${id}/revisions`);
  return response.data ?? [];
}

async function getProposalSummary(): Promise<ProposalSummary> {
  const response = await apiClient.get<ApiResponse<ProposalSummary>>('/proposals/summary');
  return response.data.data ?? {
    totalCount: 0,
    pipelineValue: 0,
    wonThisMonthCount: 0,
    wonThisMonthValue: 0,
    conversionRate: 0,
  };
}

async function createProposal(data: CreateProposalDto): Promise<Proposal> {
  const response = await apiClient.post<Proposal>('/proposals', data);
  return response.data;
}

async function updateProposal(id: string, data: UpdateProposalDto): Promise<Proposal> {
  const response = await apiClient.put<Proposal>(`/proposals/${id}`, data);
  return response.data;
}

async function deleteProposal(id: string): Promise<void> {
  await apiClient.delete(`/proposals/${id}`);
}

// Workflow actions
async function submitProposal(id: string, notes?: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/submit`, { notes });
  return response.data;
}

async function approveProposal(id: string, notes?: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/approve`, { notes });
  return response.data;
}

async function rejectProposal(id: string, reason: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/reject`, { reason });
  return response.data;
}

async function winProposal(id: string, reason?: string, wonDate?: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/win`, { reason, wonDate });
  return response.data;
}

async function loseProposal(id: string, reason: string, lostDate?: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/lose`, { reason, lostDate });
  return response.data;
}

async function cancelProposal(id: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/cancel`);
  return response.data;
}

async function createRevision(id: string, notes?: string): Promise<Proposal> {
  const response = await apiClient.post<Proposal>(`/proposals/${id}/revise`, { notes });
  return response.data;
}

// Query hooks
export function useProposals(params?: GetProposalsParams) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, params],
    queryFn: () => getProposals(params),
  });
}

export function useProposal(id: string, includeCosting: boolean = false) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, id, includeCosting],
    queryFn: () => getProposal(id, includeCosting),
    enabled: !!id,
  });
}

export function useProposalRevisions(id: string) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, id, 'revisions'],
    queryFn: () => getProposalRevisions(id),
    enabled: !!id,
  });
}

export function useProposalSummary() {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'summary'],
    queryFn: getProposalSummary,
  });
}

// Mutation hooks
export function useCreateProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateProposalDto) => createProposal(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useUpdateProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProposalDto }) => updateProposal(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useDeleteProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteProposal(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

// Workflow mutation hooks
export function useSubmitProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) => submitProposal(id, notes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useApproveProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) => approveProposal(id, notes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useRejectProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => rejectProposal(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useWinProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason, wonDate }: { id: string; reason?: string; wonDate?: string }) =>
      winProposal(id, reason, wonDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useLoseProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason, lostDate }: { id: string; reason: string; lostDate?: string }) =>
      loseProposal(id, reason, lostDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useCancelProposal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => cancelProposal(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useCreateRevision() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) => createRevision(id, notes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

// Section operations
export function useAddSection() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (data: {
      proposalId: string;
      sourceKitId?: string;
      sectionName: string;
      description?: string;
      sortOrder: number;
    }) => {
      const response = await apiClient.post<ProposalSection>(
        `/proposals/${data.proposalId}/sections`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useUpdateSection() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ sectionId, data }: {
      sectionId: string;
      data: { sectionName: string; description?: string; sortOrder: number };
    }) => {
      const response = await apiClient.put<ProposalSection>(
        `/proposals/sections/${sectionId}`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useDeleteSection() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (sectionId: string) => {
      await apiClient.delete(`/proposals/sections/${sectionId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

// Line item operations
export function useAddLineItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ sectionId, data }: {
      sectionId: string;
      data: {
        productId?: string;
        description: string;
        quantity: number;
        unit: string;
        unitCost: number;
        unitPrice: number;
        sortOrder: number;
        notes?: string;
      };
    }) => {
      const response = await apiClient.post<ProposalLineItem>(
        `/proposals/sections/${sectionId}/items`,
        { proposalSectionId: sectionId, ...data }
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useUpdateLineItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ itemId, data }: {
      itemId: string;
      data: {
        productId?: string;
        description: string;
        quantity: number;
        unit: string;
        unitCost: number;
        unitPrice: number;
        sortOrder: number;
        notes?: string;
      };
    }) => {
      const response = await apiClient.put<ProposalLineItem>(
        `/proposals/items/${itemId}`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useDeleteLineItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (itemId: string) => {
      await apiClient.delete(`/proposals/items/${itemId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

// Contact operations
export function useAddContact() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ proposalId, data }: {
      proposalId: string;
      data: {
        contactId?: string;
        contactName: string;
        email?: string;
        phone?: string;
        role: string;
        isPrimary: boolean;
      };
    }) => {
      const response = await apiClient.post<ProposalContact>(
        `/proposals/${proposalId}/contacts`,
        { proposalId, ...data }
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useUpdateContact() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ contactId, data }: {
      contactId: string;
      data: {
        contactId?: string;
        contactName: string;
        email?: string;
        phone?: string;
        role: string;
        isPrimary: boolean;
      };
    }) => {
      const response = await apiClient.put<ProposalContact>(
        `/proposals/contacts/${contactId}`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

export function useDeleteContact() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (contactId: string) => {
      await apiClient.delete(`/proposals/contacts/${contactId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_KEY });
    },
  });
}

// PDF download function
export async function downloadProposalPdf(
  proposalId: string,
  proposalNumber: string,
  version: number,
  includeCosting: boolean = false
): Promise<void> {
  try {
    const url = includeCosting
      ? `/proposals/${proposalId}/pdf?includeCosting=true`
      : `/proposals/${proposalId}/pdf`;

    const response = await apiClient.get(url, {
      responseType: 'blob',
    });

    // Create download link
    const blob = new Blob([response.data], { type: 'application/pdf' });
    const downloadUrl = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.setAttribute('download', `${proposalNumber}_v${version}.pdf`);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(downloadUrl);
  } catch (error) {
    console.error('Failed to download PDF:', error);
    throw error;
  }
}

// Helper function for status badge color
export function getStatusColor(status: ProposalStatus): string {
  const colors: Record<ProposalStatus, string> = {
    Draft: 'bg-gray-100 text-gray-800',
    Submitted: 'bg-blue-100 text-blue-800',
    UnderReview: 'bg-yellow-100 text-yellow-800',
    Approved: 'bg-green-100 text-green-800',
    Rejected: 'bg-red-100 text-red-800',
    Won: 'bg-emerald-100 text-emerald-800',
    Lost: 'bg-orange-100 text-orange-800',
    Expired: 'bg-purple-100 text-purple-800',
    Cancelled: 'bg-gray-100 text-gray-500',
  };
  return colors[status] || 'bg-gray-100 text-gray-800';
}

// Helper for status display name
export function getStatusDisplayName(status: ProposalStatus): string {
  const names: Record<ProposalStatus, string> = {
    Draft: 'Draft',
    Submitted: 'Submitted',
    UnderReview: 'Under Review',
    Approved: 'Approved',
    Rejected: 'Rejected',
    Won: 'Won',
    Lost: 'Lost',
    Expired: 'Expired',
    Cancelled: 'Cancelled',
  };
  return names[status] || status;
}

// Report Types
export interface PipelineStage {
  status: string;
  count: number;
  value: number;
  percentage: number;
}

export interface PipelineReport {
  totalPipelineValue: number;
  totalProposals: number;
  stages: PipelineStage[];
  generatedAt: string;
}

export interface ConversionReport {
  totalProposals: number;
  wonCount: number;
  lostCount: number;
  pendingCount: number;
  cancelledCount: number;
  wonValue: number;
  lostValue: number;
  conversionRate: number;
  winRate: number;
  averageProposalValue: number;
  averageWonValue: number;
  generatedAt: string;
}

export interface StatusBreakdown {
  status: string;
  count: number;
  value: number;
  averageValue: number;
  percentageOfTotal: number;
}

export interface ByStatusReport {
  statuses: StatusBreakdown[];
  totalCount: number;
  totalValue: number;
  generatedAt: string;
}

export interface CompanyProposalSummary {
  companyId: string;
  companyName: string;
  totalProposals: number;
  wonCount: number;
  lostCount: number;
  totalValue: number;
  wonValue: number;
  conversionRate: number;
}

export interface ByCompanyReport {
  companies: CompanyProposalSummary[];
  totalCompanies: number;
  generatedAt: string;
}

export interface WinLossReason {
  reason: string;
  count: number;
  value: number;
  percentage: number;
}

export interface WinLossAnalysisReport {
  winReasons: WinLossReason[];
  lossReasons: WinLossReason[];
  averageTimeToWinDays: number;
  averageTimeToLossDays: number;
  generatedAt: string;
}

export interface MonthlyDataPoint {
  year: number;
  month: number;
  monthName: string;
  proposalsCreated: number;
  proposalsWon: number;
  proposalsLost: number;
  valueCreated: number;
  valueWon: number;
  valueLost: number;
  conversionRate: number;
}

export interface MonthlyTrendsReport {
  dataPoints: MonthlyDataPoint[];
  generatedAt: string;
}

// Report API Functions
async function getPipelineReport(fromDate?: string, toDate?: string): Promise<PipelineReport> {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  const queryString = params.toString();
  const url = queryString ? `/proposals/reports/pipeline?${queryString}` : '/proposals/reports/pipeline';
  const response = await apiClient.get<ApiResponse<PipelineReport>>(url);
  return response.data.data ?? { totalPipelineValue: 0, totalProposals: 0, stages: [], generatedAt: '' };
}

async function getConversionReport(fromDate?: string, toDate?: string): Promise<ConversionReport> {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  const queryString = params.toString();
  const url = queryString ? `/proposals/reports/conversion?${queryString}` : '/proposals/reports/conversion';
  const response = await apiClient.get<ApiResponse<ConversionReport>>(url);
  return response.data.data ?? { totalProposals: 0, wonCount: 0, lostCount: 0, pendingCount: 0, cancelledCount: 0, wonValue: 0, lostValue: 0, conversionRate: 0, winRate: 0, averageProposalValue: 0, averageWonValue: 0, generatedAt: '' };
}

async function getByStatusReport(fromDate?: string, toDate?: string): Promise<ByStatusReport> {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  const queryString = params.toString();
  const url = queryString ? `/proposals/reports/by-status?${queryString}` : '/proposals/reports/by-status';
  const response = await apiClient.get<ApiResponse<ByStatusReport>>(url);
  return response.data.data ?? { statuses: [], totalCount: 0, totalValue: 0, generatedAt: '' };
}

async function getByCompanyReport(fromDate?: string, toDate?: string, top: number = 10): Promise<ByCompanyReport> {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  params.append('top', String(top));
  const queryString = params.toString();
  const url = `/proposals/reports/by-company?${queryString}`;
  const response = await apiClient.get<ApiResponse<ByCompanyReport>>(url);
  return response.data.data ?? { companies: [], totalCompanies: 0, generatedAt: '' };
}

async function getWinLossAnalysis(fromDate?: string, toDate?: string): Promise<WinLossAnalysisReport> {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  const queryString = params.toString();
  const url = queryString ? `/proposals/reports/win-loss?${queryString}` : '/proposals/reports/win-loss';
  const response = await apiClient.get<ApiResponse<WinLossAnalysisReport>>(url);
  return response.data.data ?? { winReasons: [], lossReasons: [], averageTimeToWinDays: 0, averageTimeToLossDays: 0, generatedAt: '' };
}

async function getMonthlyTrends(months: number = 12): Promise<MonthlyTrendsReport> {
  const response = await apiClient.get<ApiResponse<MonthlyTrendsReport>>(`/proposals/reports/monthly-trends?months=${months}`);
  return response.data.data ?? { dataPoints: [], generatedAt: '' };
}

// Report Query Hooks
export function usePipelineReport(fromDate?: string, toDate?: string) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'reports', 'pipeline', fromDate, toDate],
    queryFn: () => getPipelineReport(fromDate, toDate),
  });
}

export function useConversionReport(fromDate?: string, toDate?: string) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'reports', 'conversion', fromDate, toDate],
    queryFn: () => getConversionReport(fromDate, toDate),
  });
}

export function useByStatusReport(fromDate?: string, toDate?: string) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'reports', 'by-status', fromDate, toDate],
    queryFn: () => getByStatusReport(fromDate, toDate),
  });
}

export function useByCompanyReport(fromDate?: string, toDate?: string, top: number = 10) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'reports', 'by-company', fromDate, toDate, top],
    queryFn: () => getByCompanyReport(fromDate, toDate, top),
  });
}

export function useWinLossAnalysis(fromDate?: string, toDate?: string) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'reports', 'win-loss', fromDate, toDate],
    queryFn: () => getWinLossAnalysis(fromDate, toDate),
  });
}

export function useMonthlyTrendsReport(months: number = 12) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, 'reports', 'monthly-trends', months],
    queryFn: () => getMonthlyTrends(months),
  });
}

// Stock Order Conversion Types
export type ConversionMode = 'AllItems' | 'SelectedSections' | 'SelectedItems';

export interface ConvertToStockOrderDto {
  proposalId: string;
  siteId: string;
  sourceLocationId: string;
  requiredDate?: string;
  notes?: string;
  mode: ConversionMode;
  selectedSectionIds?: string[];
  selectedLineItemIds?: string[];
}

export interface ConversionPreviewItem {
  productId?: string;
  productCode?: string;
  description: string;
  quantity: number;
  availableStock: number;
  hasSufficientStock: boolean;
  isAdHocItem: boolean;
  unitPrice: number;
  lineTotal: number;
}

export interface ConversionPreview {
  totalItems: number;
  totalQuantity: number;
  totalValue: number;
  hasStockWarnings: boolean;
  hasAdHocItems: boolean;
  items: ConversionPreviewItem[];
}

export interface CreatedStockOrder {
  stockOrderId: string;
  orderNumber: string;
  itemCount: number;
  totalValue: number;
}

export interface ConversionResult {
  success: boolean;
  createdOrders: CreatedStockOrder[];
  warnings: string[];
  errorMessage?: string;
}

// Stock Order Conversion API Functions
async function canConvertProposal(proposalId: string): Promise<boolean> {
  const response = await apiClient.get<boolean>(`/proposals/${proposalId}/can-convert`);
  return response.data;
}

async function previewConversion(
  proposalId: string,
  data: Omit<ConvertToStockOrderDto, 'proposalId'>
): Promise<ConversionPreview> {
  const response = await apiClient.post<ConversionPreview>(
    `/proposals/${proposalId}/preview-conversion`,
    { ...data, proposalId }
  );
  return response.data;
}

async function convertToStockOrders(
  proposalId: string,
  data: Omit<ConvertToStockOrderDto, 'proposalId'>
): Promise<ConversionResult> {
  const response = await apiClient.post<ConversionResult>(
    `/proposals/${proposalId}/convert-to-orders`,
    { ...data, proposalId }
  );
  return response.data;
}

// Stock Order Conversion Hooks
export function useCanConvertProposal(proposalId: string) {
  return useQuery({
    queryKey: [...PROPOSALS_KEY, proposalId, 'can-convert'],
    queryFn: () => canConvertProposal(proposalId),
    enabled: !!proposalId,
  });
}

export function usePreviewConversion() {
  return useMutation({
    mutationFn: ({
      proposalId,
      data,
    }: {
      proposalId: string;
      data: Omit<ConvertToStockOrderDto, 'proposalId'>;
    }) => previewConversion(proposalId, data),
  });
}

export function useConvertToStockOrders() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      proposalId,
      data,
    }: {
      proposalId: string;
      data: Omit<ConvertToStockOrderDto, 'proposalId'>;
    }) => convertToStockOrders(proposalId, data),
    onSuccess: (_, { proposalId }) => {
      queryClient.invalidateQueries({ queryKey: [...PROPOSALS_KEY, proposalId] });
      queryClient.invalidateQueries({ queryKey: ['stock-orders'] });
    },
  });
}
