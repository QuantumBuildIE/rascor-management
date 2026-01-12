"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { format, isPast } from "date-fns";
import {
  ChevronLeft,
  Pencil,
  Printer,
  Trash2,
  Plus,
  MoreVertical,
  Package,
  FileDown,
  Download,
  ShoppingCart,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useProposal,
  useProposalRevisions,
  useDeleteProposal,
  useDeleteSection,
  useDeleteLineItem,
  useDeleteContact,
  downloadProposalPdf,
  type Proposal,
  type ProposalSection,
  type ProposalLineItem,
  type ProposalContact,
} from "@/lib/api/proposals";
import { ProposalStatusBadge } from "@/components/proposals/proposal-status-badge";
import {
  SubmitDialog,
  ApproveDialog,
  RejectDialog,
  WinDialog,
  LoseDialog,
  CancelDialog,
  CreateRevisionDialog,
} from "@/components/proposals/workflow-dialogs";
import { SectionDialog } from "@/components/proposals/section-dialog";
import { SectionFromKitDialog } from "@/components/proposals/section-from-kit-dialog";
import { LineItemDialog } from "@/components/proposals/line-item-dialog";
import { ContactDialog } from "@/components/proposals/contact-dialog";
import { ConvertToOrderDialog } from "@/components/proposals/convert-to-order-dialog";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { toast } from "sonner";
import { useHasAnyPermission } from "@/lib/auth/use-auth";
import { cn } from "@/lib/utils";

export default function ProposalDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  // Permissions
  const canViewCostings = useHasAnyPermission([
    "Proposals.ViewCostings",
    "Proposals.Admin",
  ]);
  const canSubmit = useHasAnyPermission(["Proposals.Submit", "Proposals.Admin"]);
  const canApprove = useHasAnyPermission(["Proposals.Approve", "Proposals.Admin"]);
  const canEdit = useHasAnyPermission(["Proposals.Edit", "Proposals.Admin"]);
  const canDelete = useHasAnyPermission(["Proposals.Delete", "Proposals.Admin"]);
  const canCreate = useHasAnyPermission(["Proposals.Create", "Proposals.Admin"]);

  // Data fetching
  const { data: proposal, isLoading, error } = useProposal(id, canViewCostings);
  const { data: revisions } = useProposalRevisions(id);
  const deleteProposal = useDeleteProposal();
  const deleteSection = useDeleteSection();
  const deleteLineItem = useDeleteLineItem();
  const deleteContact = useDeleteContact();

  // Dialog states
  const [submitDialogOpen, setSubmitDialogOpen] = React.useState(false);
  const [approveDialogOpen, setApproveDialogOpen] = React.useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] = React.useState(false);
  const [winDialogOpen, setWinDialogOpen] = React.useState(false);
  const [loseDialogOpen, setLoseDialogOpen] = React.useState(false);
  const [cancelDialogOpen, setCancelDialogOpen] = React.useState(false);
  const [revisionDialogOpen, setRevisionDialogOpen] = React.useState(false);
  const [deleteProposalDialogOpen, setDeleteProposalDialogOpen] = React.useState(false);

  // Section dialogs
  const [sectionDialogOpen, setSectionDialogOpen] = React.useState(false);
  const [sectionFromKitDialogOpen, setSectionFromKitDialogOpen] = React.useState(false);
  const [editingSection, setEditingSection] = React.useState<ProposalSection | null>(null);
  const [deleteSectionDialogOpen, setDeleteSectionDialogOpen] = React.useState(false);
  const [deletingSectionId, setDeletingSectionId] = React.useState<string | null>(null);

  // Line item dialogs
  const [lineItemDialogOpen, setLineItemDialogOpen] = React.useState(false);
  const [editingLineItem, setEditingLineItem] = React.useState<ProposalLineItem | null>(null);
  const [activeSection, setActiveSection] = React.useState<ProposalSection | null>(null);
  const [deleteLineItemDialogOpen, setDeleteLineItemDialogOpen] = React.useState(false);
  const [deletingLineItemId, setDeletingLineItemId] = React.useState<string | null>(null);

  // Contact dialogs
  const [contactDialogOpen, setContactDialogOpen] = React.useState(false);
  const [editingContact, setEditingContact] = React.useState<ProposalContact | null>(null);
  const [deleteContactDialogOpen, setDeleteContactDialogOpen] = React.useState(false);
  const [deletingContactId, setDeletingContactId] = React.useState<string | null>(null);

  // Stock order conversion dialog
  const [convertDialogOpen, setConvertDialogOpen] = React.useState(false);

  // PDF download state
  const [isDownloadingPdf, setIsDownloadingPdf] = React.useState(false);

  // Determine if proposal is editable
  const isEditable = proposal?.status === "Draft";

  // Check if proposal is expired
  const isExpired =
    proposal?.validUntilDate && isPast(new Date(proposal.validUntilDate));

  // Handler for creating revision and navigating to new proposal
  const handleRevisionCreated = (newProposal: Proposal) => {
    router.push(`/proposals/${newProposal.id}`);
  };

  // Delete handlers
  const handleDeleteProposal = async () => {
    if (!proposal) return;
    try {
      await deleteProposal.mutateAsync(proposal.id);
      toast.success("Proposal deleted successfully");
      router.push("/proposals");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete proposal", { description: message });
    }
  };

  const handleDeleteSection = async () => {
    if (!deletingSectionId) return;
    try {
      await deleteSection.mutateAsync(deletingSectionId);
      toast.success("Section deleted successfully");
      setDeleteSectionDialogOpen(false);
      setDeletingSectionId(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete section", { description: message });
    }
  };

  const handleDeleteLineItem = async () => {
    if (!deletingLineItemId) return;
    try {
      await deleteLineItem.mutateAsync(deletingLineItemId);
      toast.success("Line item deleted successfully");
      setDeleteLineItemDialogOpen(false);
      setDeletingLineItemId(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete line item", { description: message });
    }
  };

  const handleDeleteContact = async () => {
    if (!deletingContactId) return;
    try {
      await deleteContact.mutateAsync(deletingContactId);
      toast.success("Contact deleted successfully");
      setDeleteContactDialogOpen(false);
      setDeletingContactId(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete contact", { description: message });
    }
  };

  // Open handlers
  const openEditSection = (section: ProposalSection) => {
    setEditingSection(section);
    setSectionDialogOpen(true);
  };

  const openAddLineItem = (section: ProposalSection) => {
    setActiveSection(section);
    setEditingLineItem(null);
    setLineItemDialogOpen(true);
  };

  const openEditLineItem = (section: ProposalSection, lineItem: ProposalLineItem) => {
    setActiveSection(section);
    setEditingLineItem(lineItem);
    setLineItemDialogOpen(true);
  };

  const openEditContact = (contact: ProposalContact) => {
    setEditingContact(contact);
    setContactDialogOpen(true);
  };

  // Format currency
  const formatCurrency = (value: number, currency: string = "EUR") => {
    return value.toLocaleString("en-IE", {
      style: "currency",
      currency,
    });
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/proposals">
              <ChevronLeft className="mr-1 h-4 w-4" />
              Back to Proposals
            </Link>
          </Button>
        </div>
        <div className="space-y-4">
          <Skeleton className="h-10 w-64" />
          <div className="grid gap-4 md:grid-cols-4">
            <Skeleton className="h-24" />
            <Skeleton className="h-24" />
            <Skeleton className="h-24" />
            <Skeleton className="h-24" />
          </div>
          <Skeleton className="h-64" />
        </div>
      </div>
    );
  }

  // Error state
  if (error || !proposal) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/proposals">
              <ChevronLeft className="mr-1 h-4 w-4" />
              Back to Proposals
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load proposal details. Please try again.
          </p>
        </div>
      </div>
    );
  }

  // Get next sort order for sections
  const nextSectionSortOrder =
    proposal.sections.length > 0
      ? Math.max(...proposal.sections.map((s) => s.sortOrder)) + 1
      : 0;

  // PDF download handler
  const handleDownloadPdf = async (includeCosting: boolean = false) => {
    if (!proposal) return;
    setIsDownloadingPdf(true);
    try {
      await downloadProposalPdf(
        proposal.id,
        proposal.proposalNumber,
        proposal.version,
        includeCosting
      );
      toast.success("PDF downloaded successfully");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to download PDF", { description: message });
    } finally {
      setIsDownloadingPdf(false);
    }
  };

  // Render workflow actions
  const renderWorkflowActions = () => {
    const actions: React.ReactNode[] = [];

    // Print/PDF dropdown
    actions.push(
      <DropdownMenu key="print-pdf">
        <DropdownMenuTrigger asChild>
          <Button variant="outline" disabled={isDownloadingPdf}>
            {isDownloadingPdf ? (
              <>
                <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary border-r-transparent" />
                Generating...
              </>
            ) : (
              <>
                <Printer className="mr-2 h-4 w-4" />
                Print / PDF
              </>
            )}
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={() => handleDownloadPdf(false)}>
            <Download className="mr-2 h-4 w-4" />
            Download PDF
          </DropdownMenuItem>
          {canViewCostings && (
            <DropdownMenuItem onClick={() => handleDownloadPdf(true)}>
              <Download className="mr-2 h-4 w-4" />
              Download PDF (with costing)
            </DropdownMenuItem>
          )}
          <DropdownMenuSeparator />
          <DropdownMenuItem
            onClick={() => window.open(`/proposals/${proposal.id}/print`, "_blank")}
          >
            <Printer className="mr-2 h-4 w-4" />
            Print Preview
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    );

    // Status-based actions
    switch (proposal.status) {
      case "Draft":
        if (canEdit) {
          actions.push(
            <Button key="edit" variant="outline" asChild>
              <Link href={`/proposals/${proposal.id}/edit`}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </Link>
            </Button>
          );
        }
        if (canSubmit) {
          actions.push(
            <Button key="submit" onClick={() => setSubmitDialogOpen(true)}>
              Submit for Approval
            </Button>
          );
        }
        if (canDelete) {
          actions.push(
            <Button
              key="delete"
              variant="destructive"
              onClick={() => setDeleteProposalDialogOpen(true)}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </Button>
          );
        }
        break;

      case "Submitted":
      case "UnderReview":
        if (canApprove) {
          actions.push(
            <Button key="approve" onClick={() => setApproveDialogOpen(true)}>
              Approve
            </Button>
          );
          actions.push(
            <Button
              key="reject"
              variant="destructive"
              onClick={() => setRejectDialogOpen(true)}
            >
              Reject
            </Button>
          );
          actions.push(
            <Button
              key="cancel"
              variant="outline"
              onClick={() => setCancelDialogOpen(true)}
            >
              Cancel
            </Button>
          );
        }
        break;

      case "Approved":
        if (canEdit) {
          actions.push(
            <Button key="win" onClick={() => setWinDialogOpen(true)}>
              Mark as Won
            </Button>
          );
          actions.push(
            <Button
              key="lose"
              variant="destructive"
              onClick={() => setLoseDialogOpen(true)}
            >
              Mark as Lost
            </Button>
          );
          actions.push(
            <Button
              key="cancel"
              variant="outline"
              onClick={() => setCancelDialogOpen(true)}
            >
              Cancel
            </Button>
          );
        }
        break;

      case "Won":
        if (canEdit) {
          actions.push(
            <Button
              key="convert"
              onClick={() => setConvertDialogOpen(true)}
            >
              <ShoppingCart className="mr-2 h-4 w-4" />
              Convert to Stock Order
            </Button>
          );
        }
        if (canCreate) {
          actions.push(
            <Button
              key="revision"
              variant="outline"
              onClick={() => setRevisionDialogOpen(true)}
            >
              Create Revision
            </Button>
          );
        }
        break;

      case "Rejected":
      case "Lost":
      case "Expired":
        if (canCreate) {
          actions.push(
            <Button key="revision" onClick={() => setRevisionDialogOpen(true)}>
              Create Revision
            </Button>
          );
        }
        break;
    }

    return actions;
  };

  return (
    <div className="space-y-6">
      {/* Back button */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/proposals">
            <ChevronLeft className="mr-1 h-4 w-4" />
            Back to Proposals
          </Link>
        </Button>
      </div>

      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold tracking-tight">
              {proposal.proposalNumber} v{proposal.version}
            </h1>
            <ProposalStatusBadge status={proposal.status} />
            {isExpired && proposal.status !== "Won" && proposal.status !== "Lost" && (
              <Badge variant="destructive">Expired</Badge>
            )}
          </div>
          <p className="text-muted-foreground">{proposal.projectName}</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {renderWorkflowActions()}
        </div>
      </div>

      {/* Overview Cards */}
      <div className="grid gap-4 md:grid-cols-4 lg:grid-cols-5">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Grand Total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {formatCurrency(proposal.grandTotal, proposal.currency)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Net Total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {formatCurrency(proposal.netTotal, proposal.currency)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Discount
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{proposal.discountPercent}%</p>
            <p className="text-sm text-muted-foreground">
              ({formatCurrency(proposal.discountAmount, proposal.currency)})
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              VAT ({proposal.vatRate}%)
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">
              {formatCurrency(proposal.vatAmount, proposal.currency)}
            </p>
          </CardContent>
        </Card>
        {canViewCostings && proposal.totalMargin !== undefined && (
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Margin
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p
                className={cn(
                  "text-2xl font-bold",
                  proposal.totalMargin >= 0 ? "text-green-600" : "text-red-600"
                )}
              >
                {formatCurrency(proposal.totalMargin, proposal.currency)}
              </p>
              <p className="text-sm text-muted-foreground">
                ({proposal.marginPercent?.toFixed(1) ?? 0}%)
              </p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Details Section */}
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Client & Project</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <p className="text-sm text-muted-foreground">Company</p>
              <p className="font-medium">{proposal.companyName}</p>
            </div>
            {proposal.primaryContactName && (
              <div>
                <p className="text-sm text-muted-foreground">Primary Contact</p>
                <p className="font-medium">{proposal.primaryContactName}</p>
              </div>
            )}
            <div>
              <p className="text-sm text-muted-foreground">Project Name</p>
              <p className="font-medium">{proposal.projectName}</p>
            </div>
            {proposal.projectAddress && (
              <div>
                <p className="text-sm text-muted-foreground">Project Address</p>
                <p className="font-medium">{proposal.projectAddress}</p>
              </div>
            )}
            {proposal.projectDescription && (
              <div>
                <p className="text-sm text-muted-foreground">Project Description</p>
                <p className="font-medium">{proposal.projectDescription}</p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Dates & Terms</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <p className="text-sm text-muted-foreground">Proposal Date</p>
              <p className="font-medium">
                {format(new Date(proposal.proposalDate), "PPP")}
              </p>
            </div>
            {proposal.validUntilDate && (
              <div>
                <p className="text-sm text-muted-foreground">Valid Until</p>
                <p className="font-medium flex items-center gap-2">
                  {format(new Date(proposal.validUntilDate), "PPP")}
                  {isExpired && (
                    <Badge variant="destructive" className="text-xs">
                      Expired
                    </Badge>
                  )}
                </p>
              </div>
            )}
            {proposal.submittedDate && (
              <div>
                <p className="text-sm text-muted-foreground">Submitted</p>
                <p className="font-medium">
                  {format(new Date(proposal.submittedDate), "PPP")}
                </p>
              </div>
            )}
            {proposal.approvedBy && proposal.approvedDate && (
              <div>
                <p className="text-sm text-muted-foreground">Approved By</p>
                <p className="font-medium">
                  {proposal.approvedBy} on{" "}
                  {format(new Date(proposal.approvedDate), "PPP")}
                </p>
              </div>
            )}
            {proposal.wonDate && (
              <div>
                <p className="text-sm text-muted-foreground">Won Date</p>
                <p className="font-medium">
                  {format(new Date(proposal.wonDate), "PPP")}
                </p>
              </div>
            )}
            {proposal.lostDate && (
              <div>
                <p className="text-sm text-muted-foreground">Lost Date</p>
                <p className="font-medium">
                  {format(new Date(proposal.lostDate), "PPP")}
                </p>
              </div>
            )}
            {proposal.paymentTerms && (
              <div>
                <p className="text-sm text-muted-foreground">Payment Terms</p>
                <p className="font-medium">{proposal.paymentTerms}</p>
              </div>
            )}
            <div>
              <p className="text-sm text-muted-foreground">Currency</p>
              <p className="font-medium">{proposal.currency}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Sections & Line Items */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Sections & Line Items</CardTitle>
            {isEditable && (
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    setEditingSection(null);
                    setSectionDialogOpen(true);
                  }}
                >
                  <Plus className="mr-2 h-4 w-4" />
                  Add Section
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setSectionFromKitDialogOpen(true)}
                >
                  <Package className="mr-2 h-4 w-4" />
                  Add from Kit
                </Button>
              </div>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {proposal.sections.length === 0 ? (
            <div className="py-8 text-center text-muted-foreground">
              No sections yet. Add a section to start building your proposal.
            </div>
          ) : (
            <Accordion type="multiple" className="w-full" defaultValue={proposal.sections.map(s => s.id)}>
              {proposal.sections
                .sort((a, b) => a.sortOrder - b.sortOrder)
                .map((section) => (
                  <AccordionItem key={section.id} value={section.id}>
                    <AccordionTrigger className="hover:no-underline">
                      <div className="flex flex-1 items-center justify-between pr-4">
                        <div className="text-left">
                          <div className="font-medium">{section.sectionName}</div>
                          {section.description && (
                            <div className="text-sm text-muted-foreground">
                              {section.description}
                            </div>
                          )}
                        </div>
                        <div className="text-right">
                          <div className="font-medium">
                            {formatCurrency(section.sectionTotal, proposal.currency)}
                          </div>
                          {canViewCostings && (
                            <div className="text-sm text-muted-foreground">
                              Margin:{" "}
                              {formatCurrency(section.sectionMargin, proposal.currency)}
                            </div>
                          )}
                        </div>
                      </div>
                    </AccordionTrigger>
                    <AccordionContent>
                      <div className="space-y-4 pt-2">
                        {/* Section actions */}
                        {isEditable && (
                          <div className="flex gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => openAddLineItem(section)}
                            >
                              <Plus className="mr-2 h-4 w-4" />
                              Add Item
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => openEditSection(section)}
                            >
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit Section
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => {
                                setDeletingSectionId(section.id);
                                setDeleteSectionDialogOpen(true);
                              }}
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete Section
                            </Button>
                          </div>
                        )}

                        {/* Line items table */}
                        {section.lineItems.length === 0 ? (
                          <div className="py-4 text-center text-muted-foreground">
                            No items in this section.
                          </div>
                        ) : (
                          <div className="overflow-x-auto">
                            <Table>
                              <TableHeader>
                                <TableRow>
                                  <TableHead>Product</TableHead>
                                  <TableHead>Description</TableHead>
                                  <TableHead className="text-right">Qty</TableHead>
                                  <TableHead>Unit</TableHead>
                                  {canViewCostings && (
                                    <TableHead className="text-right">
                                      Unit Cost
                                    </TableHead>
                                  )}
                                  <TableHead className="text-right">
                                    Unit Price
                                  </TableHead>
                                  {canViewCostings && (
                                    <>
                                      <TableHead className="text-right">
                                        Line Cost
                                      </TableHead>
                                      <TableHead className="text-right">
                                        Margin
                                      </TableHead>
                                    </>
                                  )}
                                  <TableHead className="text-right">Total</TableHead>
                                  {isEditable && (
                                    <TableHead className="w-10"></TableHead>
                                  )}
                                </TableRow>
                              </TableHeader>
                              <TableBody>
                                {section.lineItems
                                  .sort((a, b) => a.sortOrder - b.sortOrder)
                                  .map((item) => (
                                    <TableRow key={item.id}>
                                      <TableCell className="font-medium">
                                        {item.productCode ?? "-"}
                                      </TableCell>
                                      <TableCell>{item.description}</TableCell>
                                      <TableCell className="text-right">
                                        {item.quantity}
                                      </TableCell>
                                      <TableCell>{item.unit}</TableCell>
                                      {canViewCostings && (
                                        <TableCell className="text-right">
                                          {formatCurrency(
                                            item.unitCost,
                                            proposal.currency
                                          )}
                                        </TableCell>
                                      )}
                                      <TableCell className="text-right">
                                        {formatCurrency(
                                          item.unitPrice,
                                          proposal.currency
                                        )}
                                      </TableCell>
                                      {canViewCostings && (
                                        <>
                                          <TableCell className="text-right">
                                            {formatCurrency(
                                              item.lineCost,
                                              proposal.currency
                                            )}
                                          </TableCell>
                                          <TableCell
                                            className={cn(
                                              "text-right",
                                              item.lineMargin >= 0
                                                ? "text-green-600"
                                                : "text-red-600"
                                            )}
                                          >
                                            {formatCurrency(
                                              item.lineMargin,
                                              proposal.currency
                                            )}{" "}
                                            ({item.marginPercent.toFixed(1)}%)
                                          </TableCell>
                                        </>
                                      )}
                                      <TableCell className="text-right font-medium">
                                        {formatCurrency(
                                          item.lineTotal,
                                          proposal.currency
                                        )}
                                      </TableCell>
                                      {isEditable && (
                                        <TableCell>
                                          <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                              <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8"
                                              >
                                                <MoreVertical className="h-4 w-4" />
                                              </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end">
                                              <DropdownMenuItem
                                                onClick={() =>
                                                  openEditLineItem(section, item)
                                                }
                                              >
                                                <Pencil className="mr-2 h-4 w-4" />
                                                Edit
                                              </DropdownMenuItem>
                                              <DropdownMenuSeparator />
                                              <DropdownMenuItem
                                                className="text-destructive"
                                                onClick={() => {
                                                  setDeletingLineItemId(item.id);
                                                  setDeleteLineItemDialogOpen(true);
                                                }}
                                              >
                                                <Trash2 className="mr-2 h-4 w-4" />
                                                Delete
                                              </DropdownMenuItem>
                                            </DropdownMenuContent>
                                          </DropdownMenu>
                                        </TableCell>
                                      )}
                                    </TableRow>
                                  ))}
                                {/* Section total row */}
                                <TableRow className="bg-muted/50">
                                  <TableCell
                                    colSpan={canViewCostings ? (isEditable ? 8 : 7) : (isEditable ? 5 : 4)}
                                    className="text-right font-medium"
                                  >
                                    Section Total
                                  </TableCell>
                                  <TableCell className="text-right font-bold">
                                    {formatCurrency(
                                      section.sectionTotal,
                                      proposal.currency
                                    )}
                                  </TableCell>
                                  {isEditable && <TableCell />}
                                </TableRow>
                              </TableBody>
                            </Table>
                          </div>
                        )}
                      </div>
                    </AccordionContent>
                  </AccordionItem>
                ))}
            </Accordion>
          )}
        </CardContent>
      </Card>

      {/* Contacts Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Contacts</CardTitle>
            {isEditable && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  setEditingContact(null);
                  setContactDialogOpen(true);
                }}
              >
                <Plus className="mr-2 h-4 w-4" />
                Add Contact
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {proposal.contacts.length === 0 ? (
            <div className="py-8 text-center text-muted-foreground">
              No contacts added.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Phone</TableHead>
                  <TableHead>Primary</TableHead>
                  {isEditable && <TableHead className="w-10"></TableHead>}
                </TableRow>
              </TableHeader>
              <TableBody>
                {proposal.contacts.map((contact) => (
                  <TableRow key={contact.id}>
                    <TableCell className="font-medium">
                      {contact.contactName}
                    </TableCell>
                    <TableCell>{contact.role}</TableCell>
                    <TableCell>{contact.email ?? "-"}</TableCell>
                    <TableCell>{contact.phone ?? "-"}</TableCell>
                    <TableCell>
                      {contact.isPrimary && (
                        <Badge variant="secondary">Primary</Badge>
                      )}
                    </TableCell>
                    {isEditable && (
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon" className="h-8 w-8">
                              <MoreVertical className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => openEditContact(contact)}
                            >
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              className="text-destructive"
                              onClick={() => {
                                setDeletingContactId(contact.id);
                                setDeleteContactDialogOpen(true);
                              }}
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    )}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Revision History */}
      {revisions && revisions.length > 1 && (
        <Card>
          <CardHeader>
            <CardTitle>Revision History</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Version</TableHead>
                  <TableHead>Proposal #</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {revisions
                  .sort((a, b) => b.version - a.version)
                  .map((revision) => (
                    <TableRow
                      key={revision.id}
                      className={cn(
                        "cursor-pointer hover:bg-muted/50",
                        revision.id === proposal.id && "bg-muted/50"
                      )}
                      onClick={() => {
                        if (revision.id !== proposal.id) {
                          router.push(`/proposals/${revision.id}`);
                        }
                      }}
                    >
                      <TableCell className="font-medium">
                        v{revision.version}
                        {revision.id === proposal.id && (
                          <Badge variant="secondary" className="ml-2">
                            Current
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>{revision.proposalNumber}</TableCell>
                      <TableCell>
                        {format(new Date(revision.proposalDate), "PPP")}
                      </TableCell>
                      <TableCell>
                        <ProposalStatusBadge status={revision.status} />
                      </TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(revision.grandTotal, revision.currency)}
                      </TableCell>
                    </TableRow>
                  ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Notes Section */}
      {(proposal.notes || proposal.wonLostReason) && (
        <Card>
          <CardHeader>
            <CardTitle>Notes</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {proposal.notes && (
              <div>
                <p className="text-sm text-muted-foreground">General Notes</p>
                <p className="whitespace-pre-wrap">{proposal.notes}</p>
              </div>
            )}
            {proposal.wonLostReason && (
              <div>
                <p className="text-sm text-muted-foreground">
                  {proposal.status === "Won" ? "Won" : "Lost"} Reason
                </p>
                <p className="whitespace-pre-wrap">{proposal.wonLostReason}</p>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Attachments Section */}
      {proposal.drawingFileName && (
        <Card>
          <CardHeader>
            <CardTitle>Attachments</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <FileDown className="h-5 w-5 text-muted-foreground" />
                <span>{proposal.drawingFileName}</span>
              </div>
              {proposal.drawingUrl && (
                <Button variant="outline" size="sm" asChild>
                  <a href={proposal.drawingUrl} download>
                    Download
                  </a>
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Dialogs */}
      <SubmitDialog
        open={submitDialogOpen}
        onOpenChange={setSubmitDialogOpen}
        proposal={proposal}
      />
      <ApproveDialog
        open={approveDialogOpen}
        onOpenChange={setApproveDialogOpen}
        proposal={proposal}
      />
      <RejectDialog
        open={rejectDialogOpen}
        onOpenChange={setRejectDialogOpen}
        proposal={proposal}
      />
      <WinDialog
        open={winDialogOpen}
        onOpenChange={setWinDialogOpen}
        proposal={proposal}
      />
      <LoseDialog
        open={loseDialogOpen}
        onOpenChange={setLoseDialogOpen}
        proposal={proposal}
      />
      <CancelDialog
        open={cancelDialogOpen}
        onOpenChange={setCancelDialogOpen}
        proposal={proposal}
      />
      <CreateRevisionDialog
        open={revisionDialogOpen}
        onOpenChange={setRevisionDialogOpen}
        proposal={proposal}
        onRevisionCreated={handleRevisionCreated}
      />
      <DeleteConfirmationDialog
        open={deleteProposalDialogOpen}
        onOpenChange={setDeleteProposalDialogOpen}
        title="Delete Proposal"
        description={`Are you sure you want to delete proposal "${proposal.proposalNumber}"? This action cannot be undone.`}
        onConfirm={handleDeleteProposal}
        isLoading={deleteProposal.isPending}
      />

      {/* Section Dialogs */}
      <SectionDialog
        open={sectionDialogOpen}
        onOpenChange={(open) => {
          setSectionDialogOpen(open);
          if (!open) setEditingSection(null);
        }}
        proposalId={proposal.id}
        section={editingSection}
        nextSortOrder={nextSectionSortOrder}
      />
      <SectionFromKitDialog
        open={sectionFromKitDialogOpen}
        onOpenChange={setSectionFromKitDialogOpen}
        proposalId={proposal.id}
        nextSortOrder={nextSectionSortOrder}
      />
      <DeleteConfirmationDialog
        open={deleteSectionDialogOpen}
        onOpenChange={(open) => {
          setDeleteSectionDialogOpen(open);
          if (!open) setDeletingSectionId(null);
        }}
        title="Delete Section"
        description="Are you sure you want to delete this section? All line items in this section will also be deleted. This action cannot be undone."
        onConfirm={handleDeleteSection}
        isLoading={deleteSection.isPending}
      />

      {/* Line Item Dialogs */}
      <LineItemDialog
        open={lineItemDialogOpen}
        onOpenChange={(open) => {
          setLineItemDialogOpen(open);
          if (!open) {
            setEditingLineItem(null);
            setActiveSection(null);
          }
        }}
        sectionId={activeSection?.id ?? ""}
        lineItem={editingLineItem}
        nextSortOrder={
          activeSection
            ? activeSection.lineItems.length > 0
              ? Math.max(...activeSection.lineItems.map((i) => i.sortOrder)) + 1
              : 0
            : 0
        }
      />
      <DeleteConfirmationDialog
        open={deleteLineItemDialogOpen}
        onOpenChange={(open) => {
          setDeleteLineItemDialogOpen(open);
          if (!open) setDeletingLineItemId(null);
        }}
        title="Delete Line Item"
        description="Are you sure you want to delete this line item? This action cannot be undone."
        onConfirm={handleDeleteLineItem}
        isLoading={deleteLineItem.isPending}
      />

      {/* Contact Dialogs */}
      <ContactDialog
        open={contactDialogOpen}
        onOpenChange={(open) => {
          setContactDialogOpen(open);
          if (!open) setEditingContact(null);
        }}
        proposalId={proposal.id}
        companyId={proposal.companyId}
        contact={editingContact}
      />
      <DeleteConfirmationDialog
        open={deleteContactDialogOpen}
        onOpenChange={(open) => {
          setDeleteContactDialogOpen(open);
          if (!open) setDeletingContactId(null);
        }}
        title="Delete Contact"
        description="Are you sure you want to remove this contact from the proposal? This action cannot be undone."
        onConfirm={handleDeleteContact}
        isLoading={deleteContact.isPending}
      />

      {/* Stock Order Conversion Dialog */}
      <ConvertToOrderDialog
        proposal={proposal}
        open={convertDialogOpen}
        onOpenChange={setConvertDialogOpen}
      />
    </div>
  );
}
