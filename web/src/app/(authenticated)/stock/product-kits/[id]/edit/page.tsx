"use client";

import { useRouter, useParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ProductKitForm } from "@/components/stock/product-kit-form";
import { useProductKit } from "@/lib/api/stock/use-product-kits";

export default function EditProductKitPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: productKit, isLoading, error } = useProductKit(id);

  const handleSuccess = () => {
    router.push(`/stock/product-kits/${id}`);
  };

  const handleCancel = () => {
    router.push(`/stock/product-kits/${id}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/product-kits">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Product Kits
            </Link>
          </Button>
        </div>
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner className="h-8 w-8" />
        </div>
      </div>
    );
  }

  if (error || !productKit) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link href="/stock/product-kits">
              <ChevronLeftIcon className="mr-1 h-4 w-4" />
              Back to Product Kits
            </Link>
          </Button>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            Failed to load product kit. Please try again.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/stock/product-kits/${id}`}>
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Kit Details
          </Link>
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Edit Product Kit</h1>
        <p className="text-muted-foreground">
          Update kit details for {productKit.kitCode}
        </p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Kit Details</CardTitle>
        </CardHeader>
        <CardContent>
          <ProductKitForm
            productKit={productKit}
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

function LoadingSpinner({ className }: { className?: string }) {
  return (
    <svg
      className={`animate-spin ${className}`}
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );
}
