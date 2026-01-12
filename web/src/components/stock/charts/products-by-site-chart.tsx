"use client";

import { useMemo } from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import type { ProductValueBySite } from "@/types/stock";

const COLORS = [
  "#3B82F6", // blue
  "#F59E0B", // amber
  "#10B981", // emerald
  "#EF4444", // red
  "#8B5CF6", // violet
  "#EC4899", // pink
  "#06B6D4", // cyan
  "#84CC16", // lime
  "#F97316", // orange
  "#6366F1", // indigo
];

interface ProductsBySiteChartProps {
  data: ProductValueBySite[] | undefined;
  isLoading: boolean;
}

export function ProductsBySiteChart({
  data,
  isLoading,
}: ProductsBySiteChartProps) {
  const { chartData, productNames } = useMemo(() => {
    if (!data || data.length === 0) {
      return { chartData: [], productNames: [] };
    }

    // Get unique sites and products
    const sites = [...new Set(data.map((d) => d.siteName))];
    const products = [...new Set(data.map((d) => d.productName))];

    // Transform data for recharts - each site becomes a data point with product values
    const transformed = sites.map((site) => {
      const siteData: Record<string, string | number> = { site };
      products.forEach((product) => {
        const record = data.find(
          (d) => d.siteName === site && d.productName === product
        );
        siteData[product] = record?.value ?? 0;
      });
      return siteData;
    });

    return { chartData: transformed, productNames: products };
  }, [data]);

  if (isLoading) {
    return (
      <div className="h-[300px] w-full animate-pulse bg-muted rounded-md flex items-center justify-center">
        <span className="text-muted-foreground">Loading chart...</span>
      </div>
    );
  }

  if (chartData.length === 0) {
    return (
      <div className="h-[300px] w-full flex items-center justify-center text-muted-foreground">
        No data available
      </div>
    );
  }

  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart
        data={chartData}
        margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
      >
        <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
        <XAxis
          dataKey="site"
          tick={{ fontSize: 12 }}
          className="text-muted-foreground"
          interval={0}
          angle={-45}
          textAnchor="end"
          height={80}
        />
        <YAxis
          tick={{ fontSize: 12 }}
          tickFormatter={(value) => `£${value.toLocaleString()}`}
          className="text-muted-foreground"
        />
        <Tooltip
          formatter={(value) => {
            const numValue = typeof value === "number" ? value : 0;
            return [`£${numValue.toLocaleString()}`, ""];
          }}
          contentStyle={{
            backgroundColor: "hsl(var(--popover))",
            border: "1px solid hsl(var(--border))",
            borderRadius: "var(--radius)",
          }}
        />
        <Legend wrapperStyle={{ fontSize: 12 }} />
        {productNames.map((product, index) => (
          <Bar
            key={product}
            dataKey={product}
            fill={COLORS[index % COLORS.length]}
            name={product}
          />
        ))}
      </BarChart>
    </ResponsiveContainer>
  );
}
