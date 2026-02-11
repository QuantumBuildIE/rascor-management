import { apiClient } from '@/lib/api/client';
import type { ApiResponse } from '@/types/auth';

// ============================================
// Certificate Types
// ============================================

export interface CertificateDto {
  id: string;
  certificateNumber: string;
  certificateType: string; // "Talk" or "Course"
  trainingTitle: string;
  includedTalks?: string[];
  issuedAt: string;
  expiresAt?: string;
  isExpired: boolean;
  isExpiringSoon: boolean;
  isRefresher: boolean;
}

// ============================================
// My Certificates (Employee Portal)
// ============================================

export async function getMyCertificates(): Promise<CertificateDto[]> {
  const response = await apiClient.get<ApiResponse<CertificateDto[]>>(
    '/my/toolbox-talks/certificates'
  );

  return response.data.data ?? [];
}

export async function downloadCertificate(id: string): Promise<Blob> {
  const response = await apiClient.get(
    `/my/toolbox-talks/certificates/${id}/download`,
    {
      responseType: 'blob',
    }
  );
  return response.data;
}
