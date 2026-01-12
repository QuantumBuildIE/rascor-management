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
import { ControlModal } from "./control-modal";
import {
  useControlLibrary,
  useDeleteControlLibraryItem,
} from "@/lib/api/rams";
import {
  ControlMeasureLibraryDto,
  ControlHierarchy,
  ControlHierarchyLabels,
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

export function ControlsLibrary() {
  const [searchInput, setSearchInput] = React.useState("");
  const [hierarchyFilter, setHierarchyFilter] = React.useState<string>("all");
  const [showInactive, setShowInactive] = React.useState(false);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Modal states
  const [modalOpen, setModalOpen] = React.useState(false);
  const [editingControl, setEditingControl] = React.useState<ControlMeasureLibraryDto | null>(null);

  // Delete dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [controlToDelete, setControlToDelete] = React.useState<ControlMeasureLibraryDto | null>(null);

  // API hooks
  const { data: controls, isLoading, error } = useControlLibrary({
    includeInactive: showInactive,
    hierarchy: hierarchyFilter !== "all" ? parseInt(hierarchyFilter) as ControlHierarchy : undefined,
    search: debouncedSearch || undefined,
  });

  const deleteControl = useDeleteControlLibraryItem();

  const handleAdd = () => {
    setEditingControl(null);
    setModalOpen(true);
  };

  const handleEdit = (control: ControlMeasureLibraryDto) => {
    setEditingControl(control);
    setModalOpen(true);
  };

  const handleDeleteClick = (control: ControlMeasureLibraryDto) => {
    setControlToDelete(control);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!controlToDelete) return;
    try {
      await deleteControl.mutateAsync(controlToDelete.id);
      toast.success("Control measure deleted successfully");
      setDeleteDialogOpen(false);
      setControlToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to delete control measure", { description: message });
    }
  };

  const getHierarchyBadgeClass = (hierarchy: ControlHierarchy) => {
    switch (hierarchy) {
      case ControlHierarchy.Elimination:
        return "bg-green-100 text-green-800";
      case ControlHierarchy.Substitution:
        return "bg-cyan-100 text-cyan-800";
      case ControlHierarchy.Engineering:
        return "bg-blue-100 text-blue-800";
      case ControlHierarchy.Administrative:
        return "bg-yellow-100 text-yellow-800";
      case ControlHierarchy.PPE:
        return "bg-gray-100 text-gray-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center">
          <p className="text-destructive">Failed to load control measures. Please try again.</p>
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
                  placeholder="Search controls..."
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  className="pl-9"
                />
              </div>
              <Select value={hierarchyFilter} onValueChange={setHierarchyFilter}>
                <SelectTrigger className="w-full sm:w-[200px]">
                  <SelectValue placeholder="All Hierarchies" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Hierarchies</SelectItem>
                  {Object.entries(ControlHierarchyLabels).map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="showInactiveControls"
                  checked={showInactive}
                  onCheckedChange={(checked) => setShowInactive(checked === true)}
                />
                <label
                  htmlFor="showInactiveControls"
                  className="text-sm font-medium leading-none cursor-pointer"
                >
                  Show Inactive
                </label>
              </div>
            </div>
            <Button onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Control
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
      ) : !controls || controls.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <ShieldCheckIcon className="mx-auto h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No Control Measures Found</h3>
            <p className="mt-2 text-muted-foreground">
              {searchInput || hierarchyFilter !== "all"
                ? "No control measures match your filters."
                : "Add your first control measure to the library."}
            </p>
            <Button className="mt-4" onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Control Measure
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
                  <TableHead className="hidden md:table-cell">Hierarchy</TableHead>
                  <TableHead className="hidden lg:table-cell">Applicable To</TableHead>
                  <TableHead className="hidden xl:table-cell">Risk Reduction</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {controls.map((control) => (
                  <TableRow
                    key={control.id}
                    className={cn(!control.isActive && "opacity-50")}
                  >
                    <TableCell className="font-medium">{control.code}</TableCell>
                    <TableCell>
                      <div>{control.name}</div>
                      <p className="text-sm text-muted-foreground truncate max-w-[300px]">
                        {control.description}
                      </p>
                    </TableCell>
                    <TableCell className="hidden md:table-cell">
                      <Badge className={getHierarchyBadgeClass(control.hierarchy)}>
                        {control.hierarchyDisplay}
                      </Badge>
                    </TableCell>
                    <TableCell className="hidden lg:table-cell">
                      {control.applicableToCategoryDisplay || "All"}
                    </TableCell>
                    <TableCell className="hidden xl:table-cell">
                      <span className="text-sm">
                        L-{control.typicalLikelihoodReduction}, S-{control.typicalSeverityReduction}
                      </span>
                    </TableCell>
                    <TableCell>
                      {control.isActive ? (
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
                          onClick={() => handleEdit(control)}
                        >
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive hover:text-destructive"
                          onClick={() => handleDeleteClick(control)}
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
            Showing {controls.length} control measure{controls.length !== 1 ? "s" : ""}
          </CardFooter>
        </Card>
      )}

      {/* Modal */}
      <ControlModal
        open={modalOpen}
        onOpenChange={setModalOpen}
        control={editingControl}
      />

      {/* Delete Dialog */}
      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Control Measure"
        description={`Are you sure you want to delete "${controlToDelete?.name}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteControl.isPending}
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

function ShieldCheckIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
      />
    </svg>
  );
}
