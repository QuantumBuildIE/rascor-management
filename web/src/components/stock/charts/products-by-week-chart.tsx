"use client";

import { useMemo } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import { format, parseISO } from "date-fns";
import type { ProductValueByWeek } from "@/types/stock";

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

interface ProductsByWeekChartProps {
  data: ProductValueByWeek[] | undefined;
  isLoading: boolean;
}

export function ProductsByWeekChart({
  data,
  isLoading,
}: ProductsByWeekChartProps) {
  const { chartData, productNames } = useMemo(() => {
    if (!data || data.length === 0) {
      return { chartData: [], productNames: [] };
    }

    // Get unique weeks and products
    const weeks = [...new Set(data.map((d) => d.weekStartDate))].sort();
    const products = [...new Set(data.map((d) => d.productName))];

    // Transform data for recharts - each week becomes a data point with product values
    const transformed = weeks.map((week) => {
      const weekData: Record<string, string | number> = {
        week,
        weekLabel: format(parseISO(week), "dd MMM"),
      };
      products.forEach((product) => {
        const record = data.find(
          (d) => d.weekStartDate === week && d.productName === product
        );
        weekData[product] = record?.value ?? 0;
      });
      return weekData;
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
      <LineChart
        data={chartData}
        margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
      >
        <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
        <XAxis
          dataKey="weekLabel"
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
          labelFormatter={(_, payload) => {
            if (payload && payload.length > 0) {
              const week = payload[0]?.payload?.week;
              if (week) {
                return `Week of ${format(parseISO(week), "dd MMM yyyy")}`;
              }
            }
            return "";
          }}
          contentStyle={{
            backgroundColor: "hsl(var(--popover))",
            border: "1px solid hsl(var(--border))",
            borderRadius: "var(--radius)",
          }}
        />
        <Legend wrapperStyle={{ fontSize: 12 }} />
        {productNames.map((product, index) => (
          <Line
            key={product}
            type="monotone"
            dataKey={product}
            stroke={COLORS[index % COLORS.length]}
            name={product}
            strokeWidth={2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
          />
        ))}
      </LineChart>
    </ResponsiveContainer>
  );
}
