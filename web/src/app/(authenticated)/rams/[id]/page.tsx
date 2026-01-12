"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { format } from "date-fns";
import {
  ChevronLeft,
  Pencil,
  Trash2,
  Plus,
  AlertTriangle,
  ListOrdered,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  useRamsDocument,
  useDeleteRamsDocument,
  useSubmitRamsDocument,
  useApproveRamsDocument,
  useRejectRamsDocument,
  useRiskAssessments,
  useDeleteRiskAssessment,
  useMethodSteps,
  useDeleteMethodStep,
  type RiskAssessmentDto,
  type MethodStepDto,
} from "@/lib/api/rams";
import {
  RamsStatusBadge,
  RiskAssessmentDialog,
  MethodStepDialog,
  SortableRiskAssessmentTable,
  SortableMethodStepList,
} from "@/components/rams";
import { DeleteConfirmationDialog } from "@/components/shared/delete-confirmation-dialog";
import { toast } from "sonner";
import { useHasAnyPermission } from "@/lib/auth/use-auth";
export default function RamsDocumentDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  // Permissions
  const canCreate = useHasAnyPermission(["Rams.Create", "Rams.Admin"]);
  const canEdit = useHasAnyPermission(["Rams.Edit", "Rams.Admin"]);
  const canDelete = useHasAnyPermission(["Rams.Delete", "Rams.Admin"]);
  const canApprove = useHasAnyPermission(["Rams.Approve", "Rams.Admin"]);

  // Data fetching
  const { data: document, isLoading: isLoadingDocument, error: documentError } = useRamsDocument(id);
  const { data: riskAssessments = [], isLoading: isLoadingRisks } = useRiskAssessments(id);
  const { data: methodSteps = [], isLoading: isLoadingSteps } = useMethodSteps(id);

  // Mutations
  const deleteDocument = useDeleteRamsDocument();
  const submitDocument = useSubmitRamsDocument();
  const approveDocument = useApproveRamsDocument();
  const rejectDocument = useRejectRamsDocument();
  const deleteRiskAssessment = useDeleteRiskAssessment();
  const deleteMethodStep = useDeleteMethodStep();

  // Dialog states
  const [deleteDocumentDialogOpen, setDeleteDocumentDialogOpen] = React.useState(false);
  const [approvalDialogOpen, setApprovalDialogOpen] = React.useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] = React.useState(false);
  const [approvalComments, setApprovalComments] = React.useState("");

  // Risk Assessment dialogs
  const [riskDialogOpen, setRiskDialogOpen] = React.useState(false);
  const [editingRisk, setEditingRisk] = React.useState<RiskAssessmentDto | null>(null);
  const [deleteRiskDialogOpen, setDeleteRiskDialogOpen] = React.useState(false);
  const [deletingRiskId, setDeletingRiskId] = React.useState<string | null>(null);

  // Method Step dialogs
  const [stepDialogOpen, setStepDialogOpen] = React.useState(false);
  const [editingStep, setEditingStep] = React.useState<MethodStepDto | null>(null);
  const [deleteStepDialogOpen, setDeleteStepDialogOpen] = React.useState(false);
  const [deletingStepId, setDeletingStepId] = React.useState<string | null>(null);

  // Determine if document is editable
  const isEditable = document?.status === "Draft" || document?.status === "Rejected";
  const canSubmitDocument = isEditable && riskAssessments.length > 0 && methodSteps.length > 0;
  const canApproveDocument = document?.status === "PendingReview" && canApprove;

  // Handlers
  const handleDeleteDocument = async () => {
    if (!document) return;
    try {
      await deleteDocument.mutateAsync(document.id);
      toast.success("RAMS document deleted");
      router.push("/rams");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete RAMS document", { description: message });
    }
  };

  const handleSubmit = async () => {
    try {
      await submitDocument.mutateAsync(id);
      toast.success("RAMS document submitted for review");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to submit document", { description: message });
    }
  };

  const handleApprove = async () => {
    try {
      await approveDocument.mutateAsync({ id, comments: approvalComments || undefined });
      toast.success("RAMS document approved");
      setApprovalDialogOpen(false);
      setApprovalComments("");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to approve document", { description: message });
    }
  };

  const handleReject = async () => {
    if (!approvalComments.trim()) {
      toast.error("Please provide rejection comments");
      return;
    }
    try {
      await rejectDocument.mutateAsync({ id, comments: approvalComments });
      toast.success("RAMS document rejected");
      setRejectDialogOpen(false);
      setApprovalComments("");
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to reject document", { description: message });
    }
  };

  const handleDeleteRisk = async () => {
    if (!deletingRiskId) return;
    try {
      await deleteRiskAssessment.mutateAsync({ ramsDocumentId: id, id: deletingRiskId });
      toast.success("Risk assessment deleted");
      setDeleteRiskDialogOpen(false);
      setDeletingRiskId(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete risk assessment", { description: message });
    }
  };

  const handleDeleteStep = async () => {
    if (!deletingStepId) return;
    try {
      await deleteMethodStep.mutateAsync({ ramsDocumentId: id, id: deletingStepId });
      toast.success("Method step deleted");
      setDeleteStepDialogOpen(false);
      setDeletingStepId(null);
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error("Failed to delete method step", { description: message });
    }
  };

  // Risk assessment stats
  const riskStats = React.useMemo(() => {
    return {
      high: riskAssessments.filter((r) => r.residualRiskLevelDisplay === "High").length,
      medium: riskAssessments.filter((r) => r.residualRiskLevelDisplay === "Medium").length,
      low: riskAssessments.filter((r) => r.residualRiskLevelDisplay === "Low").length,
    };
  }, [riskAssessments]);

  // Next sort order for new items
  const nextRiskSortOrder = riskAssessments.length > 0
    ? Math.max(...riskAssessments.map((r) => r.sortOrder)) + 1
    : 0;
  const nextStepNumber = methodSteps.length > 0
    ? Math.max(...methodSteps.map((s) => s.stepNumber)) + 1
    : 1;

  // Loading state
  if (isLoadingDocument) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/rams">
              <ChevronLeft className="mr-1 h-4 w-4" />
              Back to RAMS Documents
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
  if (documentError || !document) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/rams">
              <ChevronLeft className="mr-1 h-4 w-4" />
              Back to RAMS Documents
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load RAMS document. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back button */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/rams">
            <ChevronLeft className="mr-1 h-4 w-4" />
            Back to RAMS Documents
          </Link>
        </Button>
      </div>

      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold tracking-tight">
              {document.projectReference}
            </h1>
            <RamsStatusBadge status={document.status} />
          </div>
          <p className="text-muted-foreground">{document.projectName}</p>
          {document.clientName && (
            <p className="text-sm text-muted-foreground">{document.clientName}</p>
          )}
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {/* Edit button */}
          {isEditable && canEdit && (
            <Button variant="outline" asChild>
              <Link href={`/rams/${id}/edit`}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </Link>
            </Button>
          )}

          {/* Submit button */}
          {canSubmitDocument && canCreate && (
            <Button onClick={handleSubmit} disabled={submitDocument.isPending}>
              {submitDocument.isPending && (
                <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
              )}
              Submit for Review
            </Button>
          )}

          {/* Approval buttons */}
          {canApproveDocument && (
            <>
              <Button onClick={() => setApprovalDialogOpen(true)}>
                Approve
              </Button>
              <Button variant="destructive" onClick={() => setRejectDialogOpen(true)}>
                Reject
              </Button>
            </>
          )}

          {/* Delete button */}
          {isEditable && canDelete && (
            <Button
              variant="destructive"
              onClick={() => setDeleteDocumentDialogOpen(true)}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </Button>
          )}
        </div>
      </div>

      {/* Rejection banner */}
      {document.status === "Rejected" && document.approvalComments && (
        <div className="rounded-lg border border-destructive bg-destructive/10 p-4">
          <div className="flex items-start gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive mt-0.5" />
            <div>
              <p className="font-medium text-destructive">Rejection Reason</p>
              <p className="text-sm text-muted-foreground">{document.approvalComments}</p>
            </div>
          </div>
        </div>
      )}

      {/* Overview Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Project Type
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-lg font-semibold">{document.projectTypeDisplay}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Risk Assessments
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{riskAssessments.length}</p>
            {riskAssessments.length > 0 && (
              <div className="flex gap-2 mt-1">
                {riskStats.high > 0 && (
                  <Badge variant="destructive" className="text-xs">
                    {riskStats.high} High
                  </Badge>
                )}
                {riskStats.medium > 0 && (
                  <Badge className="text-xs bg-yellow-100 text-yellow-800 hover:bg-yellow-100">
                    {riskStats.medium} Med
                  </Badge>
                )}
                {riskStats.low > 0 && (
                  <Badge className="text-xs bg-green-100 text-green-800 hover:bg-green-100">
                    {riskStats.low} Low
                  </Badge>
                )}
              </div>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Method Steps
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{methodSteps.length}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Proposed Start
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-lg font-semibold">
              {document.proposedStartDate
                ? format(new Date(document.proposedStartDate), "PPP")
                : "-"}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="risks">
            Risk Assessments
            <Badge variant="secondary" className="ml-2">
              {riskAssessments.length}
            </Badge>
          </TabsTrigger>
          <TabsTrigger value="steps">
            Method Steps
            <Badge variant="secondary" className="ml-2">
              {methodSteps.length}
            </Badge>
          </TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Project Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div>
                  <p className="text-sm text-muted-foreground">Project Reference</p>
                  <p className="font-medium">{document.projectReference}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Project Name</p>
                  <p className="font-medium">{document.projectName}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Project Type</p>
                  <p className="font-medium">{document.projectTypeDisplay}</p>
                </div>
                {document.clientName && (
                  <div>
                    <p className="text-sm text-muted-foreground">Client</p>
                    <p className="font-medium">{document.clientName}</p>
                  </div>
                )}
                {document.siteName && (
                  <div>
                    <p className="text-sm text-muted-foreground">Site</p>
                    <p className="font-medium">{document.siteName}</p>
                  </div>
                )}
                {document.siteAddress && (
                  <div>
                    <p className="text-sm text-muted-foreground">Site Address</p>
                    <p className="font-medium">{document.siteAddress}</p>
                  </div>
                )}
                {document.areaOfActivity && (
                  <div>
                    <p className="text-sm text-muted-foreground">Area of Activity</p>
                    <p className="font-medium">{document.areaOfActivity}</p>
                  </div>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Dates & Approval</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {document.proposedStartDate && (
                  <div>
                    <p className="text-sm text-muted-foreground">Proposed Start Date</p>
                    <p className="font-medium">
                      {format(new Date(document.proposedStartDate), "PPP")}
                    </p>
                  </div>
                )}
                {document.proposedEndDate && (
                  <div>
                    <p className="text-sm text-muted-foreground">Proposed End Date</p>
                    <p className="font-medium">
                      {format(new Date(document.proposedEndDate), "PPP")}
                    </p>
                  </div>
                )}
                {document.safetyOfficerName && (
                  <div>
                    <p className="text-sm text-muted-foreground">Safety Officer</p>
                    <p className="font-medium">{document.safetyOfficerName}</p>
                  </div>
                )}
                <div>
                  <p className="text-sm text-muted-foreground">Created</p>
                  <p className="font-medium">
                    {format(new Date(document.createdAt), "PPP")}
                  </p>
                </div>
                {document.dateApproved && (
                  <div>
                    <p className="text-sm text-muted-foreground">Approved</p>
                    <p className="font-medium">
                      {format(new Date(document.dateApproved), "PPP")}
                      {document.approvedByName && ` by ${document.approvedByName}`}
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

          {document.methodStatementBody && (
            <Card>
              <CardHeader>
                <CardTitle>Method Statement Overview</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="whitespace-pre-wrap">{document.methodStatementBody}</p>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Risk Assessments Tab */}
        <TabsContent value="risks" className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-medium">Risk Assessments</h3>
            {isEditable && canEdit && (
              <Button
                size="sm"
                onClick={() => {
                  setEditingRisk(null);
                  setRiskDialogOpen(true);
                }}
              >
                <Plus className="mr-2 h-4 w-4" />
                Add Risk Assessment
              </Button>
            )}
          </div>

          {isLoadingRisks ? (
            <div className="space-y-2">
              <Skeleton className="h-16" />
              <Skeleton className="h-16" />
              <Skeleton className="h-16" />
            </div>
          ) : riskAssessments.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center">
                <AlertTriangle className="mx-auto h-12 w-12 text-muted-foreground" />
                <h3 className="mt-4 text-lg font-medium">No Risk Assessments</h3>
                <p className="text-muted-foreground">
                  Add risk assessments to identify hazards and control measures.
                </p>
                {isEditable && canEdit && (
                  <Button
                    className="mt-4"
                    onClick={() => {
                      setEditingRisk(null);
                      setRiskDialogOpen(true);
                    }}
                  >
                    <Plus className="mr-2 h-4 w-4" />
                    Add Risk Assessment
                  </Button>
                )}
              </CardContent>
            </Card>
          ) : (
            <Card>
              <SortableRiskAssessmentTable
                ramsDocumentId={id}
                riskAssessments={riskAssessments}
                canEdit={isEditable && canEdit}
                onEdit={(risk) => {
                  setEditingRisk(risk);
                  setRiskDialogOpen(true);
                }}
                onDelete={(riskId) => {
                  setDeletingRiskId(riskId);
                  setDeleteRiskDialogOpen(true);
                }}
              />
            </Card>
          )}
        </TabsContent>

        {/* Method Steps Tab */}
        <TabsContent value="steps" className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-medium">Method Steps</h3>
            {isEditable && canEdit && (
              <Button
                size="sm"
                onClick={() => {
                  setEditingStep(null);
                  setStepDialogOpen(true);
                }}
              >
                <Plus className="mr-2 h-4 w-4" />
                Add Step
              </Button>
            )}
          </div>

          {isLoadingSteps ? (
            <div className="space-y-2">
              <Skeleton className="h-20" />
              <Skeleton className="h-20" />
              <Skeleton className="h-20" />
            </div>
          ) : methodSteps.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center">
                <ListOrdered className="mx-auto h-12 w-12 text-muted-foreground" />
                <h3 className="mt-4 text-lg font-medium">No Method Steps</h3>
                <p className="text-muted-foreground">
                  Add method steps to define the work procedure.
                </p>
                {isEditable && canEdit && (
                  <Button
                    className="mt-4"
                    onClick={() => {
                      setEditingStep(null);
                      setStepDialogOpen(true);
                    }}
                  >
                    <Plus className="mr-2 h-4 w-4" />
                    Add Method Step
                  </Button>
                )}
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="p-0">
                <SortableMethodStepList
                  ramsDocumentId={id}
                  methodSteps={methodSteps}
                  canEdit={isEditable && canEdit}
                  onEdit={(step) => {
                    setEditingStep(step);
                    setStepDialogOpen(true);
                  }}
                  onDelete={(stepId) => {
                    setDeletingStepId(stepId);
                    setDeleteStepDialogOpen(true);
                  }}
                />
              </CardContent>
            </Card>
          )}
        </TabsContent>
      </Tabs>

      {/* Dialogs */}
      <DeleteConfirmationDialog
        open={deleteDocumentDialogOpen}
        onOpenChange={setDeleteDocumentDialogOpen}
        title="Delete RAMS Document"
        description={`Are you sure you want to delete RAMS document "${document.projectReference}"? This action cannot be undone.`}
        onConfirm={handleDeleteDocument}
        isLoading={deleteDocument.isPending}
      />

      {/* Approval Dialog */}
      <Dialog open={approvalDialogOpen} onOpenChange={setApprovalDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Approve RAMS Document</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <p>Are you sure you want to approve this RAMS document?</p>
            <div>
              <label className="text-sm font-medium">Comments (optional)</label>
              <Textarea
                className="mt-2"
                placeholder="Add any approval comments..."
                value={approvalComments}
                onChange={(e) => setApprovalComments(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setApprovalDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleApprove} disabled={approveDocument.isPending}>
              {approveDocument.isPending && (
                <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
              )}
              Approve
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onOpenChange={setRejectDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reject RAMS Document</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <p>Please provide a reason for rejecting this RAMS document.</p>
            <div>
              <label className="text-sm font-medium">
                Rejection Reason <span className="text-destructive">*</span>
              </label>
              <Textarea
                className="mt-2"
                placeholder="Enter rejection reason..."
                value={approvalComments}
                onChange={(e) => setApprovalComments(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setRejectDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleReject}
              disabled={rejectDocument.isPending}
            >
              {rejectDocument.isPending && (
                <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-r-transparent" />
              )}
              Reject
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Risk Assessment Dialog */}
      <RiskAssessmentDialog
        open={riskDialogOpen}
        onOpenChange={(open) => {
          setRiskDialogOpen(open);
          if (!open) setEditingRisk(null);
        }}
        ramsDocumentId={id}
        riskAssessment={editingRisk}
        nextSortOrder={nextRiskSortOrder}
        projectType={document.projectType}
      />

      {/* Delete Risk Dialog */}
      <DeleteConfirmationDialog
        open={deleteRiskDialogOpen}
        onOpenChange={(open) => {
          setDeleteRiskDialogOpen(open);
          if (!open) setDeletingRiskId(null);
        }}
        title="Delete Risk Assessment"
        description="Are you sure you want to delete this risk assessment? This action cannot be undone."
        onConfirm={handleDeleteRisk}
        isLoading={deleteRiskAssessment.isPending}
      />

      {/* Method Step Dialog */}
      <MethodStepDialog
        open={stepDialogOpen}
        onOpenChange={(open) => {
          setStepDialogOpen(open);
          if (!open) setEditingStep(null);
        }}
        ramsDocumentId={id}
        methodStep={editingStep}
        riskAssessments={riskAssessments}
        nextStepNumber={nextStepNumber}
      />

      {/* Delete Step Dialog */}
      <DeleteConfirmationDialog
        open={deleteStepDialogOpen}
        onOpenChange={(open) => {
          setDeleteStepDialogOpen(open);
          if (!open) setDeletingStepId(null);
        }}
        title="Delete Method Step"
        description="Are you sure you want to delete this method step? This action cannot be undone."
        onConfirm={handleDeleteStep}
        isLoading={deleteMethodStep.isPending}
      />
    </div>
  );
}
