"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ProposalForm } from "@/components/proposals/proposal-form";
import { useCreateProposal, type CreateProposalDto } from "@/lib/api/proposals";
import { toast } from "sonner";

export default function NewProposalPage() {
  const router = useRouter();
  const createProposal = useCreateProposal();

  const handleSubmit = async (data: CreateProposalDto) => {
    try {
      const proposal = await createProposal.mutateAsync(data);
      toast.success("Proposal created successfully");
      // Redirect to edit page to add sections and line items
      router.push(`/proposals/${proposal.id}/edit`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to create proposal";
      toast.error("Failed to create proposal", { description: message });
      throw error;
    }
  };

  const handleCancel = () => {
    router.push("/proposals");
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/proposals">
            <ChevronLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">New Proposal</h1>
          <p className="text-muted-foreground">
            Create a new proposal for a client project
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Proposal Details</CardTitle>
          <CardDescription>
            Fill in the basic proposal information. You can add sections and line items after saving.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ProposalForm
            onSubmit={handleSubmit}
            onCancel={handleCancel}
            isLoading={createProposal.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
