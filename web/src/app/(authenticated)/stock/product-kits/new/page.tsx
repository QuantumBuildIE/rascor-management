"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ProductKitForm } from "@/components/stock/product-kit-form";

export default function NewProductKitPage() {
  const router = useRouter();

  const handleSuccess = (id: string) => {
    // Redirect to edit page so user can add items
    router.push(`/stock/product-kits/${id}/edit`);
  };

  const handleCancel = () => {
    router.push("/stock/product-kits");
  };

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

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Add Product Kit</h1>
        <p className="text-muted-foreground">
          Create a new product kit. After creating, you can add items to the kit.
        </p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Kit Details</CardTitle>
        </CardHeader>
        <CardContent>
          <ProductKitForm onSuccess={handleSuccess} onCancel={handleCancel} />
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
