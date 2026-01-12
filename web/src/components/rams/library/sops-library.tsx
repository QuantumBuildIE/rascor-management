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
import { SopModal } from "./sop-modal";
import {
  useSopLibrary,
  useDeleteSopLibraryItem,
} from "@/lib/api/rams";
import { SopReferenceDto } from "@/types/rams";
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

export function SopsLibrary() {
  const [searchInput, setSearchInput] = React.useState("");
  const [showInactive, setShowInactive] = React.useState(false);
  const debouncedSearch = useDebounce(searchInput, 300);

  // Modal states
  const [modalOpen, setModalOpen] = React.useState(false);
  const [editingSop, setEditingSop] = React.useState<SopReferenceDto | null>(null);

  // Delete dialog states
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [sopToDelete, setSopToDelete] = React.useState<SopReferenceDto | null>(null);

  // API hooks
  const { data: sops, isLoading, error } = useSopLibrary({
    includeInactive: showInactive,
    search: debouncedSearch || undefined,
  });

  const deleteSop = useDeleteSopLibraryItem();

  const handleAdd = () => {
    setEditingSop(null);
    setModalOpen(true);
  };

  const handleEdit = (sop: SopReferenceDto) => {
    setEditingSop(sop);
    setModalOpen(true);
  };

  const handleDeleteClick = (sop: SopReferenceDto) => {
    setSopToDelete(sop);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!sopToDelete) return;
    try {
      await deleteSop.mutateAsync(sopToDelete.id);
      toast.success("SOP deleted successfully");
      setDeleteDialogOpen(false);
      setSopToDelete(null);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An error occurred";
      toast.error("Failed to delete SOP", { description: message });
    }
  };

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center">
          <p className="text-destructive">Failed to load SOPs. Please try again.</p>
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
                  placeholder="Search SOPs..."
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  className="pl-9"
                />
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="showInactiveSops"
                  checked={showInactive}
                  onCheckedChange={(checked) => setShowInactive(checked === true)}
                />
                <label
                  htmlFor="showInactiveSops"
                  className="text-sm font-medium leading-none cursor-pointer"
                >
                  Show Inactive
                </label>
              </div>
            </div>
            <Button onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add SOP
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
      ) : !sops || sops.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <FileTextIcon className="mx-auto h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No SOPs Found</h3>
            <p className="mt-2 text-muted-foreground">
              {searchInput
                ? "No SOPs match your search."
                : "Add your first Standard Operating Procedure to the library."}
            </p>
            <Button className="mt-4" onClick={handleAdd}>
              <PlusIcon className="h-4 w-4 mr-2" />
              Add SOP
            </Button>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>SOP ID</TableHead>
                  <TableHead>Topic</TableHead>
                  <TableHead className="hidden md:table-cell">Description</TableHead>
                  <TableHead className="hidden lg:table-cell">Keywords</TableHead>
                  <TableHead className="hidden xl:table-cell">Document</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sops.map((sop) => (
                  <TableRow
                    key={sop.id}
                    className={cn(!sop.isActive && "opacity-50")}
                  >
                    <TableCell className="font-medium">{sop.sopId}</TableCell>
                    <TableCell>
                      <div>{sop.topic}</div>
                    </TableCell>
                    <TableCell className="hidden md:table-cell">
                      {sop.description ? (
                        <p className="text-sm text-muted-foreground truncate max-w-[250px]">
                          {sop.description}
                        </p>
                      ) : (
                        "-"
                      )}
                    </TableCell>
                    <TableCell className="hidden lg:table-cell">
                      {sop.taskKeywords ? (
                        <p className="text-sm truncate max-w-[200px]">
                          {sop.taskKeywords}
                        </p>
                      ) : (
                        "-"
                      )}
                    </TableCell>
                    <TableCell className="hidden xl:table-cell">
                      {sop.documentUrl ? (
                        <a
                          href={sop.documentUrl}
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
                      {sop.isActive ? (
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
                          onClick={() => handleEdit(sop)}
                        >
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive hover:text-destructive"
                          onClick={() => handleDeleteClick(sop)}
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
            Showing {sops.length} SOP{sops.length !== 1 ? "s" : ""}
          </CardFooter>
        </Card>
      )}

      {/* Modal */}
      <SopModal
        open={modalOpen}
        onOpenChange={setModalOpen}
        sop={editingSop}
      />

      {/* Delete Dialog */}
      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        title="Delete SOP"
        description={`Are you sure you want to delete "${sopToDelete?.topic}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        isLoading={deleteSop.isPending}
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

function FileTextIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth={2}
        d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
      />
    </svg>
  );
}
