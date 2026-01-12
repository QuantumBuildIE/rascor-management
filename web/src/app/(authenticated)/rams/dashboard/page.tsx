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
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { format } from "date-fns";
import {
  useRamsDashboard,
  useExportRamsToExcel,
  getStatusColor,
} from "@/lib/api/rams";
import { RamsStatusBadge } from "@/components/rams";
import { toast } from "sonner";
import type { RamsStatus, RamsExportRequest } from "@/types/rams";
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
  FileText,
  AlertTriangle,
  CheckCircle,
  Clock,
  TrendingUp,
  Download,
  ArrowLeft,
  ArrowDown,
  CalendarIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";

const RISK_COLORS = {
  High: "#ef4444",
  Medium: "#f59e0b",
  Low: "#22c55e",
};

const STATUS_COLORS: Record<string, string> = {
  Draft: "#9ca3af",
  PendingReview: "#f59e0b",
  Approved: "#22c55e",
  Rejected: "#ef4444",
  Archived: "#8b5cf6",
};

export default function RamsDashboardPage() {
  const router = useRouter();
  const [exportDialogOpen, setExportDialogOpen] = React.useState(false);
  const [exportOptions, setExportOptions] = React.useState<RamsExportRequest>({
    includeRiskAssessments: true,
    includeMethodSteps: true,
  });
  const [dateFrom, setDateFrom] = React.useState<Date | undefined>();
  const [dateTo, setDateTo] = React.useState<Date | undefined>();
  const [statusFilter, setStatusFilter] = React.useState<string>("");
  const [projectTypeFilter, setProjectTypeFilter] = React.useState<string>("");

  const { data: dashboard, isLoading, error } = useRamsDashboard();
  const exportMutation = useExportRamsToExcel();

  const handleExport = async () => {
    try {
      const request: RamsExportRequest = {
        ...exportOptions,
        dateFrom: dateFrom?.toISOString(),
        dateTo: dateTo?.toISOString(),
        status: statusFilter || undefined,
        projectType: projectTypeFilter || undefined,
      };

      const blob = await exportMutation.mutateAsync(request);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `RAMS_Export_${new Date().toISOString().split("T")[0]}.xlsx`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      setExportDialogOpen(false);
      toast.success("Export downloaded successfully");
    } catch (err) {
      toast.error("Failed to export data");
    }
  };

  // Prepare chart data
  const statusPieData = React.useMemo(() => {
    if (!dashboard?.statusCounts) return [];
    return dashboard.statusCounts.map((s) => ({
      name: s.status === "PendingReview" ? "Pending Review" : s.status,
      value: s.count,
      percentage: s.percentage,
      status: s.status,
    }));
  }, [dashboard?.statusCounts]);

  const riskDistributionData = React.useMemo(() => {
    if (!dashboard?.riskDistribution) return [];
    return dashboard.riskDistribution.map((r) => ({
      name: r.riskLevel,
      Initial: r.initialCount,
      Residual: r.residualCount,
    }));
  }, [dashboard?.riskDistribution]);

  const monthlyTrendsData = React.useMemo(() => {
    if (!dashboard?.monthlyTrends) return [];
    return dashboard.monthlyTrends;
  }, [dashboard?.monthlyTrends]);

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">RAMS Dashboard</h1>
            <p className="text-muted-foreground">Risk Assessment and Method Statement Analytics</p>
          </div>
        </div>
        <div className="rounded-lg border bg-card p-8 text-center">
          <p className="text-destructive">Failed to load dashboard data. Please try again.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.push("/rams")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">RAMS Dashboard</h1>
            <p className="text-muted-foreground">
              Risk Assessment and Method Statement Analytics
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => setExportDialogOpen(true)}>
            <Download className="mr-2 h-4 w-4" />
            Export to Excel
          </Button>
          <Button asChild>
            <Link href="/rams">View All Documents</Link>
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Documents</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <>
                <div className="text-2xl font-bold">{dashboard?.summary.totalDocuments ?? 0}</div>
                <p className="text-xs text-muted-foreground">
                  {dashboard?.summary.documentsThisMonth ?? 0} this month
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending Review</CardTitle>
            <Clock className="h-4 w-4 text-yellow-500" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <>
                <div className="text-2xl font-bold text-yellow-600">
                  {dashboard?.summary.pendingReviewDocuments ?? 0}
                </div>
                <p className="text-xs text-muted-foreground">Awaiting approval</p>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Approved</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <>
                <div className="text-2xl font-bold text-green-600">
                  {dashboard?.summary.approvedDocuments ?? 0}
                </div>
                <p className="text-xs text-muted-foreground">
                  {dashboard?.summary.approvalsThisMonth ?? 0} this month
                </p>
              </>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">High Risk Items</CardTitle>
            <AlertTriangle className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <>
                <div className="text-2xl font-bold text-red-600">
                  {dashboard?.summary.highRiskCount ?? 0}
                </div>
                <p className="text-xs text-muted-foreground">
                  Residual high risk assessments
                </p>
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Charts Row 1 */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Status Breakdown Pie Chart */}
        <Card>
          <CardHeader>
            <CardTitle>Documents by Status</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-[300px] w-full" />
            ) : statusPieData.length === 0 ? (
              <div className="flex h-[300px] items-center justify-center text-muted-foreground">
                No data available
              </div>
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
                        fill={STATUS_COLORS[entry.status] || "#6b7280"}
                      />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value, name) => [value, name]} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        {/* Risk Distribution Chart */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>Risk Distribution</span>
              <span className="text-sm font-normal text-muted-foreground">
                Initial vs Residual
              </span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-[300px] w-full" />
            ) : riskDistributionData.length === 0 ? (
              <div className="flex h-[300px] items-center justify-center text-muted-foreground">
                No data available
              </div>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={riskDistributionData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="Initial" fill="#94a3b8" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="Residual" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Monthly Trends Chart */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-4 w-4" />
            Monthly Trends (Last 6 Months)
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-[300px] w-full" />
          ) : monthlyTrendsData.length === 0 ? (
            <div className="flex h-[300px] items-center justify-center text-muted-foreground">
              No data available
            </div>
          ) : (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={monthlyTrendsData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="month" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="created"
                  name="Created"
                  stroke="#3b82f6"
                  strokeWidth={2}
                />
                <Line
                  type="monotone"
                  dataKey="approved"
                  name="Approved"
                  stroke="#22c55e"
                  strokeWidth={2}
                />
                <Line
                  type="monotone"
                  dataKey="rejected"
                  name="Rejected"
                  stroke="#ef4444"
                  strokeWidth={2}
                />
              </LineChart>
            </ResponsiveContainer>
          )}
        </CardContent>
      </Card>

      {/* Approval Metrics */}
      <Card>
        <CardHeader>
          <CardTitle>Approval Metrics</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
              {[...Array(6)].map((_, i) => (
                <Skeleton key={i} className="h-24 w-full" />
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-blue-600">
                  {dashboard?.approvalMetrics.averageApprovalDays.toFixed(1) ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Avg. Days to Approve</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-red-600">
                  {dashboard?.approvalMetrics.averageRejectionRate.toFixed(1) ?? 0}%
                </p>
                <p className="text-sm text-muted-foreground">Rejection Rate</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-green-600">
                  {dashboard?.approvalMetrics.fastestApprovalDays ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Fastest (days)</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-orange-600">
                  {dashboard?.approvalMetrics.slowestApprovalDays ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Slowest (days)</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-green-600">
                  {dashboard?.approvalMetrics.totalApprovedLast30Days ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Approved (30d)</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-red-600">
                  {dashboard?.approvalMetrics.totalRejectedLast30Days ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Rejected (30d)</p>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Tables Row */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Pending Approvals Table */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Pending Approvals
            </CardTitle>
            <Badge variant="secondary">
              {dashboard?.pendingApprovals.length ?? 0}
            </Badge>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="space-y-2">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : dashboard?.pendingApprovals.length === 0 ? (
              <div className="flex h-32 items-center justify-center text-muted-foreground">
                No documents pending approval
              </div>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Reference</TableHead>
                      <TableHead>Project</TableHead>
                      <TableHead className="text-center">Days</TableHead>
                      <TableHead className="text-center">Risks</TableHead>
                      <TableHead></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {dashboard?.pendingApprovals.map((doc) => (
                      <TableRow key={doc.id}>
                        <TableCell className="font-medium">{doc.projectReference}</TableCell>
                        <TableCell className="max-w-[150px] truncate">{doc.projectName}</TableCell>
                        <TableCell className="text-center">
                          <Badge
                            variant={
                              doc.daysPending > 7
                                ? "destructive"
                                : doc.daysPending > 3
                                  ? "secondary"
                                  : "outline"
                            }
                          >
                            {doc.daysPending}d
                          </Badge>
                        </TableCell>
                        <TableCell className="text-center">
                          {doc.riskAssessmentCount}
                          {doc.highRiskCount > 0 && (
                            <Badge variant="destructive" className="ml-1">
                              {doc.highRiskCount} high
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell>
                          <Button variant="ghost" size="sm" asChild>
                            <Link href={`/rams/${doc.id}`}>Review</Link>
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Overdue Documents Table */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-4 w-4 text-red-500" />
              Overdue Documents
            </CardTitle>
            {(dashboard?.overdueDocuments.length ?? 0) > 0 && (
              <Badge variant="destructive">
                {dashboard?.overdueDocuments.length}
              </Badge>
            )}
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="space-y-2">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : dashboard?.overdueDocuments.length === 0 ? (
              <div className="flex h-32 items-center justify-center text-muted-foreground">
                No overdue documents
              </div>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Reference</TableHead>
                      <TableHead>Project</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead className="text-center">Overdue</TableHead>
                      <TableHead></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {dashboard?.overdueDocuments.map((doc) => (
                      <TableRow key={doc.id}>
                        <TableCell className="font-medium">{doc.projectReference}</TableCell>
                        <TableCell className="max-w-[150px] truncate">{doc.projectName}</TableCell>
                        <TableCell>
                          <RamsStatusBadge status={doc.status as RamsStatus} />
                        </TableCell>
                        <TableCell className="text-center">
                          <Badge variant="destructive">{doc.daysOverdue} days</Badge>
                        </TableCell>
                        <TableCell>
                          <Button variant="ghost" size="sm" asChild>
                            <Link href={`/rams/${doc.id}`}>View</Link>
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Project Type Breakdown */}
      <Card>
        <CardHeader>
          <CardTitle>Documents by Project Type</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-24 w-full" />
          ) : (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
              {dashboard?.projectTypeCounts.map((pt) => (
                <div key={pt.projectType} className="rounded-lg border p-4 text-center">
                  <p className="text-2xl font-bold">{pt.count}</p>
                  <p className="text-sm text-muted-foreground">{pt.projectType}</p>
                  <p className="text-xs text-muted-foreground">{pt.percentage}%</p>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Risk Summary */}
      <Card>
        <CardHeader>
          <CardTitle>Risk Assessment Summary</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-24 w-full" />
          ) : (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold">{dashboard?.summary.totalRiskAssessments ?? 0}</p>
                <p className="text-sm text-muted-foreground">Total Assessments</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-red-600">
                  {dashboard?.summary.highRiskCount ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">High Risk</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-yellow-600">
                  {dashboard?.summary.mediumRiskCount ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Medium Risk</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-green-600">
                  {dashboard?.summary.lowRiskCount ?? 0}
                </p>
                <p className="text-sm text-muted-foreground">Low Risk</p>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Export Dialog */}
      <Dialog open={exportDialogOpen} onOpenChange={setExportDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Export RAMS Data</DialogTitle>
            <DialogDescription>
              Configure export options and download RAMS data as Excel file.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Date From</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className={cn(
                        "w-full justify-start text-left font-normal",
                        !dateFrom && "text-muted-foreground"
                      )}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {dateFrom ? format(dateFrom, "PPP") : "Pick date"}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <Calendar
                      mode="single"
                      selected={dateFrom}
                      onSelect={setDateFrom}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
              <div className="space-y-2">
                <Label>Date To</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className={cn(
                        "w-full justify-start text-left font-normal",
                        !dateTo && "text-muted-foreground"
                      )}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {dateTo ? format(dateTo, "PPP") : "Pick date"}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <Calendar
                      mode="single"
                      selected={dateTo}
                      onSelect={setDateTo}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>
            <div className="space-y-2">
              <Label>Status Filter</Label>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger>
                  <SelectValue placeholder="All statuses" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">All Statuses</SelectItem>
                  <SelectItem value="Draft">Draft</SelectItem>
                  <SelectItem value="PendingReview">Pending Review</SelectItem>
                  <SelectItem value="Approved">Approved</SelectItem>
                  <SelectItem value="Rejected">Rejected</SelectItem>
                  <SelectItem value="Archived">Archived</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Project Type Filter</Label>
              <Select value={projectTypeFilter} onValueChange={setProjectTypeFilter}>
                <SelectTrigger>
                  <SelectValue placeholder="All types" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">All Types</SelectItem>
                  <SelectItem value="RemedialInjection">Remedial Injection</SelectItem>
                  <SelectItem value="RascotankNewBuild">Rascotank New Build</SelectItem>
                  <SelectItem value="CarParkCoating">Car Park Coating</SelectItem>
                  <SelectItem value="GroundGasBarrier">Ground Gas Barrier</SelectItem>
                  <SelectItem value="Other">Other</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-4">
              <Label>Include in Export</Label>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="includeRiskAssessments"
                  checked={exportOptions.includeRiskAssessments}
                  onCheckedChange={(checked) =>
                    setExportOptions((prev) => ({
                      ...prev,
                      includeRiskAssessments: !!checked,
                    }))
                  }
                />
                <Label htmlFor="includeRiskAssessments" className="text-sm font-normal">
                  Risk Assessments
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="includeMethodSteps"
                  checked={exportOptions.includeMethodSteps}
                  onCheckedChange={(checked) =>
                    setExportOptions((prev) => ({
                      ...prev,
                      includeMethodSteps: !!checked,
                    }))
                  }
                />
                <Label htmlFor="includeMethodSteps" className="text-sm font-normal">
                  Method Steps
                </Label>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setExportDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleExport} disabled={exportMutation.isPending}>
              {exportMutation.isPending ? "Exporting..." : "Export"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
