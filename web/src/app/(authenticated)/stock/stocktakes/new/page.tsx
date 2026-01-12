"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { StocktakeForm } from "@/components/stock/stocktake-form";

export default function NewStocktakePage() {
  const router = useRouter();

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild>
          <Link href="/stock/stocktakes">
            <ChevronLeftIcon className="mr-1 h-4 w-4" />
            Back to Stocktakes
          </Link>
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New Stocktake</h1>
        <p className="text-muted-foreground">
          Create a new stock count for a location
        </p>
      </div>

      <div className="max-w-2xl">
        <StocktakeForm
          onSuccess={(stocktake) => {
            router.push(`/stock/stocktakes/${stocktake.id}`);
          }}
          onCancel={() => router.push("/stock/stocktakes")}
        />
      </div>
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
