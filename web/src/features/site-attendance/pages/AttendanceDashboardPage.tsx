'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { DataTable, type Column } from '@/components/shared/data-table';
import { useAttendanceKpis, useEmployeePerformance, type EmployeePerformance } from '../hooks/useSiteAttendance';
import { format, startOfWeek, startOfMonth } from 'date-fns';
import { Clock, Users, TrendingUp, TrendingDown, AlertTriangle, CheckCircle } from 'lucide-react';

export default function AttendanceDashboardPage() {
  const [period, setPeriod] = useState<'week' | 'month'>('week');

  const today = new Date();
  const fromDate = period === 'week'
    ? format(startOfWeek(today, { weekStartsOn: 1 }), 'yyyy-MM-dd')
    : format(startOfMonth(today), 'yyyy-MM-dd');
  const toDate = format(today, 'yyyy-MM-dd');

  const { data: kpis, isLoading: kpisLoading } = useAttendanceKpis({ fromDate, toDate });
  const { data: performance, isLoading: perfLoading } = useEmployeePerformance({ fromDate, toDate });

  const getStatusBadge = (status: string) => {
    const variants: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
      Excellent: 'default',
      Good: 'secondary',
      BelowTarget: 'destructive',
      Absent: 'outline',
    };
    return <Badge variant={variants[status] || 'outline'}>{status}</Badge>;
  };

  const getUtilizationColor = (percent: number) => {
    if (percent >= 90) return 'text-green-600';
    if (percent >= 75) return 'text-blue-600';
    if (percent >= 50) return 'text-yellow-600';
    return 'text-red-600';
  };

  const columns: Column<EmployeePerformance>[] = [
    { key: 'employeeName', header: 'Employee' },
    {
      key: 'totalHours',
      header: 'Total Hours',
      render: (item) => `${item.totalHours.toFixed(1)}h`
    },
    {
      key: 'expectedHours',
      header: 'Expected',
      render: (item) => `${item.expectedHours.toFixed(1)}h`
    },
    {
      key: 'utilizationPercent',
      header: 'Utilization',
      render: (item) => (
        <span className={getUtilizationColor(item.utilizationPercent)}>
          {item.utilizationPercent.toFixed(1)}%
        </span>
      )
    },
    {
      key: 'varianceHours',
      header: 'Variance',
      render: (item) => {
        const variance = item.varianceHours;
        return (
          <span className={variance >= 0 ? 'text-green-600' : 'text-red-600'}>
            {variance >= 0 ? '+' : ''}{variance.toFixed(1)}h
          </span>
        );
      }
    },
    {
      key: 'status',
      header: 'Status',
      render: (item) => getStatusBadge(item.status)
    },
    { key: 'daysPresent', header: 'Days Present' },
    { key: 'spaCount', header: 'SPA Count' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Site Attendance Dashboard</h1>
          <p className="text-muted-foreground">
            {format(new Date(fromDate), 'dd MMM yyyy')} - {format(new Date(toDate), 'dd MMM yyyy')}
          </p>
        </div>

        <div className="flex gap-4">
          <Select value={period} onValueChange={(v) => setPeriod(v as 'week' | 'month')}>
            <SelectTrigger className="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="week">This Week</SelectItem>
              <SelectItem value="month">This Month</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Overall Utilization</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${getUtilizationColor(kpis?.overallUtilization || 0)}`}>
              {kpis?.overallUtilization?.toFixed(1)}%
            </div>
            <Progress value={kpis?.overallUtilization || 0} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-2">
              {kpis?.actualHours?.toFixed(1)}h of {kpis?.expectedHours?.toFixed(1)}h expected
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Avg Hours/Day</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{kpis?.averageHoursPerDay?.toFixed(1)}h</div>
            <p className="text-xs text-muted-foreground mt-2">Target: 7.5h per day</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Employees</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{kpis?.totalActiveEmployees}</div>
            <p className="text-xs text-muted-foreground mt-2">
              Across {kpis?.totalActiveSites} sites
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Variance</CardTitle>
            {(kpis?.varianceHours || 0) >= 0 ? (
              <TrendingUp className="h-4 w-4 text-green-500" />
            ) : (
              <TrendingDown className="h-4 w-4 text-red-500" />
            )}
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${(kpis?.varianceHours || 0) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
              {(kpis?.varianceHours || 0) >= 0 ? '+' : ''}{kpis?.varianceHours?.toFixed(1)}h
            </div>
            <p className="text-xs text-muted-foreground mt-2">{kpis?.workingDays} working days</p>
          </CardContent>
        </Card>
      </div>

      {/* Performance Distribution */}
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        <Card className="bg-green-50 border-green-200">
          <CardContent className="pt-6 text-center">
            <CheckCircle className="h-8 w-8 text-green-600 mx-auto" />
            <div className="text-3xl font-bold text-green-700 mt-2">{kpis?.excellentCount || 0}</div>
            <p className="text-sm text-green-600">Excellent (&ge;90%)</p>
          </CardContent>
        </Card>

        <Card className="bg-blue-50 border-blue-200">
          <CardContent className="pt-6 text-center">
            <TrendingUp className="h-8 w-8 text-blue-600 mx-auto" />
            <div className="text-3xl font-bold text-blue-700 mt-2">{kpis?.goodCount || 0}</div>
            <p className="text-sm text-blue-600">Good (75-90%)</p>
          </CardContent>
        </Card>

        <Card className="bg-yellow-50 border-yellow-200">
          <CardContent className="pt-6 text-center">
            <AlertTriangle className="h-8 w-8 text-yellow-600 mx-auto" />
            <div className="text-3xl font-bold text-yellow-700 mt-2">{kpis?.belowTargetCount || 0}</div>
            <p className="text-sm text-yellow-600">Below Target (&lt;75%)</p>
          </CardContent>
        </Card>

        <Card className="bg-gray-50 border-gray-200">
          <CardContent className="pt-6 text-center">
            <Users className="h-8 w-8 text-gray-400 mx-auto" />
            <div className="text-3xl font-bold text-gray-700 mt-2">{kpis?.absentCount || 0}</div>
            <p className="text-sm text-gray-600">Absent</p>
          </CardContent>
        </Card>
      </div>

      {/* Employee Performance Table */}
      <Card>
        <CardHeader>
          <CardTitle>Employee Performance</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={performance || []}
            isLoading={perfLoading}
            keyExtractor={(item) => item.employeeId}
            emptyMessage="No employee performance data available"
          />
        </CardContent>
      </Card>
    </div>
  );
}
