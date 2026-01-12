export type RamsStatus =
  | 'Draft'
  | 'PendingReview'
  | 'Approved'
  | 'Rejected'
  | 'Archived';

export type ProjectType =
  | 'RemedialInjection'
  | 'RascotankNewBuild'
  | 'CarParkCoating'
  | 'GroundGasBarrier'
  | 'Other';

export type RiskLevel = 'Low' | 'Medium' | 'High';

export interface RamsDocumentListItem {
  id: string;
  projectName: string;
  projectReference: string;
  projectType: ProjectType;
  projectTypeDisplay: string;
  clientName?: string;
  status: RamsStatus;
  statusDisplay: string;
  proposedStartDate?: string;
  riskAssessmentCount: number;
  methodStepCount: number;
  createdAt: string;
}

export interface RamsDocument {
  id: string;
  projectName: string;
  projectReference: string;
  projectType: ProjectType;
  projectTypeDisplay: string;
  clientName?: string;
  siteAddress?: string;
  areaOfActivity?: string;
  proposedStartDate?: string;
  proposedEndDate?: string;
  safetyOfficerId?: string;
  safetyOfficerName?: string;
  status: RamsStatus;
  statusDisplay: string;
  dateApproved?: string;
  approvedById?: string;
  approvedByName?: string;
  approvalComments?: string;
  methodStatementBody?: string;
  generatedPdfUrl?: string;
  proposalId?: string;
  siteId?: string;
  siteName?: string;
  riskAssessmentCount: number;
  methodStepCount: number;
  createdAt: string;
  modifiedAt?: string;
}

export interface CreateRamsDocumentDto {
  projectName: string;
  projectReference: string;
  projectType: ProjectType;
  clientName?: string;
  siteAddress?: string;
  areaOfActivity?: string;
  proposedStartDate?: string;
  proposedEndDate?: string;
  safetyOfficerId?: string;
  methodStatementBody?: string;
  proposalId?: string;
  siteId?: string;
}

export interface UpdateRamsDocumentDto {
  projectName: string;
  projectReference: string;
  projectType: ProjectType;
  clientName?: string;
  siteAddress?: string;
  areaOfActivity?: string;
  proposedStartDate?: string;
  proposedEndDate?: string;
  safetyOfficerId?: string;
  methodStatementBody?: string;
  siteId?: string;
}

export interface GetRamsParams {
  search?: string;
  status?: RamsStatus;
  projectType?: ProjectType;
  siteId?: string;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
}

export const ProjectTypeLabels: Record<ProjectType, string> = {
  RemedialInjection: 'Remedial Injection',
  RascotankNewBuild: 'RASCOtank New Build',
  CarParkCoating: 'Car Park Coating',
  GroundGasBarrier: 'Ground Gas Barrier',
  Other: 'Other',
};

export const RamsStatusLabels: Record<RamsStatus, string> = {
  Draft: 'Draft',
  PendingReview: 'Pending Review',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Archived: 'Archived',
};

// Risk Assessment types
export interface RiskAssessmentDto {
  id: string;
  ramsDocumentId: string;
  taskActivity: string;
  locationArea?: string;
  hazardIdentified: string;
  whoAtRisk?: string;
  initialLikelihood: number;
  initialSeverity: number;
  initialRiskRating: number;
  initialRiskLevel: RiskLevel;
  initialRiskLevelDisplay: string;
  controlMeasures?: string;
  relevantLegislation?: string;
  referenceSops?: string;
  residualLikelihood: number;
  residualSeverity: number;
  residualRiskRating: number;
  residualRiskLevel: RiskLevel;
  residualRiskLevelDisplay: string;
  isAiGenerated: boolean;
  aiGeneratedAt?: string;
  sortOrder: number;
  createdAt: string;
  modifiedAt?: string;
}

export interface CreateRiskAssessmentDto {
  taskActivity: string;
  locationArea?: string;
  hazardIdentified: string;
  whoAtRisk?: string;
  initialLikelihood: number;
  initialSeverity: number;
  controlMeasures?: string;
  relevantLegislation?: string;
  referenceSops?: string;
  residualLikelihood: number;
  residualSeverity: number;
  sortOrder?: number;
}

export interface UpdateRiskAssessmentDto {
  taskActivity: string;
  locationArea?: string;
  hazardIdentified: string;
  whoAtRisk?: string;
  initialLikelihood: number;
  initialSeverity: number;
  controlMeasures?: string;
  relevantLegislation?: string;
  referenceSops?: string;
  residualLikelihood: number;
  residualSeverity: number;
  sortOrder?: number;
}

// Method Step types
export interface MethodStepDto {
  id: string;
  ramsDocumentId: string;
  stepNumber: number;
  stepTitle: string;
  detailedProcedure?: string;
  linkedRiskAssessmentId?: string;
  linkedRiskAssessmentTask?: string;
  requiredPermits?: string;
  requiresSignoff: boolean;
  signoffUrl?: string;
  createdAt: string;
  modifiedAt?: string;
}

export interface CreateMethodStepDto {
  stepNumber?: number;
  stepTitle: string;
  detailedProcedure?: string;
  linkedRiskAssessmentId?: string;
  requiredPermits?: string;
  requiresSignoff: boolean;
}

export interface UpdateMethodStepDto {
  stepNumber?: number;
  stepTitle: string;
  detailedProcedure?: string;
  linkedRiskAssessmentId?: string;
  requiredPermits?: string;
  requiresSignoff: boolean;
}

// Risk level color mapping for UI
export const RiskLevelColors: Record<RiskLevel, { bg: string; text: string; badgeVariant: 'default' | 'secondary' | 'destructive' | 'outline' }> = {
  Low: { bg: 'bg-green-100', text: 'text-green-800', badgeVariant: 'secondary' },
  Medium: { bg: 'bg-yellow-100', text: 'text-yellow-800', badgeVariant: 'secondary' },
  High: { bg: 'bg-red-100', text: 'text-red-800', badgeVariant: 'destructive' },
};

// Library types
export enum HazardCategory {
  Physical = 0,
  Chemical = 1,
  Biological = 2,
  Ergonomic = 3,
  Psychological = 4,
  Environmental = 5,
  Electrical = 6,
  Fire = 7,
  WorkingAtHeight = 8,
  ManualHandling = 9,
  MachineryEquipment = 10,
  Other = 99
}

export enum ControlHierarchy {
  Elimination = 0,
  Substitution = 1,
  Engineering = 2,
  Administrative = 3,
  PPE = 4
}

export const HazardCategoryLabels: Record<HazardCategory, string> = {
  [HazardCategory.Physical]: 'Physical',
  [HazardCategory.Chemical]: 'Chemical',
  [HazardCategory.Biological]: 'Biological',
  [HazardCategory.Ergonomic]: 'Ergonomic',
  [HazardCategory.Psychological]: 'Psychological',
  [HazardCategory.Environmental]: 'Environmental',
  [HazardCategory.Electrical]: 'Electrical',
  [HazardCategory.Fire]: 'Fire',
  [HazardCategory.WorkingAtHeight]: 'Working at Height',
  [HazardCategory.ManualHandling]: 'Manual Handling',
  [HazardCategory.MachineryEquipment]: 'Machinery/Equipment',
  [HazardCategory.Other]: 'Other'
};

export const ControlHierarchyLabels: Record<ControlHierarchy, string> = {
  [ControlHierarchy.Elimination]: 'Elimination',
  [ControlHierarchy.Substitution]: 'Substitution',
  [ControlHierarchy.Engineering]: 'Engineering Controls',
  [ControlHierarchy.Administrative]: 'Administrative Controls',
  [ControlHierarchy.PPE]: 'PPE'
};

export interface HazardLibraryDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  category: HazardCategory;
  categoryDisplay: string;
  keywords?: string;
  defaultLikelihood: number;
  defaultSeverity: number;
  typicalWhoAtRisk?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface CreateHazardLibraryDto {
  code: string;
  name: string;
  description?: string;
  category: HazardCategory;
  keywords?: string;
  defaultLikelihood: number;
  defaultSeverity: number;
  typicalWhoAtRisk?: string;
  sortOrder: number;
}

export interface UpdateHazardLibraryDto extends CreateHazardLibraryDto {
  isActive: boolean;
}

export interface ControlMeasureLibraryDto {
  id: string;
  code: string;
  name: string;
  description: string;
  hierarchy: ControlHierarchy;
  hierarchyDisplay: string;
  applicableToCategory?: HazardCategory;
  applicableToCategoryDisplay?: string;
  keywords?: string;
  typicalLikelihoodReduction: number;
  typicalSeverityReduction: number;
  isActive: boolean;
  sortOrder: number;
}

export interface CreateControlMeasureLibraryDto {
  code: string;
  name: string;
  description: string;
  hierarchy: ControlHierarchy;
  applicableToCategory?: HazardCategory;
  keywords?: string;
  typicalLikelihoodReduction: number;
  typicalSeverityReduction: number;
  sortOrder: number;
}

export interface UpdateControlMeasureLibraryDto extends CreateControlMeasureLibraryDto {
  isActive: boolean;
}

export interface LegislationReferenceDto {
  id: string;
  code: string;
  name: string;
  shortName?: string;
  description?: string;
  jurisdiction?: string;
  keywords?: string;
  documentUrl?: string;
  applicableCategories?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface CreateLegislationReferenceDto {
  code: string;
  name: string;
  shortName?: string;
  description?: string;
  jurisdiction?: string;
  keywords?: string;
  documentUrl?: string;
  applicableCategories?: string;
  sortOrder: number;
}

export interface UpdateLegislationReferenceDto extends CreateLegislationReferenceDto {
  isActive: boolean;
}

export interface SopReferenceDto {
  id: string;
  sopId: string;
  topic: string;
  description?: string;
  taskKeywords?: string;
  policySnippet?: string;
  procedureDetails?: string;
  applicableLegislation?: string;
  documentUrl?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface CreateSopReferenceDto {
  sopId: string;
  topic: string;
  description?: string;
  taskKeywords?: string;
  policySnippet?: string;
  procedureDetails?: string;
  applicableLegislation?: string;
  documentUrl?: string;
  sortOrder: number;
}

export interface UpdateSopReferenceDto extends CreateSopReferenceDto {
  isActive: boolean;
}

// AI Suggestion types
export interface ControlMeasureSuggestionRequest {
  taskActivity: string;
  hazardIdentified: string;
  locationArea?: string;
  whoAtRisk?: string;
  initialLikelihood?: number;
  initialSeverity?: number;
  projectType?: string;
  additionalContext?: string;
}

export interface ControlMeasureSuggestionResponse {
  success: boolean;
  errorMessage?: string;
  matchedHazards: HazardMatch[];
  suggestedControls: ControlMatch[];
  relevantLegislation: LegislationMatch[];
  relevantSops: SopMatch[];
  aiGeneratedControlMeasures?: string;
  aiGeneratedLegislation?: string;
  suggestedResidualLikelihood?: number;
  suggestedResidualSeverity?: number;
  auditLogId?: string;
  usedAi: boolean;
}

export interface HazardMatch {
  id: string;
  code: string;
  name: string;
  description?: string;
  category: string;
  defaultLikelihood: number;
  defaultSeverity: number;
  matchScore: number;
}

export interface ControlMatch {
  id: string;
  code: string;
  name: string;
  description: string;
  hierarchy: string;
  likelihoodReduction: number;
  severityReduction: number;
  matchScore: number;
}

export interface LegislationMatch {
  id: string;
  code: string;
  name: string;
  shortName?: string;
  matchScore: number;
}

export interface SopMatch {
  id: string;
  sopId: string;
  topic: string;
  policySnippet?: string;
  matchScore: number;
}

export interface AcceptSuggestionDto {
  auditLogId: string;
  accepted: boolean;
}

// Dashboard types
export interface RamsDashboard {
  summary: RamsSummaryStats;
  statusCounts: RamsStatusCount[];
  projectTypeCounts: RamsProjectTypeCount[];
  riskDistribution: RamsRiskDistribution[];
  monthlyTrends: RamsMonthlyTrend[];
  pendingApprovals: RamsPendingApproval[];
  overdueDocuments: RamsOverdueDocument[];
  approvalMetrics: RamsApprovalMetrics;
}

export interface RamsSummaryStats {
  totalDocuments: number;
  draftDocuments: number;
  pendingReviewDocuments: number;
  approvedDocuments: number;
  rejectedDocuments: number;
  archivedDocuments: number;
  totalRiskAssessments: number;
  highRiskCount: number;
  mediumRiskCount: number;
  lowRiskCount: number;
  documentsThisMonth: number;
  approvalsThisMonth: number;
}

export interface RamsStatusCount {
  status: string;
  count: number;
  percentage: number;
}

export interface RamsProjectTypeCount {
  projectType: string;
  count: number;
  percentage: number;
}

export interface RamsRiskDistribution {
  riskLevel: string;
  initialCount: number;
  residualCount: number;
}

export interface RamsMonthlyTrend {
  month: string;
  year: number;
  created: number;
  approved: number;
  rejected: number;
}

export interface RamsPendingApproval {
  id: string;
  projectReference: string;
  projectName: string;
  projectType: string;
  clientName?: string;
  submittedAt: string;
  daysPending: number;
  riskAssessmentCount: number;
  highRiskCount: number;
  submittedByName?: string;
}

export interface RamsOverdueDocument {
  id: string;
  projectReference: string;
  projectName: string;
  status: string;
  proposedEndDate?: string;
  daysOverdue: number;
  safetyOfficerName?: string;
}

export interface RamsApprovalMetrics {
  averageApprovalDays: number;
  averageRejectionRate: number;
  fastestApprovalDays: number;
  slowestApprovalDays: number;
  totalApprovedLast30Days: number;
  totalRejectedLast30Days: number;
}

export interface RamsExportRequest {
  dateFrom?: string;
  dateTo?: string;
  status?: string;
  projectType?: string;
  includeRiskAssessments?: boolean;
  includeMethodSteps?: boolean;
}
