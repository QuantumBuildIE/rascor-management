"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { RamsForm } from "@/components/rams/rams-form";
import { useCreateRamsDocument, type CreateRamsDocumentDto } from "@/lib/api/rams";
import { toast } from "sonner";

export default function NewRamsDocumentPage() {
  const router = useRouter();
  const createRams = useCreateRamsDocument();

  const handleSubmit = async (data: CreateRamsDocumentDto) => {
    try {
      const rams = await createRams.mutateAsync(data);
      toast.success("RAMS document created successfully");
      // Redirect to the view page
      router.push(`/rams/${rams.id}`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to create RAMS document";
      toast.error("Failed to create RAMS document", { description: message });
      throw error;
    }
  };

  const handleCancel = () => {
    router.push("/rams");
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/rams">
            <ChevronLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">New RAMS Document</h1>
          <p className="text-muted-foreground">
            Create a new Risk Assessment and Method Statement
          </p>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>RAMS Details</CardTitle>
              <CardDescription>
                Fill in the basic information. You can add risk assessments and method steps after saving.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <RamsForm
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                isLoading={createRams.isPending}
              />
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">About RAMS Documents</CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground space-y-3">
              <p>
                A RAMS document combines a <strong className="text-foreground">Risk Assessment</strong> and{" "}
                <strong className="text-foreground">Method Statement</strong> to document how work will be
                carried out safely.
              </p>
              <div>
                <p className="font-medium text-foreground mb-2">After creating:</p>
                <ul className="list-disc list-inside space-y-1">
                  <li>Add Risk Assessments for each hazard</li>
                  <li>Add Method Steps for the procedure</li>
                  <li>Submit for review and approval</li>
                  <li>Generate PDF for distribution</li>
                </ul>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base">Project Types</CardTitle>
            </CardHeader>
            <CardContent className="text-sm text-muted-foreground space-y-2">
              <div>
                <strong className="text-foreground">Remedial Injection:</strong>
                <p>Waterproofing injection works</p>
              </div>
              <div>
                <strong className="text-foreground">RASCOtank New Build:</strong>
                <p>New tank lining installation</p>
              </div>
              <div>
                <strong className="text-foreground">Car Park Coating:</strong>
                <p>Deck coating applications</p>
              </div>
              <div>
                <strong className="text-foreground">Ground Gas Barrier:</strong>
                <p>Gas membrane installation</p>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
