"use client";

import * as React from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { ChevronLeft, AlertTriangle, FileEdit } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ProposalForm } from "@/components/proposals/proposal-form";
import {
  useProposal,
  useUpdateProposal,
  useCreateRevision,
  getStatusColor,
  getStatusDisplayName,
  type UpdateProposalDto,
  type ProposalStatus,
} from "@/lib/api/proposals";
import { toast } from "sonner";

// Statuses that allow editing
const EDITABLE_STATUSES: ProposalStatus[] = ["Draft", "Rejected"];

// Statuses that allow creating a revision
const REVISABLE_STATUSES: ProposalStatus[] = ["Rejected", "Approved", "Won", "Lost"];

export default function EditProposalPage() {
  const params = useParams();
  const router = useRouter();
  const proposalId = params.id as string;

  const { data: proposal, isLoading, error } = useProposal(proposalId);
  const updateProposal = useUpdateProposal();
  const createRevision = useCreateRevision();

  const isEditable = proposal ? EDITABLE_STATUSES.includes(proposal.status) : false;
  const canRevise = proposal ? REVISABLE_STATUSES.includes(proposal.status) : false;

  const handleSubmit = async (data: UpdateProposalDto) => {
    if (!proposal) return;

    try {
      await updateProposal.mutateAsync({ id: proposalId, data });
      toast.success("Proposal updated successfully");
      router.push(`/proposals/${proposalId}`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to update proposal";
      toast.error("Failed to update proposal", { description: message });
      throw error;
    }
  };

  const handleCancel = () => {
    router.push(`/proposals/${proposalId}`);
  };

  const handleCreateRevision = async () => {
    if (!proposal) return;

    try {
      const revision = await createRevision.mutateAsync({ id: proposalId });
      toast.success("Revision created successfully", {
        description: `New revision: ${revision.proposalNumber} v${revision.version}`,
      });
      router.push(`/proposals/${revision.id}/edit`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to create revision";
      toast.error("Failed to create revision", { description: message });
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/proposals">
              <ChevronLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <Skeleton className="h-8 w-48" />
            <Skeleton className="h-4 w-64 mt-2" />
          </div>
        </div>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-32" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error || !proposal) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/proposals">
              <ChevronLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">Edit Proposal</h1>
            <p className="text-muted-foreground">Proposal not found</p>
          </div>
        </div>
        <Card>
          <CardContent className="py-8">
            <div className="text-center">
              <p className="text-destructive">
                Failed to load proposal. The proposal may have been deleted.
              </p>
              <Button className="mt-4" asChild>
                <Link href="/proposals">Back to Proposals</Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href={`/proposals/${proposalId}`}>
              <ChevronLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-semibold tracking-tight">Edit Proposal</h1>
              <Badge className={getStatusColor(proposal.status)}>
                {getStatusDisplayName(proposal.status)}
              </Badge>
            </div>
            <p className="text-muted-foreground">
              {proposal.proposalNumber} v{proposal.version} - {proposal.projectName}
            </p>
          </div>
        </div>
      </div>

      {/* Status Warning for non-editable proposals */}
      {!isEditable && (
        <Card className="border-amber-200 bg-amber-50 dark:border-amber-900 dark:bg-amber-950/20">
          <CardContent className="py-4">
            <div className="flex items-start gap-4">
              <AlertTriangle className="h-5 w-5 text-amber-600 shrink-0 mt-0.5" />
              <div className="flex-1 space-y-2">
                <div>
                  <h4 className="text-sm font-medium text-amber-800 dark:text-amber-200">
                    Proposal cannot be edited
                  </h4>
                  <p className="text-sm text-amber-700 dark:text-amber-300">
                    This proposal has status &quot;{getStatusDisplayName(proposal.status)}&quot; and cannot be modified.
                  </p>
                </div>
                {canRevise && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleCreateRevision}
                    disabled={createRevision.isPending}
                    className="border-amber-300 hover:bg-amber-100 dark:border-amber-700 dark:hover:bg-amber-900/40"
                  >
                    <FileEdit className="mr-2 h-4 w-4" />
                    {createRevision.isPending ? "Creating..." : "Create a Revision"}
                  </Button>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Proposal Details</CardTitle>
          <CardDescription>
            {isEditable ? (
              "Update the proposal information below."
            ) : (
              "Viewing proposal details. This proposal cannot be edited in its current status."
            )}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isEditable ? (
            <ProposalForm
              proposal={proposal}
              onSubmit={handleSubmit}
              onCancel={handleCancel}
              isLoading={updateProposal.isPending}
            />
          ) : (
            <div className="space-y-6">
              {/* Read-only display of proposal details */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Client Info */}
                <div className="space-y-4">
                  <h3 className="text-lg font-medium">Client</h3>
                  <div className="space-y-2">
                    <div>
                      <span className="text-sm text-muted-foreground">Company</span>
                      <p className="font-medium">{proposal.companyName}</p>
                    </div>
                    {proposal.primaryContactName && (
                      <div>
                        <span className="text-sm text-muted-foreground">Primary Contact</span>
                        <p className="font-medium">{proposal.primaryContactName}</p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Project Info */}
                <div className="space-y-4">
                  <h3 className="text-lg font-medium">Project</h3>
                  <div className="space-y-2">
                    <div>
                      <span className="text-sm text-muted-foreground">Project Name</span>
                      <p className="font-medium">{proposal.projectName}</p>
                    </div>
                    {proposal.projectAddress && (
                      <div>
                        <span className="text-sm text-muted-foreground">Address</span>
                        <p>{proposal.projectAddress}</p>
                      </div>
                    )}
                    {proposal.projectDescription && (
                      <div>
                        <span className="text-sm text-muted-foreground">Description</span>
                        <p>{proposal.projectDescription}</p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Dates */}
                <div className="space-y-4">
                  <h3 className="text-lg font-medium">Dates</h3>
                  <div className="space-y-2">
                    <div>
                      <span className="text-sm text-muted-foreground">Proposal Date</span>
                      <p className="font-medium">
                        {new Date(proposal.proposalDate).toLocaleDateString()}
                      </p>
                    </div>
                    {proposal.validUntilDate && (
                      <div>
                        <span className="text-sm text-muted-foreground">Valid Until</span>
                        <p className="font-medium">
                          {new Date(proposal.validUntilDate).toLocaleDateString()}
                        </p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Pricing */}
                <div className="space-y-4">
                  <h3 className="text-lg font-medium">Pricing</h3>
                  <div className="space-y-2">
                    <div>
                      <span className="text-sm text-muted-foreground">Currency</span>
                      <p className="font-medium">{proposal.currency}</p>
                    </div>
                    <div>
                      <span className="text-sm text-muted-foreground">VAT Rate</span>
                      <p className="font-medium">{proposal.vatRate}%</p>
                    </div>
                    {proposal.discountPercent > 0 && (
                      <div>
                        <span className="text-sm text-muted-foreground">Discount</span>
                        <p className="font-medium">{proposal.discountPercent}%</p>
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Terms */}
              {(proposal.paymentTerms || proposal.termsAndConditions) && (
                <div className="space-y-4 border-t pt-6">
                  <h3 className="text-lg font-medium">Terms</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {proposal.paymentTerms && (
                      <div>
                        <span className="text-sm text-muted-foreground">Payment Terms</span>
                        <p>{proposal.paymentTerms}</p>
                      </div>
                    )}
                    {proposal.termsAndConditions && (
                      <div className="md:col-span-2">
                        <span className="text-sm text-muted-foreground">Terms & Conditions</span>
                        <p className="whitespace-pre-wrap">{proposal.termsAndConditions}</p>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* Notes */}
              {proposal.notes && (
                <div className="space-y-4 border-t pt-6">
                  <h3 className="text-lg font-medium">Notes</h3>
                  <p className="whitespace-pre-wrap text-muted-foreground">{proposal.notes}</p>
                </div>
              )}

              {/* Actions */}
              <div className="flex justify-end gap-3 border-t pt-6">
                <Button variant="outline" asChild>
                  <Link href={`/proposals/${proposalId}`}>Back to Proposal</Link>
                </Button>
                {canRevise && (
                  <Button
                    onClick={handleCreateRevision}
                    disabled={createRevision.isPending}
                  >
                    <FileEdit className="mr-2 h-4 w-4" />
                    {createRevision.isPending ? "Creating..." : "Create Revision"}
                  </Button>
                )}
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
