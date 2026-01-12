import { cn } from "@/lib/utils";
import {
  type ProposalStatus,
  getStatusColor,
  getStatusDisplayName,
} from "@/lib/api/proposals";

interface ProposalStatusBadgeProps {
  status: ProposalStatus;
  className?: string;
}

export function ProposalStatusBadge({ status, className }: ProposalStatusBadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
        getStatusColor(status),
        className
      )}
    >
      {getStatusDisplayName(status)}
    </span>
  );
}
