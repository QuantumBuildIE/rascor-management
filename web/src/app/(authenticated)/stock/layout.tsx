"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import Link from "next/link";
import { usePermission } from "@/lib/auth/use-auth";
import { cn } from "@/lib/utils";

const stockNavItems = [
  { href: "/stock", label: "Dashboard", exact: true },
  { href: "/stock/products", label: "Products" },
  { href: "/stock/categories", label: "Categories" },
  { href: "/stock/suppliers", label: "Suppliers" },
  { href: "/stock/product-kits", label: "Product Kits" },
  { href: "/stock/levels", label: "Stock Levels" },
  { href: "/stock/bay-locations", label: "Bay Locations" },
  { href: "/stock/orders", label: "Orders" },
  { href: "/stock/purchase-orders", label: "Purchase Orders" },
  { href: "/stock/goods-receipts", label: "Goods Receipts" },
  { href: "/stock/stocktakes", label: "Stocktakes" },
  { href: "/stock/reports", label: "Reports" },
];

export default function StockLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const hasViewPermission = usePermission("StockManagement.View");

  useEffect(() => {
    if (!hasViewPermission) {
      router.replace("/dashboard");
    }
  }, [hasViewPermission, router]);

  if (!hasViewPermission) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="text-muted-foreground">
          You do not have permission to view Stock Management.
        </div>
      </div>
    );
  }

  const isActive = (href: string, exact?: boolean) => {
    if (exact) {
      return pathname === href;
    }
    return pathname.startsWith(href);
  };

  return (
    <div className="space-y-6">
      <nav className="border-b bg-background -mx-4 px-4 sm:mx-0 sm:px-0">
        <div className="flex h-10 items-center gap-4 overflow-x-auto sm:gap-6 scrollbar-hide">
          {stockNavItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "relative flex h-10 min-h-[44px] items-center px-1 text-sm font-medium transition-colors hover:text-foreground whitespace-nowrap sm:min-h-0 sm:px-0",
                isActive(item.href, item.exact)
                  ? "text-foreground"
                  : "text-muted-foreground"
              )}
            >
              {item.label}
              {isActive(item.href, item.exact) && (
                <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary" />
              )}
            </Link>
          ))}
        </div>
      </nav>
      <div>{children}</div>
    </div>
  );
}
