"use client";

import * as React from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
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
import { LegislationModal } from "./legislation-modal";
import {
  useLegislationLibrary,
  useDeleteLegislationLibraryItem,
} from "@/lib/api/rams";
import { LegislationReferenceDto } from "@/types/rams";
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

export function LegislationLibrary() {
  const [searchInput, setSearchInput] = React.useState("");
  const [showInactive, setShowInactive] = React.useState(false);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Modal states
  const [modalOpen, setModalOpen] = React.useState(false);
  const [editingLegislation, setEditingLegislation] = React.useState<LegislationReferenceDto | null>(null);

  // Delete dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [legislationToDelete, setLegislationToDelete] = React.useState<LegislationReferenceDto | null>(null);

  // API hooks
  const { data: legislation, isLoading, error } = useLegislationLibrary({
    includeInactive: showInactive,
    search: debouncedSearch || undefined,
  });

  const deleteLegislation = useDeleteLegislationLibraryItem();

  const handleAdd = () => {
    setEditingLegislation(null);
    setModalOpen(true);
  };

  const handleEdit = (item: LegislationReferenceDto) => {
    setEditingLegislation(item);
    setModalOpen(true);
  };

  const handleDeleteClick = (item: LegislationReferenceDto) => {
    setLegislationToDelete(item);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!legislationToDelete) return;
    try {
      await deleteLegislation.mutateAsync(legislationToDelete.id);
      toast.success("Legislation deleted successfully");
      setDeleteDialogOpen(false);
      setLegislationToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to delete legislation", { description: message });
    }
  };

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center">
          <p className="text-destructive">Failed to load legislation. Please try again.</p>
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
                  placeholder="Search legislation..."
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  className="pl-9"
                />
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="showInactiveLegislation"
                  checked={showInactive}
                  onCheckedChange={(checked) => setShowInactive(checked === true)}
                />
                <label
                  htmlFor="showInactiveLegislation"
                  className="text-sm font-medium leading-none cursor-pointer"
                >
                  Show Inactive
                </label>
              </div>
            </div>
            <Button onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Legislation
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
      ) : !legislation || legislation.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <BookIcon className="mx-auto h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No Legislation Found</h3>
            <p className="mt-2 text-muted-foreground">
              {searchInput
                ? "No legislation matches your search."
                : "Add your first legislation reference to the library."}
            </p>
            <Button className="mt-4" onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add Legislation
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
                  <TableHead className="hidden md:table-cell">Short Name</TableHead>
                  <TableHead className="hidden lg:table-cell">Jurisdiction</TableHead>
                  <TableHead className="hidden xl:table-cell">Document</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {legislation.map((item) => (
                  <TableRow
                    key={item.id}
                    className={cn(!item.isActive && "opacity-50")}
                  >
                    <TableCell className="font-medium">{item.code}</TableCell>
                    <TableCell>
                      <div>{item.name}</div>
                      {item.description && (
                        <p className="text-sm text-muted-foreground truncate max-w-[300px]">
                          {item.description}
                        </p>
                      )}
                    </TableCell>
                    <TableCell className="hidden md:table-cell">
                      {item.shortName || "-"}
                    </TableCell>
                    <TableCell className="hidden lg:table-cell">
                      {item.jurisdiction ? (
                        <Badge variant="outline">{item.jurisdiction}</Badge>
                      ) : (
                        "-"
                      )}
                    </TableCell>
                    <TableCell className="hidden xl:table-cell">
                      {item.documentUrl ? (
                        <a
                          href={item.documentUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="text-sm text-blue-600 hover:underline"
                        >
                          View Document
                        </a>
                      ) : (
                        "-"
                      )}
                    </TableCell>
                    <TableCell>
                      {item.isActive ? (
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
                          onClick={() => handleEdit(item)}
                        >
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive hover:text-destructive"
                          onClick={() => handleDeleteClick(item)}
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
            Showing {legislation.length} legislation reference{legislation.length !== 1 ? "s" : ""}
          </CardFooter>
        </Card>
      )}

      {/* Modal */}
      <LegislationModal
        open={modalOpen}
        onOpenChange={setModalOpen}
        legislation={editingLegislation}
      />

      {/* Delete Dialog */}
      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete Legislation"
        description={`Are you sure you want to delete "${legislationToDelete?.name}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteLegislation.isPending}
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

function BookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
      />
    </svg>
  );
}
