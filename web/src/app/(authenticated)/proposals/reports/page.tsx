"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  usePipelineReport,
  useConversionReport,
  useByStatusReport,
  useByCompanyReport,
  useWinLossAnalysis,
  useMonthlyTrendsReport,
  getStatusDisplayName,
  getStatusColor,
  type ProposalStatus,
} from "@/lib/api/proposals";
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts";
import {
  TrendingUp,
  Trophy,
  Percent,
  Target,
  ArrowLeft,
  Building2,
  Clock,
} from "lucide-react";

// Date range presets
type DateRange = "thisMonth" | "thisQuarter" | "thisYear" | "last12Months" | "all";

function getDateRange(range: DateRange): { fromDate?: string; toDate?: string } {
  const now = new Date();
  const toDate = now.toISOString().split("T")[0];

  switch (range) {
    case "thisMonth": {
      const fromDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split("T")[0];
      return { fromDate, toDate };
    }
    case "thisQuarter": {
      const quarter = Math.floor(now.getMonth() / 3);
      const fromDate = new Date(now.getFullYear(), quarter * 3, 1).toISOString().split("T")[0];
      return { fromDate, toDate };
    }
    case "thisYear": {
      const fromDate = new Date(now.getFullYear(), 0, 1).toISOString().split("T")[0];
      return { fromDate, toDate };
    }
    case "last12Months": {
      const fromDate = new Date(now.getFullYear() - 1, now.getMonth(), 1).toISOString().split("T")[0];
      return { fromDate, toDate };
    }
    case "all":
    default:
      return {};
  }
}

const COLORS = ["#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899", "#6366f1", "#14b8a6", "#f97316"];

const STATUS_COLORS: Record<string, string> = {
  Draft: "#9ca3af",
  Submitted: "#3b82f6",
  UnderReview: "#f59e0b",
  Approved: "#10b981",
  Rejected: "#ef4444",
  Won: "#059669",
  Lost: "#f97316",
  Expired: "#8b5cf6",
  Cancelled: "#6b7280",
};

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: "EUR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

function formatCurrencyShort(value: number): string {
  if (value >= 1000000) {
    return `€${(value / 1000000).toFixed(1)}M`;
  }
  if (value >= 1000) {
    return `€${(value / 1000).toFixed(0)}K`;
  }
  return formatCurrency(value);
}

export default function ProposalReportsPage() {
  const router = useRouter();
  const [dateRange, setDateRange] = React.useState<DateRange>("last12Months");
  const { fromDate, toDate } = getDateRange(dateRange);

  // Fetch all reports
  const { data: pipelineData, isLoading: pipelineLoading } = usePipelineReport(fromDate, toDate);
  const { data: conversionData, isLoading: conversionLoading } = useConversionReport(fromDate, toDate);
  const { data: statusData, isLoading: statusLoading } = useByStatusReport(fromDate, toDate);
  const { data: companyData, isLoading: companyLoading } = useByCompanyReport(fromDate, toDate, 10);
  const { data: winLossData, isLoading: winLossLoading } = useWinLossAnalysis(fromDate, toDate);
  const { data: trendsData, isLoading: trendsLoading } = useMonthlyTrendsReport(12);

  const isLoading = pipelineLoading || conversionLoading || statusLoading || companyLoading || winLossLoading || trendsLoading;

  // Prepare chart data
  const pipelineChartData = React.useMemo(() => {
    if (!pipelineData?.stages) return [];
    return pipelineData.stages.map((stage) => ({
      name: getStatusDisplayName(stage.status as ProposalStatus),
      count: stage.count,
      value: stage.value,
      status: stage.status,
    }));
  }, [pipelineData]);

  const statusPieData = React.useMemo(() => {
    if (!statusData?.statuses) return [];
    return statusData.statuses.map((status) => ({
      name: getStatusDisplayName(status.status as ProposalStatus),
      value: status.count,
      totalValue: status.value,
      status: status.status,
    }));
  }, [statusData]);

  const trendsChartData = React.useMemo(() => {
    if (!trendsData?.dataPoints) return [];
    return trendsData.dataPoints;
  }, [trendsData]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">Proposal Analytics</h1>
            <p className="text-muted-foreground">
              Track proposal performance and conversion metrics
            </p>
          </div>
        </div>
        <div className="flex items-center gap-4">
          <Select value={dateRange} onValueChange={(v) => setDateRange(v as DateRange)}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Date range" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="thisMonth">This Month</SelectItem>
              <SelectItem value="thisQuarter">This Quarter</SelectItem>
              <SelectItem value="thisYear">This Year</SelectItem>
              <SelectItem value="last12Months">Last 12 Months</SelectItem>
              <SelectItem value="all">All Time</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Pipeline</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {pipelineLoading ? (
              <Skeleton className="h-8 w-32" />
            ) : (
              <>
                <div className="text-2xl font-bold">
                  {formatCurrency(pipelineData?.totalPipelineValue ?? 0)}
                </div>
                <p className="text-xs text-muted-foreground">
                  {pipelineData?.totalProposals ?? 0} active proposals
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Conversion Rate</CardTitle>
            <Percent className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {conversionLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <>
                <div className="text-2xl font-bold">
                  {conversionData?.conversionRate?.toFixed(1) ?? 0}%
                </div>
                <p className="text-xs text-muted-foreground">
                  Won / (Won + Lost)
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Won Value</CardTitle>
            <Trophy className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {conversionLoading ? (
              <Skeleton className="h-8 w-32" />
            ) : (
              <>
                <div className="text-2xl font-bold text-green-600">
                  {formatCurrency(conversionData?.wonValue ?? 0)}
                </div>
                <p className="text-xs text-muted-foreground">
                  {conversionData?.wonCount ?? 0} proposals won
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Average Deal Size</CardTitle>
            <Target className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {conversionLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <>
                <div className="text-2xl font-bold">
                  {formatCurrency(conversionData?.averageWonValue ?? 0)}
                </div>
                <p className="text-xs text-muted-foreground">
                  Avg. won proposal value
                </p>
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Charts Row 1 */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Pipeline Funnel Chart */}
        <Card>
          <CardHeader>
            <CardTitle>Pipeline by Stage</CardTitle>
          </CardHeader>
          <CardContent>
            {pipelineLoading ? (
              <Skeleton className="h-[300px] w-full" />
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={pipelineChartData} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" tickFormatter={formatCurrencyShort} />
                  <YAxis dataKey="name" type="category" width={100} />
                  <Tooltip
                    formatter={(value, name) => [
                      name === "value" ? formatCurrency(Number(value)) : value,
                      name === "value" ? "Value" : "Count",
                    ]}
                  />
                  <Bar
                    dataKey="value"
                    name="Value"
                    fill="#3b82f6"
                    radius={[0, 4, 4, 0]}
                  />
                </BarChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        {/* Status Breakdown Pie Chart */}
        <Card>
          <CardHeader>
            <CardTitle>Proposals by Status</CardTitle>
          </CardHeader>
          <CardContent>
            {statusLoading ? (
              <Skeleton className="h-[300px] w-full" />
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={statusPieData}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="50%"
                    outerRadius={100}
                    label={({ name, value }) => `${name}: ${value}`}
                  >
                    {statusPieData.map((entry, index) => (
                      <Cell
                        key={`cell-${index}`}
                        fill={STATUS_COLORS[entry.status] || COLORS[index % COLORS.length]}
                      />
                    ))}
                  </Pie>
                  <Tooltip
                    formatter={(value, name, props) => [
                      `${value} proposals (${formatCurrency(props.payload?.totalValue ?? 0)})`,
                      String(name),
                    ]}
                  />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Monthly Trends Chart */}
      <Card>
        <CardHeader>
          <CardTitle>Monthly Trends</CardTitle>
        </CardHeader>
        <CardContent>
          {trendsLoading ? (
            <Skeleton className="h-[300px] w-full" />
          ) : (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={trendsChartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="monthName" />
                <YAxis yAxisId="left" />
                <YAxis yAxisId="right" orientation="right" tickFormatter={(v) => `${v}%`} />
                <Tooltip
                  formatter={(value, name) => {
                    const numValue = Number(value);
                    if (name === "conversionRate") return [`${numValue.toFixed(1)}%`, "Conversion Rate"];
                    return [numValue, name === "proposalsCreated" ? "Created" : name === "proposalsWon" ? "Won" : "Lost"];
                  }}
                />
                <Legend />
                <Line
                  yAxisId="left"
                  type="monotone"
                  dataKey="proposalsCreated"
                  name="Created"
                  stroke="#3b82f6"
                  strokeWidth={2}
                />
                <Line
                  yAxisId="left"
                  type="monotone"
                  dataKey="proposalsWon"
                  name="Won"
                  stroke="#10b981"
                  strokeWidth={2}
                />
                <Line
                  yAxisId="left"
                  type="monotone"
                  dataKey="proposalsLost"
                  name="Lost"
                  stroke="#f97316"
                  strokeWidth={2}
                />
                <Line
                  yAxisId="right"
                  type="monotone"
                  dataKey="conversionRate"
                  name="Conversion Rate"
                  stroke="#8b5cf6"
                  strokeWidth={2}
                  strokeDasharray="5 5"
                />
              </LineChart>
            </ResponsiveContainer>
          )}
        </CardContent>
      </Card>

      {/* Bottom Row */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Top Companies Table */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-4 w-4" />
              Top Companies
            </CardTitle>
          </CardHeader>
          <CardContent>
            {companyLoading ? (
              <div className="space-y-2">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : companyData?.companies.length === 0 ? (
              <p className="py-8 text-center text-muted-foreground">
                No proposals found in this period
              </p>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Company</TableHead>
                      <TableHead className="text-right">Proposals</TableHead>
                      <TableHead className="text-right">Won</TableHead>
                      <TableHead className="text-right">Value</TableHead>
                      <TableHead className="text-right">Conv. %</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {companyData?.companies.map((company, index) => (
                      <TableRow key={`${company.companyId}-${index}`}>
                        <TableCell className="font-medium">{company.companyName}</TableCell>
                        <TableCell className="text-right">{company.totalProposals}</TableCell>
                        <TableCell className="text-right text-green-600">{company.wonCount}</TableCell>
                        <TableCell className="text-right">{formatCurrencyShort(company.totalValue)}</TableCell>
                        <TableCell className="text-right">
                          {company.conversionRate.toFixed(0)}%
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Win/Loss Analysis */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Win/Loss Analysis
            </CardTitle>
          </CardHeader>
          <CardContent>
            {winLossLoading ? (
              <div className="space-y-4">
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
              </div>
            ) : (
              <div className="space-y-6">
                {/* Time metrics */}
                <div className="grid grid-cols-2 gap-4">
                  <div className="rounded-lg border p-4">
                    <p className="text-sm text-muted-foreground">Avg. Time to Win</p>
                    <p className="text-2xl font-bold text-green-600">
                      {winLossData?.averageTimeToWinDays?.toFixed(0) ?? 0} days
                    </p>
                  </div>
                  <div className="rounded-lg border p-4">
                    <p className="text-sm text-muted-foreground">Avg. Time to Lose</p>
                    <p className="text-2xl font-bold text-orange-600">
                      {winLossData?.averageTimeToLossDays?.toFixed(0) ?? 0} days
                    </p>
                  </div>
                </div>

                {/* Reasons */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="mb-2 text-sm font-medium text-green-600">Top Win Reasons</p>
                    {winLossData?.winReasons.length === 0 ? (
                      <p className="text-sm text-muted-foreground">No data</p>
                    ) : (
                      <ul className="space-y-1 text-sm">
                        {winLossData?.winReasons.slice(0, 5).map((reason, i) => (
                          <li key={i} className="flex justify-between">
                            <span className="truncate">{reason.reason}</span>
                            <span className="ml-2 text-muted-foreground">{reason.count}</span>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                  <div>
                    <p className="mb-2 text-sm font-medium text-orange-600">Top Loss Reasons</p>
                    {winLossData?.lossReasons.length === 0 ? (
                      <p className="text-sm text-muted-foreground">No data</p>
                    ) : (
                      <ul className="space-y-1 text-sm">
                        {winLossData?.lossReasons.slice(0, 5).map((reason, i) => (
                          <li key={i} className="flex justify-between">
                            <span className="truncate">{reason.reason}</span>
                            <span className="ml-2 text-muted-foreground">{reason.count}</span>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Conversion Summary Table */}
      <Card>
        <CardHeader>
          <CardTitle>Conversion Summary</CardTitle>
        </CardHeader>
        <CardContent>
          {conversionLoading ? (
            <Skeleton className="h-24 w-full" />
          ) : (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold">{conversionData?.totalProposals ?? 0}</p>
                <p className="text-sm text-muted-foreground">Total Proposals</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-green-600">{conversionData?.wonCount ?? 0}</p>
                <p className="text-sm text-muted-foreground">Won</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-orange-600">{conversionData?.lostCount ?? 0}</p>
                <p className="text-sm text-muted-foreground">Lost</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-blue-600">{conversionData?.pendingCount ?? 0}</p>
                <p className="text-sm text-muted-foreground">Pending</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-gray-500">{conversionData?.cancelledCount ?? 0}</p>
                <p className="text-sm text-muted-foreground">Cancelled</p>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
