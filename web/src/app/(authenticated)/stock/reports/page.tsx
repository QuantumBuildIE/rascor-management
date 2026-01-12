"use client";

import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { usePermission } from "@/lib/auth/use-auth";

export default function StockReportsPage() {
  const hasViewCostingsPermission = usePermission("StockManagement.ViewCostings");

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Stock Reports</h1>
        <p className="text-muted-foreground">
          Generate and view stock reports and analytics
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* Stock Valuation Report */}
        {hasViewCostingsPermission && (
          <Link href="/stock/reports/valuation">
            <Card className="h-full transition-all hover:shadow-md cursor-pointer">
              <CardHeader>
                <CardTitle>Stock Valuation Report</CardTitle>
                <CardDescription>
                  View current stock value by product and location
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ul className="text-sm text-muted-foreground space-y-1">
                  <li>• Current stock values</li>
                  <li>• Filter by location or category</li>
                  <li>• Breakdown by product</li>
                  <li>• Printable format</li>
                </ul>
              </CardContent>
            </Card>
          </Link>
        )}

        {/* Placeholder for future reports */}
        <Card className="h-full opacity-50">
          <CardHeader>
            <CardTitle>Low Stock Report</CardTitle>
            <CardDescription>
              Products below reorder levels
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground italic">
              Coming soon...
            </p>
          </CardContent>
        </Card>

        <Card className="h-full opacity-50">
          <CardHeader>
            <CardTitle>Stock Movement Report</CardTitle>
            <CardDescription>
              Track stock movements over time
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground italic">
              Coming soon...
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
