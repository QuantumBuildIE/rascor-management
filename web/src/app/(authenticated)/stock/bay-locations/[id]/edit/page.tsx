"use client";

import { use } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { BayLocationForm } from "@/components/stock/bay-location-form";
import { useBayLocation } from "@/lib/api/stock/use-bay-locations";

interface EditBayLocationPageProps {
  params: Promise<{ id: string }>;
}

export default function EditBayLocationPage({ params }: EditBayLocationPageProps) {
  const { id } = use(params);
  const router = useRouter();
  const { data: bayLocation, isLoading, error } = useBayLocation(id);

  const handleSuccess = () => {
    router.push("/stock/bay-locations");
  };

  const handleCancel = () => {
    router.push("/stock/bay-locations");
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/bay-locations">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Bay Locations
            </Link>
          </Button>
        </div>

        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Edit Bay Location
          </h1>
          <p className="text-muted-foreground">Update bay location details</p>
        </div>

        <Card className="max-w-2xl">
          <CardHeader>
            <CardTitle>Bay Location Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="flex items-center gap-3 rounded-md border p-4">
              <Skeleton className="h-5 w-5" />
              <div className="space-y-1">
                <Skeleton className="h-4 w-16" />
                <Skeleton className="h-3 w-48" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error || !bayLocation) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/bay-locations">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Bay Locations
            </Link>
          </Button>
        </div>

        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Bay location not found or failed to load.
          </p>
          <Button variant="outline" className="mt-4" asChild>
            <Link href="/stock/bay-locations">Return to Bay Locations</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/stock/bay-locations">
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Bay Locations
          </Link>
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit Bay Location</h1>
        <p className="text-muted-foreground">
          Update details for {bayLocation.bayCode}
        </p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Bay Location Details</CardTitle>
        </CardHeader>
        <CardContent>
          <BayLocationForm
            bayLocation={bayLocation}
            onSuccess={handleSuccess}
            onCancel={handleCancel}
          />
        </CardContent>
      </Card>
    </div>
  );
}

function ChevronLeftIcon({ className }: { className?: string }) {
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
        d="M15 19l-7-7 7-7"
      />
    </svg>
  );
}
