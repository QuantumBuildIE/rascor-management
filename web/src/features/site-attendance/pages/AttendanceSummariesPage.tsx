'use client';

import * as React from 'react';
import { useSearchParams, useRouter, usePathname } from 'next/navigation';
import { format, startOfWeek, endOfWeek } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { DataTable, type Column } from '@/components/shared/data-table';
import { useAttendanceSummaries, type AttendanceSummary } from '../hooks/useSiteAttendance';
import { useAllSites } from '@/lib/api/admin/use-sites';
import { useAllEmployees } from '@/lib/api/admin/use-employees';
import { Clock, User, Building2, Filter, X, TrendingUp, TrendingDown, AlertTriangle } from 'lucide-react';
import { cn } from '@/lib/utils';

export default function AttendanceSummariesPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Default to current week
  const today = new Date();
  const defaultFromDate = format(startOfWeek(today, { weekStartsOn: 1 }), 'yyyy-MM-dd');
  const defaultToDate = format(endOfWeek(today, { weekStartsOn: 1 }), 'yyyy-MM-dd');

  // Parse URL params
  const pageNumber = Number(searchParams.get('page')) || 1;
  const pageSize = Number(searchParams.get('pageSize')) || 20;
  const employeeId = searchParams.get('employeeId') || undefined;
  const siteId = searchParams.get('siteId') || undefined;
  const fromDate = searchParams.get('fromDate') || defaultFromDate;
  const toDate = searchParams.get('toDate') || defaultToDate;

  // Fetch data
  const { data: summariesData, isLoading } = useAttendanceSummaries({
    pageNumber,
    pageSize,
    employeeId,
    siteId,
    fromDate,
    toDate,
  });

  const { data: sites } = useAllSites();
  const { data: employees } = useAllEmployees();

  const updateUrlParams = (updates: Record<string, string | number | null | undefined>) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value === null || value === undefined || value === '') {
        params.delete(key);
      } else {
        params.set(key, String(value));
      }
    });

    const queryString = params.toString();
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  };

  const handlePageChange = (page: number) => {
    updateUrlParams({ page });
  };

  const handlePageSizeChange = (size: number) => {
    updateUrlParams({ pageSize: size, page: 1 });
  };

  const handleFilterChange = (key: string, value: string | null) => {
    updateUrlParams({ [key]: value, page: 1 });
  };

  const clearFilters = () => {
    router.push(pathname);
  };

  const hasActiveFilters = employeeId || siteId ||
    (fromDate && fromDate !== defaultFromDate) ||
    (toDate && toDate !== defaultToDate);

  const getStatusBadge = (status: AttendanceSummary['status']) => {
    const variants: Record<string, { variant: 'default' | 'secondary' | 'destructive' | 'outline'; icon: React.ReactNode }> = {
      Excellent: { variant: 'default', icon: <TrendingUp className="h-3 w-3 mr-1" /> },
      Good: { variant: 'secondary', icon: null },
      BelowTarget: { variant: 'destructive', icon: <TrendingDown className="h-3 w-3 mr-1" /> },
      Absent: { variant: 'outline', icon: null },
      Incomplete: { variant: 'outline', icon: <AlertTriangle className="h-3 w-3 mr-1" /> },
    };

    const config = variants[status] || variants.Absent;

    return (
      <Badge variant={config.variant} className="flex items-center w-fit">
        {config.icon}
        {status}
      </Badge>
    );
  };

  const getUtilizationColor = (percent: number) => {
    if (percent >= 90) return 'text-green-600';
    if (percent >= 75) return 'text-blue-600';
    if (percent >= 50) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getProgressColor = (percent: number) => {
    if (percent >= 90) return 'bg-green-600';
    if (percent >= 75) return 'bg-blue-600';
    if (percent >= 50) return 'bg-yellow-600';
    return 'bg-red-600';
  };

  const columns: Column<AttendanceSummary>[] = [
    {
      key: 'date',
      header: 'Date',
      sortable: true,
      render: (summary) => (
        <div>
          <div className="font-medium">{format(new Date(summary.date), 'EEE, dd MMM')}</div>
          <div className="text-xs text-muted-foreground">
            {format(new Date(summary.date), 'yyyy')}
          </div>
        </div>
      ),
    },
    {
      key: 'employeeName',
      header: 'Employee',
      sortable: true,
      render: (summary) => (
        <div className="flex items-center gap-2">
          <User className="h-4 w-4 text-muted-foreground" />
          {summary.employeeName}
        </div>
      ),
    },
    {
      key: 'siteName',
      header: 'Site',
      sortable: true,
      className: 'hidden md:table-cell',
      headerClassName: 'hidden md:table-cell',
      render: (summary) => (
        <div className="flex items-center gap-2">
          <Building2 className="h-4 w-4 text-muted-foreground" />
          {summary.siteName}
        </div>
      ),
    },
    {
      key: 'timeOnSite',
      header: 'Time on Site',
      render: (summary) => (
        <div className="flex items-center gap-2">
          <Clock className="h-4 w-4 text-muted-foreground" />
          <div>
            <div className="font-medium">{summary.timeOnSiteHours.toFixed(1)}h</div>
            <div className="text-xs text-muted-foreground">
              of {summary.expectedHours.toFixed(1)}h expected
            </div>
          </div>
        </div>
      ),
    },
    {
      key: 'utilizationPercent',
      header: 'Utilization',
      className: 'hidden lg:table-cell',
      headerClassName: 'hidden lg:table-cell',
      render: (summary) => (
        <div className="w-32 space-y-1">
          <div className="flex justify-between text-sm">
            <span className={cn('font-medium', getUtilizationColor(summary.utilizationPercent))}>
              {summary.utilizationPercent.toFixed(0)}%
            </span>
          </div>
          <div className="h-2 rounded-full bg-muted overflow-hidden">
            <div
              className={cn('h-full rounded-full transition-all', getProgressColor(summary.utilizationPercent))}
              style={{ width: `${Math.min(summary.utilizationPercent, 100)}%` }}
            />
          </div>
        </div>
      ),
    },
    {
      key: 'varianceHours',
      header: 'Variance',
      className: 'hidden xl:table-cell',
      headerClassName: 'hidden xl:table-cell',
      render: (summary) => {
        const variance = summary.varianceHours;
        return (
          <span className={cn('font-medium', variance >= 0 ? 'text-green-600' : 'text-red-600')}>
            {variance >= 0 ? '+' : ''}{variance.toFixed(1)}h
          </span>
        );
      },
    },
    {
      key: 'times',
      header: 'Entry/Exit',
      className: 'hidden xl:table-cell',
      headerClassName: 'hidden xl:table-cell',
      render: (summary) => (
        <div className="text-sm">
          <div>
            <span className="text-muted-foreground">In:</span>{' '}
            {summary.firstEntry ? format(new Date(summary.firstEntry), 'HH:mm') : '-'}
          </div>
          <div>
            <span className="text-muted-foreground">Out:</span>{' '}
            {summary.lastExit ? format(new Date(summary.lastExit), 'HH:mm') : '-'}
          </div>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (summary) => getStatusBadge(summary.status),
    },
    {
      key: 'counts',
      header: 'Events',
      className: 'hidden lg:table-cell',
      headerClassName: 'hidden lg:table-cell',
      render: (summary) => (
        <div className="text-sm text-muted-foreground">
          <span>{summary.entryCount} in / {summary.exitCount} out</span>
          {summary.hasSpa && (
            <Badge variant="outline" className="ml-2 text-xs">
              SPA
            </Badge>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Daily Summaries</h1>
        <p className="text-muted-foreground">View daily attendance summaries by employee and site</p>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-base flex items-center gap-2">
              <Filter className="h-4 w-4" />
              Filters
            </CardTitle>
            {hasActiveFilters && (
              <Button variant="ghost" size="sm" onClick={clearFilters}>
                <X className="h-4 w-4 mr-1" />
                Clear All
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {/* Employee Filter */}
            <div className="space-y-2">
              <Label>Employee</Label>
              <Select
                value={employeeId || 'all'}
                onValueChange={(v) => handleFilterChange('employeeId', v === 'all' ? null : v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="All Employees" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Employees</SelectItem>
                  {employees?.map((emp) => (
                    <SelectItem key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Site Filter */}
            <div className="space-y-2">
              <Label>Site</Label>
              <Select
                value={siteId || 'all'}
                onValueChange={(v) => handleFilterChange('siteId', v === 'all' ? null : v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="All Sites" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Sites</SelectItem>
                  {sites?.map((site) => (
                    <SelectItem key={site.id} value={site.id}>
                      {site.siteName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Date Range */}
            <div className="space-y-2">
              <Label>From Date</Label>
              <Input
                type="date"
                value={fromDate || ''}
                onChange={(e) => handleFilterChange('fromDate', e.target.value || null)}
              />
            </div>

            <div className="space-y-2">
              <Label>To Date</Label>
              <Input
                type="date"
                value={toDate || ''}
                onChange={(e) => handleFilterChange('toDate', e.target.value || null)}
              />
            </div>
          </div>

          {/* Quick Date Presets */}
          <div className="flex flex-wrap gap-2 mt-4 pt-4 border-t">
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                const today = new Date();
                const start = format(startOfWeek(today, { weekStartsOn: 1 }), 'yyyy-MM-dd');
                const end = format(endOfWeek(today, { weekStartsOn: 1 }), 'yyyy-MM-dd');
                updateUrlParams({ fromDate: start, toDate: end, page: 1 });
              }}
            >
              This Week
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                const today = new Date();
                const lastWeek = new Date(today.setDate(today.getDate() - 7));
                const start = format(startOfWeek(lastWeek, { weekStartsOn: 1 }), 'yyyy-MM-dd');
                const end = format(endOfWeek(lastWeek, { weekStartsOn: 1 }), 'yyyy-MM-dd');
                updateUrlParams({ fromDate: start, toDate: end, page: 1 });
              }}
            >
              Last Week
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                const today = new Date();
                const start = format(new Date(today.getFullYear(), today.getMonth(), 1), 'yyyy-MM-dd');
                const end = format(today, 'yyyy-MM-dd');
                updateUrlParams({ fromDate: start, toDate: end, page: 1 });
              }}
            >
              This Month
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                const today = new Date();
                const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 1);
                const start = format(lastMonth, 'yyyy-MM-dd');
                const end = format(new Date(today.getFullYear(), today.getMonth(), 0), 'yyyy-MM-dd');
                updateUrlParams({ fromDate: start, toDate: end, page: 1 });
              }}
            >
              Last Month
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Summaries Table */}
      <DataTable
        columns={columns}
        data={summariesData?.items || []}
        isLoading={isLoading}
        emptyMessage="No attendance summaries found for the selected period"
        keyExtractor={(summary) => summary.id}
        pagination={{
          pageNumber: summariesData?.pageNumber || 1,
          pageSize: summariesData?.pageSize || 20,
          totalCount: summariesData?.totalCount || 0,
          totalPages: summariesData?.totalPages || 0,
        }}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />
    </div>
  );
}
