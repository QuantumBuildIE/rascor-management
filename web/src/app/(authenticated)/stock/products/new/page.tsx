"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ProductForm } from "@/components/stock/product-form";

export default function NewProductPage() {
  const router = useRouter();

  return (
    <div className="space-y-6">
      <div>
        <nav className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
          <Link href="/stock/products" className="hover:text-foreground">
            Products
          </Link>
          <span>/</span>
          <span className="text-foreground">Add Product</span>
        </nav>
        <h1 className="text-2xl font-semibold tracking-tight">Add Product</h1>
        <p className="text-muted-foreground">
          Create a new product in your catalog
        </p>
      </div>

      <div className="max-w-2xl">
        <div className="rounded-lg border bg-card p-4 sm:p-6">
          <ProductForm
            onSuccess={() => router.push("/stock/products")}
            onCancel={() => router.push("/stock/products")}
          />
        </div>
      </div>
    </div>
  );
}
