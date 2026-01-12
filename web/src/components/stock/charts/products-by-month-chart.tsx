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
import type { ProductValueByMonth } from "@/types/stock";

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

interface ProductsByMonthChartProps {
  data: ProductValueByMonth[] | undefined;
  isLoading: boolean;
}

export function ProductsByMonthChart({
  data,
  isLoading,
}: ProductsByMonthChartProps) {
  const { chartData, productNames } = useMemo(() => {
    if (!data || data.length === 0) {
      return { chartData: [], productNames: [] };
    }

    // Get unique months and products
    const months = [...new Set(data.map((d) => d.month))];
    const products = [...new Set(data.map((d) => d.productName))];

    // Transform data for recharts - each month becomes a data point with product values
    const transformed = months.map((month) => {
      const monthData: Record<string, string | number> = { month };
      products.forEach((product) => {
        const record = data.find(
          (d) => d.month === month && d.productName === product
        );
        monthData[product] = record?.value ?? 0;
      });
      return monthData;
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
          dataKey="month"
          tick={{ fontSize: 12 }}
          className="text-muted-foreground"
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
