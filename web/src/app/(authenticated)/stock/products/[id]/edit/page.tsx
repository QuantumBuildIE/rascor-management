"use client";

import { use } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ProductForm } from "@/components/stock/product-form";
import { useProduct } from "@/lib/api/stock/use-products";
import { Skeleton } from "@/components/ui/skeleton";

interface EditProductPageProps {
  params: Promise<{ id: string }>;
}

export default function EditProductPage({ params }: EditProductPageProps) {
  const { id } = use(params);
  const router = useRouter();
  const { data: product, isLoading, error } = useProduct(id);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <nav className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
            <Link href="/stock/products" className="hover:text-foreground">
              Products
            </Link>
            <span>/</span>
            <span className="text-foreground">Edit Product</span>
          </nav>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-5 w-64 mt-2" />
        </div>
        <div className="max-w-2xl">
          <div className="rounded-lg border bg-card p-6 space-y-6">
            <div className="grid gap-6 sm:grid-cols-2">
              <Skeleton className="h-10" />
              <Skeleton className="h-10" />
            </div>
            <div className="grid gap-6 sm:grid-cols-2">
              <Skeleton className="h-10" />
              <Skeleton className="h-10" />
            </div>
            <div className="grid gap-6 sm:grid-cols-2">
              <Skeleton className="h-10" />
              <Skeleton className="h-10" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="space-y-6">
        <div>
          <nav className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
            <Link href="/stock/products" className="hover:text-foreground">
              Products
            </Link>
            <span>/</span>
            <span className="text-foreground">Edit Product</span>
          </nav>
          <h1 className="text-2xl font-semibold tracking-tight">Edit Product</h1>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">
            {error ? "Failed to load product. Please try again." : "Product not found."}
          </p>
          <Link
            href="/stock/products"
            className="mt-4 inline-block text-sm text-muted-foreground hover:text-foreground"
          >
            Back to Products
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <nav className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
          <Link href="/stock/products" className="hover:text-foreground">
            Products
          </Link>
          <span>/</span>
          <span className="text-foreground">Edit Product</span>
        </nav>
        <h1 className="text-2xl font-semibold tracking-tight">Edit Product</h1>
        <p className="text-muted-foreground">
          Update {product.productName} ({product.productCode})
        </p>
      </div>

      <div className="max-w-2xl">
        <div className="rounded-lg border bg-card p-6">
          <ProductForm
            product={product}
            onSuccess={() => router.push("/stock/products")}
            onCancel={() => router.push("/stock/products")}
          />
        </div>
      </div>
    </div>
  );
}
