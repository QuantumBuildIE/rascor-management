import { useQuery } from '@tanstack/react-query';
import { getMyCertificates, downloadCertificate } from './certificates';

// ============================================
// Query Keys
// ============================================

export const MY_CERTIFICATES_KEY = ['my-certificates'];

// ============================================
// Certificate Query Hooks
// ============================================

export function useMyCertificates() {
  return useQuery({
    queryKey: MY_CERTIFICATES_KEY,
    queryFn: getMyCertificates,
  });
}

// ============================================
// Download Helper
// ============================================

export async function handleDownloadCertificate(id: string, certificateNumber: string) {
  const blob = await downloadCertificate(id);
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Certificate-${certificateNumber}.pdf`;
  document.body.appendChild(a);
  a.click();
  window.URL.revokeObjectURL(url);
  document.body.removeChild(a);
}
