"use client";

import * as React from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { HazardModal } from "./hazard-modal";
import {
  useHazardLibrary,
  useDeleteHazardLibraryItem,
} from "@/lib/api/rams";
import {
  HazardLibraryDto,
  HazardCategory,
  HazardCategoryLabels,
} from "@/types/rams";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = React.useState(value);
  React.useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);
  return debouncedValue;
}

export function HazardsLibrary() {
  const [searchInput, setSearchInput] = React.useState("");
  const [categoryFilter, setCategoryFilter] = React.useState<string>("all");
  const [showInactive, setShowInactive] = React.useState(false);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Modal states
  const [modalOpen, setModalOpen] = React.useState(false);
  const [editingHazard, setEditingHazard] = React.useState<HazardLibraryDto | null>(null);

  // Delete dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [hazardToDelete, setHazardToDelete] = React.useState<HazardLibraryDto | null>(null);

  // API hooks
  const { data: hazards, isLoading, error } = useHazardLibrary({
    includeInactive: showInactive,
    category: categoryFilter !== "all" ? parseInt(categoryFilter) as HazardCategory : undefined,
    search: debouncedSearch || undefined,
  });

  const deleteHazard = useDeleteHazardLibraryItem();

  const handleAdd = () => {
    setEditingHazard(null);
    setModalOpen(true);
  };

  const handleEdit = (hazard: HazardLibraryDto) => {
    setEditingHazard(hazard);
    setModalOpen(true);
  };

  const handleDeleteClick = (hazard: HazardLibraryDto) => {
    setHazardToDelete(hazard);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!hazardToDelete) return;
    try {
      await deleteHazard.mutateAsync(hazardToDelete.id);
      toast.success("Hazard deleted successfully");
      setDeleteDialogOpen(false);
      setHazardToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to delete hazard", { description: message });
    }
  };

  const getRiskBadgeClass = (likelihood: number, severity: number) => {
    const rating = likelihood * severity;
    if (rating <= 4) return "bg-green-100 text-green-800";
    if (rating <= 12) return "bg-yellow-100 text-yellow-800";
    return "bg-red-100 text-red-800";
  };

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center">
          <p className="text-destructive">Failed to load hazards. Please try again.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <Card>
        <CardContent className="py-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center flex-1">
              <div className="relative flex-1 sm:max-w-sm">
                <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="Search hazards..."
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  className="pl-9"
                />
              </div>
              <Select value={categoryFilter} onValueChange={setCategoryFilter}>
                <SelectTrigger className="w-full sm:w-[200px]">
                  <SelectValue placeholder="All Categories" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Categories</SelectItem>
                  {Object.entries(HazardCategoryLabels).map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="showInactive"
                  checked={showInactive}
                  onCheckedChange={(checked) => setShowInactive(checked === true)}
                />
                <label
                  htmlFor="showInactive"
                  className="text-sm font-medium leading-none cursor-pointer"
                >
                  Show Inactive
                </label>
              </div>
            </div>
            <Button onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Hazard
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      {isLoading ? (
        <Card>
          <CardContent className="py-8">
            <div className="flex justify-center">
              <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-r-transparent" />
            </div>
          </CardContent>
        </Card>
      ) : !hazards || hazards.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <AlertTriangleIcon className="mx-auto h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No Hazards Found</h3>
            <p className="mt-2 text-muted-foreground">
              {searchInput || categoryFilter !== "all"
                ? "No hazards match your filters."
                : "Add your first hazard to the library."}
            </p>
            <Button className="mt-4" onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Hazard
            </Button>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className="hidden md:table-cell">Category</TableHead>
                  <TableHead className="hidden lg:table-cell">Default Risk</TableHead>
                  <TableHead className="hidden xl:table-cell">Who at Risk</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {hazards.map((hazard) => (
                  <TableRow
                    key={hazard.id}
                    className={cn(!hazard.isActive && "opacity-50")}
                  >
                    <TableCell className="font-medium">{hazard.code}</TableCell>
                    <TableCell>
                      <div>{hazard.name}</div>
                      {hazard.description && (
                        <p className="text-sm text-muted-foreground truncate max-w-[300px]">
                          {hazard.description}
                        </p>
                      )}
                    </TableCell>
                    <TableCell className="hidden md:table-cell">
                      <Badge variant="secondary">{hazard.categoryDisplay}</Badge>
                    </TableCell>
                    <TableCell className="hidden lg:table-cell">
                      <div className="flex items-center gap-2">
                        <Badge
                          className={getRiskBadgeClass(
                            hazard.defaultLikelihood,
                            hazard.defaultSeverity
                          )}
                        >
                          {hazard.defaultLikelihood * hazard.defaultSeverity}
                        </Badge>
                        <span className="text-sm text-muted-foreground">
                          (L{hazard.defaultLikelihood} x S{hazard.defaultSeverity})
                        </span>
                      </div>
                    </TableCell>
                    <TableCell className="hidden xl:table-cell">
                      {hazard.typicalWhoAtRisk || "-"}
                    </TableCell>
                    <TableCell>
                      {hazard.isActive ? (
                        <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">
                          Active
                        </Badge>
                      ) : (
                        <Badge variant="secondary">Inactive</Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEdit(hazard)}
                        >
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive hover:text-destructive"
                          onClick={() => handleDeleteClick(hazard)}
                        >
                          Delete
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <CardFooter className="text-sm text-muted-foreground border-t py-3">
            Showing {hazards.length} hazard{hazards.length !== 1 ? "s" : ""}
          </CardFooter>
        </Card>
      )}

      {/* Modal */}
      <HazardModal
        open={modalOpen}
        onOpenChange={setModalOpen}
        hazard={editingHazard}
      />

      {/* Delete Dialog */}
      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Hazard"
        description={`Are you sure you want to delete "${hazardToDelete?.name}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteHazard.isPending}
      />
    </div>
  );
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
      />
    </svg>
  );
}

function PlusIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
    </svg>
  );
}

function AlertTriangleIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
      />
    </svg>
  );
}
