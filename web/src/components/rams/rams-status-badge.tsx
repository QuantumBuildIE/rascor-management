"use client";

import { Badge } from "@/components/ui/badge";
import type { RamsStatus } from "@/types/rams";
import { cn } from "@/lib/utils";

interface RamsStatusBadgeProps {
  status: RamsStatus;
  className?: string;
}

const statusConfig: Record<
  RamsStatus,
  { label: string; variant: "default" | "secondary" | "destructive" | "outline"; className?: string }
> = {
  Draft: {
    label: "Draft",
    variant: "secondary",
    className: "bg-gray-100 text-gray-800 hover:bg-gray-100",
  },
  PendingReview: {
    label: "Pending Review",
    variant: "secondary",
    className: "bg-yellow-100 text-yellow-800 hover:bg-yellow-100",
  },
  Approved: {
    label: "Approved",
    variant: "secondary",
    className: "bg-green-100 text-green-800 hover:bg-green-100",
  },
  Rejected: {
    label: "Rejected",
    variant: "destructive",
    className: "bg-red-100 text-red-800 hover:bg-red-100",
  },
  Archived: {
    label: "Archived",
    variant: "secondary",
    className: "bg-purple-100 text-purple-800 hover:bg-purple-100",
  },
};

export function RamsStatusBadge({ status, className }: RamsStatusBadgeProps) {
  const config = statusConfig[status] || statusConfig.Draft;

  return (
    <Badge
      variant={config.variant}
      className={cn(config.className, className)}
    >
      {config.label}
    </Badge>
  );
}
