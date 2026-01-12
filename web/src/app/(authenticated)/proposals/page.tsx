"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
import { ProposalStatusBadge } from "@/components/proposals";
import {
  useProposals,
  useProposalSummary,
  useDeleteProposal,
  type ProposalListItem,
  type ProposalStatus,
} from "@/lib/api/proposals";
import { useAllCompanies } from "@/lib/api/admin/use-companies";
import { usePermission } from "@/lib/auth/use-auth";
import { toast } from "sonner";
import { FileText, TrendingUp, Trophy, Percent, BarChart3 } from "lucide-react";
import { cn } from "@/lib/utils";

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
  { value: "Submitted", label: "Submitted" },
  { value: "UnderReview", label: "Under Review" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
  { value: "Won", label: "Won" },
  { value: "Lost", label: "Lost" },
  { value: "Expired", label: "Expired" },
  { value: "Cancelled", label: "Cancelled" },
];

function formatCurrency(value: number, currency: string = "EUR"): string {
  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: currency,
    minimumFractionDigits: 2,
  }).format(value);
}

function isExpired(validUntilDate?: string): boolean {
  if (!validUntilDate) return false;
  return new Date(validUntilDate) < new Date();
}

export default function ProposalsPage() {
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
  const companyFilter = searchParams.get("companyId") || "all";

  const [searchInput, setSearchInput] = React.useState(searchParam);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [proposalToDelete, setProposalToDelete] = React.useState<ProposalListItem | null>(null);

  // Permissions
  const canCreate = usePermission("Proposals.Create");
  const canViewCostings = usePermission("Proposals.ViewCostings");

  // Update URL when search changes (debounced)
  React.useEffect(() => {
    if (debouncedSearch !== searchParam) {
      updateUrlParams({ search: debouncedSearch || null, page: 1 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  // Fetch data
  const { data: proposalsData, isLoading, error } = useProposals({
    pageNumber,
    pageSize,
    sortColumn,
    sortDirection: sortDirection as "asc" | "desc" | undefined,
    search: searchParam || undefined,
    status: statusFilter !== "all" ? statusFilter : undefined,
    companyId: companyFilter !== "all" ? companyFilter : undefined,
  });

  const { data: summary } = useProposalSummary();
  const { data: companies } = useAllCompanies();
  const deleteProposal = useDeleteProposal();

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

  const handleCompanyChange = (companyId: string) => {
    updateUrlParams({ companyId: companyId === "all" ? null : companyId, page: 1 });
  };

  const handleDeleteClick = (proposal: ProposalListItem) => {
    setProposalToDelete(proposal);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!proposalToDelete) return;

    try {
      await deleteProposal.mutateAsync(proposalToDelete.id);
      toast.success("Proposal deleted successfully");
      setDeleteDialogOpen(false);
      setProposalToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to delete proposal", { description: message });
    }
  };

  const renderActions = (proposal: ProposalListItem) => {
    return (
      <div className="flex items-center justify-end gap-1">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/proposals/${proposal.id}`}>View</Link>
        </Button>
        {proposal.status === "Draft" && (
          <>
            <Button variant="ghost" size="sm" asChild>
              <Link href={`/proposals/${proposal.id}/edit`}>Edit</Link>
            </Button>
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive hover:text-destructive"
              onClick={(e) => {
                e.stopPropagation();
                handleDeleteClick(proposal);
              }}
            >
              Delete
            </Button>
          </>
        )}
      </div>
    );
  };

  const columns: Column<ProposalListItem>[] = [
    {
      key: "proposalNumber",
      header: "Proposal #",
      sortable: true,
      className: "font-medium",
      render: (proposal) => (
        <span>
          {proposal.proposalNumber}
          {proposal.version > 1 && (
            <span className="ml-1 text-muted-foreground">v{proposal.version}</span>
          )}
        </span>
      ),
    },
    {
      key: "projectName",
      header: "Project Name",
      sortable: true,
    },
    {
      key: "companyName",
      header: "Company",
      sortable: true,
      className: "hidden md:table-cell",
      headerClassName: "hidden md:table-cell",
    },
    {
      key: "proposalDate",
      header: "Date",
      sortable: true,
      className: "hidden lg:table-cell",
      headerClassName: "hidden lg:table-cell",
      render: (proposal) => new Date(proposal.proposalDate).toLocaleDateString(),
    },
    {
      key: "validUntilDate",
      header: "Valid Until",
      sortable: true,
      className: "hidden xl:table-cell",
      headerClassName: "hidden xl:table-cell",
      render: (proposal) => {
        if (!proposal.validUntilDate) {
          return <span className="text-muted-foreground">-</span>;
        }
        const expired = isExpired(proposal.validUntilDate);
        return (
          <span className={cn(expired && "text-destructive")}>
            {new Date(proposal.validUntilDate).toLocaleDateString()}
            {expired && (
              <span className="ml-1 inline-flex items-center rounded-full bg-red-100 px-1.5 py-0.5 text-xs font-medium text-red-800">
                Expired
              </span>
            )}
          </span>
        );
      },
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      render: (proposal) => <ProposalStatusBadge status={proposal.status} />,
    },
    {
      key: "grandTotal",
      header: "Total",
      sortable: true,
      headerClassName: "text-right",
      className: "text-right",
      render: (proposal) => formatCurrency(proposal.grandTotal, proposal.currency),
    },
    ...(canViewCostings
      ? [
          {
            key: "marginPercent",
            header: "Margin %",
            sortable: true,
            headerClassName: "text-right hidden lg:table-cell",
            className: "text-right hidden lg:table-cell",
            render: (proposal: ProposalListItem) => {
              if (proposal.marginPercent === undefined || proposal.marginPercent === null) {
                return <span className="text-muted-foreground">-</span>;
              }
              const margin = proposal.marginPercent;
              let colorClass = "text-red-600";
              if (margin >= 20) {
                colorClass = "text-green-600";
              } else if (margin >= 10) {
                colorClass = "text-yellow-600";
              }
              return (
                <span className={cn("font-medium", colorClass)}>
                  {margin.toFixed(1)}%
                </span>
              );
            },
          } as Column<ProposalListItem>,
        ]
      : []),
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
            <h1 className="text-2xl font-semibold tracking-tight">Proposals</h1>
            <p className="text-muted-foreground">
              Create and manage project proposals
            </p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load proposals. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Proposals</h1>
          <p className="text-muted-foreground">
            Create and manage project proposals
          </p>
        </div>
        <div className="flex flex-col gap-2 sm:flex-row sm:gap-4">
          <Button variant="outline" asChild className="w-full sm:w-auto">
            <Link href="/proposals/reports">
              <BarChart3 className="mr-2 h-4 w-4" />
              Analytics
            </Link>
          </Button>
          {canCreate && (
            <Button asChild className="w-full sm:w-auto">
              <Link href="/proposals/new">New Proposal</Link>
            </Button>
          )}
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Proposals</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{summary?.totalCount ?? 0}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pipeline Value</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {formatCurrency(summary?.pipelineValue ?? 0)}
            </div>
            <p className="text-xs text-muted-foreground">
              Active proposals (Draft, Submitted, Under Review, Approved)
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Won This Month</CardTitle>
            <Trophy className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {summary?.wonThisMonthCount ?? 0}
            </div>
            <p className="text-xs text-muted-foreground">
              {formatCurrency(summary?.wonThisMonthValue ?? 0)} total value
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Conversion Rate</CardTitle>
            <Percent className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {(summary?.conversionRate ?? 0).toFixed(1)}%
            </div>
            <p className="text-xs text-muted-foreground">
              Won / (Won + Lost)
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 sm:max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search proposals..."
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

        <Select value={companyFilter} onValueChange={handleCompanyChange}>
          <SelectTrigger className="w-full sm:w-[200px]">
            <SelectValue placeholder="Company" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Companies</SelectItem>
            {companies?.map((company) => (
              <SelectItem key={company.id} value={company.id}>
                {company.companyName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Data Table */}
      <DataTable
        columns={columns}
        data={proposalsData?.items ?? []}
        isLoading={isLoading}
        emptyMessage="No proposals found"
        keyExtractor={(proposal) => proposal.id}
        sortColumn={sortColumn}
        sortDirection={sortDirection}
        onSort={handleSort}
        onRowClick={(proposal) => router.push(`/proposals/${proposal.id}`)}
        pagination={{
          pageNumber: proposalsData?.pageNumber ?? 1,
          pageSize: proposalsData?.pageSize ?? 20,
          totalCount: proposalsData?.totalCount ?? 0,
          totalPages: proposalsData?.totalPages ?? 0,
        }}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Proposal"
        description={`Are you sure you want to delete proposal "${proposalToDelete?.proposalNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteProposal.isPending}
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
