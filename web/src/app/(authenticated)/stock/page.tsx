"use client";

import Link from "next/link";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useAllProducts } from "@/lib/api/stock/use-products";
import { useLowStockLevels } from "@/lib/api/stock/use-stock-levels";
import { useStockOrders } from "@/lib/api/stock/use-stock-orders";
import { usePurchaseOrders } from "@/lib/api/stock/use-purchase-orders";
import {
  useProductsByMonth,
  useProductsBySite,
  useProductsByWeek,
} from "@/lib/api/stock/use-stock-reports";
import {
  ProductsByMonthChart,
  ProductsBySiteChart,
  ProductsByWeekChart,
} from "@/components/stock/charts";

export default function StockDashboardPage() {
  const { data: products, isLoading: productsLoading } = useAllProducts();
  const { data: lowStockItems, isLoading: lowStockLoading } = useLowStockLevels();
  const { data: stockOrders, isLoading: ordersLoading } = useStockOrders();
  const { data: purchaseOrders, isLoading: posLoading } = usePurchaseOrders();

  // Report data hooks
  const { data: productsByMonth, isLoading: monthLoading } = useProductsByMonth(4, 10);
  const { data: productsBySite, isLoading: siteLoading } = useProductsBySite(10);
  const { data: productsByWeek, isLoading: weekLoading } = useProductsByWeek(12, 10);

  const isLoading = productsLoading || lowStockLoading || ordersLoading || posLoading;

  const totalProducts = products?.length ?? 0;
  const lowStockCount = lowStockItems?.length ?? 0;
  const pendingOrders = stockOrders?.filter((o) => o.status === "PendingApproval").length ?? 0;
  const openPurchaseOrders =
    purchaseOrders?.filter(
      (po) => po.status === "Draft" || po.status === "Confirmed" || po.status === "PartiallyReceived"
    ).length ?? 0;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Stock Dashboard</h1>
        <p className="text-muted-foreground">
          Overview of your stock management
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Total Products</CardDescription>
            <CardTitle className="text-3xl">
              {isLoading ? (
                <span className="animate-pulse text-muted-foreground">...</span>
              ) : (
                totalProducts
              )}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Link
              href="/stock/products"
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              View all products
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Low Stock Items</CardDescription>
            <CardTitle
              className={`text-3xl ${lowStockCount > 0 ? "text-amber-600" : ""}`}
            >
              {isLoading ? (
                <span className="animate-pulse text-muted-foreground">...</span>
              ) : (
                lowStockCount
              )}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Link
              href="/stock/levels"
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              View stock levels
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Pending Orders</CardDescription>
            <CardTitle className="text-3xl">
              {isLoading ? (
                <span className="animate-pulse text-muted-foreground">...</span>
              ) : (
                pendingOrders
              )}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Link
              href="/stock/orders"
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              View stock orders
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Open Purchase Orders</CardDescription>
            <CardTitle className="text-3xl">
              {isLoading ? (
                <span className="animate-pulse text-muted-foreground">...</span>
              ) : (
                openPurchaseOrders
              )}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Link
              href="/stock/purchase-orders"
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              View purchase orders
            </Link>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
          <CardDescription>Common stock management tasks</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Button asChild className="w-full sm:w-auto">
            <Link href="/stock/orders/new">New Stock Order</Link>
          </Button>
          <Button asChild variant="outline" className="w-full sm:w-auto">
            <Link href="/stock/purchase-orders/new">New Purchase Order</Link>
          </Button>
          <Button asChild variant="outline" className="w-full sm:w-auto">
            <Link href="/stock/goods-receipts/new">Receive Goods</Link>
          </Button>
        </CardContent>
      </Card>

      {/* Reports Section */}
      <div>
        <h2 className="text-xl font-semibold tracking-tight mb-4">Reports</h2>
        <p className="text-muted-foreground mb-4">
          Product value trends from collected stock orders
        </p>

        <div className="grid gap-4 lg:grid-cols-2">
          {/* Products by Month Chart - Full width on smaller screens */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Top Products by Month</CardTitle>
              <CardDescription>
                Product values over the last 4 months
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ProductsByMonthChart data={productsByMonth} isLoading={monthLoading} />
            </CardContent>
          </Card>

          {/* Products by Site Chart */}
          <Card>
            <CardHeader>
              <CardTitle>Top Products by Site</CardTitle>
              <CardDescription>
                Product values across all sites
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ProductsBySiteChart data={productsBySite} isLoading={siteLoading} />
            </CardContent>
          </Card>

          {/* Products by Week Chart */}
          <Card>
            <CardHeader>
              <CardTitle>Weekly Product Trends</CardTitle>
              <CardDescription>
                Product values over the last 12 weeks
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ProductsByWeekChart data={productsByWeek} isLoading={weekLoading} />
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Recent Activity Placeholder */}
      <Card>
        <CardHeader>
          <CardTitle>Recent Activity</CardTitle>
          <CardDescription>Latest stock movements and orders</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Recent activity feed coming soon...
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
