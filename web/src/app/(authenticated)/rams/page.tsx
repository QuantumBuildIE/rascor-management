"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DataTable,
  type Column,
  type SortDirection,
} from "@/components/shared/data-table";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { RamsStatusBadge } from "@/components/rams";
import {
  useRamsDocuments,
  useDeleteRamsDocument,
  type RamsDocumentListItem,
} from "@/lib/api/rams";
import { usePermission } from "@/lib/auth/use-auth";
import { toast } from "sonner";
import type { RamsStatus, ProjectType } from "@/types/rams";
import { ProjectTypeLabels } from "@/types/rams";

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = React.useState(value);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}

const statusOptions: { value: string; label: string }[] = [
  { value: "all", label: "All Statuses" },
  { value: "Draft", label: "Draft" },
  { value: "PendingReview", label: "Pending Review" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
  { value: "Archived", label: "Archived" },
];

const projectTypeOptions: { value: string; label: string }[] = [
  { value: "all", label: "All Types" },
  ...Object.entries(ProjectTypeLabels).map(([value, label]) => ({
    value,
    label,
  })),
];

export default function RamsDocumentsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const pageNumber = Number(searchParams.get("page")) || 1;
  const pageSize = Number(searchParams.get("pageSize")) || 20;
  const sortColumn = searchParams.get("sortColumn") || undefined;
  const sortDirection =
    (searchParams.get("sortDirection") as SortDirection) || undefined;
  const searchParam = searchParams.get("search") || "";
  const statusFilter = searchParams.get("status") || "all";
  const typeFilter = searchParams.get("projectType") || "all";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [ramsToDelete, setRamsToDelete] = React.useState<RamsDocumentListItem | null>(null);

  // Permissions
  const canCreate = usePermission("Rams.Create");

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null, page: 1 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  // Fetch data
  const { data: ramsData, isLoading, error } = useRamsDocuments({
    pageNumber,
    pageSize,
    sortColumn,
    sortDirection: sortDirection as "asc" | "desc" | undefined,
    search: searchParam || undefined,
    status: statusFilter !== "all" ? (statusFilter as RamsStatus) : undefined,
    projectType: typeFilter !== "all" ? (typeFilter as ProjectType) : undefined,
  });

  const deleteRams = useDeleteRamsDocument();

  const updateUrlParams = (
    updates: Record<string, string | number | null | undefined>
  ) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value === null || value === undefined || value === "") {
        params.delete(key);
      } else {
        params.set(key, String(value));
      }
    });

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handleSort = (column: string, direction: SortDirection) => {
    updateUrlParams({ sortColumn: column, sortDirection: direction });
  };

  const handlePageChange = (page: number) => {
    updateUrlParams({ page });
  };

  const handlePageSizeChange = (size: number) => {
    updateUrlParams({ pageSize: size, page: 1 });
  };

  const handleStatusChange = (status: string) => {
    updateUrlParams({ status: status === "all" ? null : status, page: 1 });
  };

  const handleTypeChange = (projectType: string) => {
    updateUrlParams({ projectType: projectType === "all" ? null : projectType, page: 1 });
  };

  const handleDeleteClick = (rams: RamsDocumentListItem) => {
    setRamsToDelete(rams);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!ramsToDelete) return;

    try {
      await deleteRams.mutateAsync(ramsToDelete.id);
      toast.success("RAMS document deleted successfully");
      setDeleteDialogOpen(false);
      setRamsToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to delete RAMS document", { description: message });
    }
  };

  const renderActions = (rams: RamsDocumentListItem) => {
    return (
      <div className="flex items-center justify-end gap-1">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/rams/${rams.id}`}>View</Link>
        </Button>
        {(rams.status === "Draft" || rams.status === "Rejected") && (
          <>
            <Button variant="ghost" size="sm" asChild>
              <Link href={`/rams/${rams.id}/edit`}>Edit</Link>
            </Button>
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive hover:text-destructive"
              onClick={(e) => {
                e.stopPropagation();
                handleDeleteClick(rams);
              }}
            >
              Delete
            </Button>
          </>
        )}
      </div>
    );
  };

  const columns: Column<RamsDocumentListItem>[] = [
    {
      key: "projectReference",
      header: "Reference",
      sortable: true,
      className: "font-medium",
    },
    {
      key: "projectName",
      header: "Project Name",
      sortable: true,
    },
    {
      key: "projectTypeDisplay",
      header: "Type",
      sortable: true,
      className: "hidden md:table-cell",
      headerClassName: "hidden md:table-cell",
      render: (rams) => (
        <Badge variant="secondary">{rams.projectTypeDisplay}</Badge>
      ),
    },
    {
      key: "clientName",
      header: "Client",
      sortable: true,
      className: "hidden lg:table-cell",
      headerClassName: "hidden lg:table-cell",
      render: (rams) => rams.clientName || <span className="text-muted-foreground">-</span>,
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      render: (rams) => <RamsStatusBadge status={rams.status} />,
    },
    {
      key: "proposedStartDate",
      header: "Start Date",
      sortable: true,
      className: "hidden xl:table-cell",
      headerClassName: "hidden xl:table-cell",
      render: (rams) =>
        rams.proposedStartDate ? (
          new Date(rams.proposedStartDate).toLocaleDateString()
        ) : (
          <span className="text-muted-foreground">-</span>
        ),
    },
    {
      key: "riskAssessmentCount",
      header: "Risks",
      headerClassName: "text-center hidden md:table-cell",
      className: "text-center hidden md:table-cell",
      render: (rams) => (
        <Badge variant="outline">{rams.riskAssessmentCount}</Badge>
      ),
    },
    {
      key: "methodStepCount",
      header: "Steps",
      headerClassName: "text-center hidden md:table-cell",
      className: "text-center hidden md:table-cell",
      render: (rams) => (
        <Badge variant="outline">{rams.methodStepCount}</Badge>
      ),
    },
    {
      key: "createdAt",
      header: "Created",
      sortable: true,
      className: "hidden lg:table-cell",
      headerClassName: "hidden lg:table-cell",
      render: (rams) => new Date(rams.createdAt).toLocaleDateString(),
    },
    {
      key: "actions",
      header: "Actions",
      headerClassName: "text-right",
      className: "text-right",
      render: renderActions,
    },
  ];

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">RAMS Documents</h1>
            <p className="text-muted-foreground">
              Risk Assessment and Method Statement documents
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load RAMS documents. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">RAMS Documents</h1>
          <p className="text-muted-foreground">
            Risk Assessment and Method Statement documents
          </p>
        </div>
        {canCreate && (
          <Button asChild className="w-full sm:w-auto">
            <Link href="/rams/new">New RAMS Document</Link>
          </Button>
        )}
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 sm:max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search RAMS..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-9"
          />
        </div>

        <Select value={statusFilter} onValueChange={handleStatusChange}>
          <SelectTrigger className="w-full sm:w-[180px]">
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            {statusOptions.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={typeFilter} onValueChange={handleTypeChange}>
          <SelectTrigger className="w-full sm:w-[200px]">
            <SelectValue placeholder="Project Type" />
          </SelectTrigger>
          <SelectContent>
            {projectTypeOptions.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Data Table */}
      <DataTable
        columns={columns}
        data={ramsData?.items ?? []}
        isLoading={isLoading}
        emptyMessage="No RAMS documents found"
        keyExtractor={(rams) => rams.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        onRowClick={(rams) => router.push(`/rams/${rams.id}`)}
        pagination={{
          pageNumber: ramsData?.pageNumber ?? 1,
          pageSize: ramsData?.pageSize ?? 20,
          totalCount: ramsData?.totalCount ?? 0,
          totalPages: ramsData?.totalPages ?? 0,
        }}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete RAMS Document"
        description={`Are you sure you want to delete RAMS document "${ramsToDelete?.projectReference}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteRams.isPending}
      />
    </div>
  );
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
      />
    </svg>
  );
}
