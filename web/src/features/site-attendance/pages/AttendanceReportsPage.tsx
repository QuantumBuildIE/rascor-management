'use client';

import * as React from 'react';
import { useState, useMemo, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';
import { DataTable, type Column } from '@/components/shared/data-table';
import { ImageThumbnail } from '../components/ImagePreviewModal';
import { useAttendanceReport, type AttendanceReportEntry, type AttendanceReportStatus } from '../hooks/useSiteAttendance';
import { format, parseISO } from 'date-fns';
import { Calendar, Clock, RefreshCw, Search, Users, UserCheck, UserPlus } from 'lucide-react';
import { cn } from '@/lib/utils';
import Link from 'next/link';

type StatusFilter = 'All' | AttendanceReportStatus;

// Helper to normalize status (API may return number or string)
function normalizeStatus(status: AttendanceReportStatus | number): AttendanceReportStatus {
  if (typeof status === 'number') {
    const statusMap: Record<number, AttendanceReportStatus> = {
      0: 'Planned',
      1: 'Arrived',
      2: 'Unplanned',
    };
    return statusMap[status] || 'Planned';
  }
  return status;
}

// Format arrival time
function formatArrivalTime(dateTimeStr: string | null): string {
  if (!dateTimeStr) return '—';

  try {
    const date = parseISO(dateTimeStr);
    const hours = date.getHours();
    const minutes = date.getMinutes();

    // If midnight (00:00), just show the date
    if (hours === 0 && minutes === 0) {
      return format(date, 'dd MMM');
    }

    // If same day as today, just show time
    const today = new Date();
    if (
      date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear()
    ) {
      return format(date, 'HH:mm');
    }

    // Otherwise show date and time
    return format(date, 'dd MMM HH:mm');
  } catch {
    return '—';
  }
}

export default function AttendanceReportsPage() {
  const [selectedDate, setSelectedDate] = useState(() => format(new Date(), 'yyyy-MM-dd'));
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());
  const [secondsAgo, setSecondsAgo] = useState(0);

  const { data: report, isLoading, isFetching, refetch, dataUpdatedAt } = useAttendanceReport(selectedDate);

  // Update last updated timestamp when data changes
  useEffect(() => {
    if (dataUpdatedAt) {
      setLastUpdated(new Date(dataUpdatedAt));
    }
  }, [dataUpdatedAt]);

  // Update seconds ago counter
  useEffect(() => {
    const interval = setInterval(() => {
      setSecondsAgo(Math.floor((Date.now() - lastUpdated.getTime()) / 1000));
    }, 1000);
    return () => clearInterval(interval);
  }, [lastUpdated]);

  // Filter entries based on search and status
  const filteredEntries = useMemo(() => {
    if (!report?.entries) return [];

    return report.entries.filter((entry) => {
      // Status filter
      if (statusFilter !== 'All') {
        const entryStatus = normalizeStatus(entry.status);
        if (entryStatus !== statusFilter) return false;
      }

      // Search filter
      if (searchQuery) {
        const query = searchQuery.toLowerCase();
        const matchesEmployee = entry.employeeName.toLowerCase().includes(query);
        const matchesSite = entry.siteName.toLowerCase().includes(query);
        const matchesSiteCode = entry.siteCode?.toLowerCase().includes(query);
        if (!matchesEmployee && !matchesSite && !matchesSiteCode) return false;
      }

      return true;
    });
  }, [report?.entries, statusFilter, searchQuery]);

  // Status badge component
  const getStatusBadge = (status: AttendanceReportStatus | number) => {
    const normalizedStatus = normalizeStatus(status);
    const variants: Record<AttendanceReportStatus, { className: string; icon: React.ReactNode }> = {
      Planned: {
        className: 'bg-amber-100 text-amber-800 border-amber-200 hover:bg-amber-100',
        icon: <Clock className="h-3 w-3 mr-1" />,
      },
      Arrived: {
        className: 'bg-green-100 text-green-800 border-green-200 hover:bg-green-100',
        icon: <UserCheck className="h-3 w-3 mr-1" />,
      },
      Unplanned: {
        className: 'bg-blue-100 text-blue-800 border-blue-200 hover:bg-blue-100',
        icon: <UserPlus className="h-3 w-3 mr-1" />,
      },
    };

    const config = variants[normalizedStatus];

    return (
      <Badge variant="outline" className={cn('flex items-center w-fit', config.className)}>
        {config.icon}
        {normalizedStatus}
      </Badge>
    );
  };

  // Table columns
  const columns: Column<AttendanceReportEntry>[] = [
    {
      key: 'status',
      header: 'Status',
      render: (entry) => getStatusBadge(entry.status),
    },
    {
      key: 'employeeName',
      header: 'Employee',
      render: (entry) => (
        <span className="font-medium">{entry.employeeName}</span>
      ),
    },
    {
      key: 'siteName',
      header: 'Site',
      render: (entry) => (
        <div>
          <div className="font-medium">{entry.siteName}</div>
          {entry.siteCode && (
            <div className="text-xs text-muted-foreground">{entry.siteCode}</div>
          )}
        </div>
      ),
    },
    {
      key: 'plannedArrival',
      header: 'Planned Arrival',
      render: (entry) => (
        <span className="text-sm">{formatArrivalTime(entry.plannedArrival)}</span>
      ),
    },
    {
      key: 'actualArrival',
      header: 'Actual Arrival',
      render: (entry) => (
        <span className="text-sm">{formatArrivalTime(entry.actualArrival)}</span>
      ),
    },
    {
      key: 'spa',
      header: 'SPA',
      render: (entry) => {
        if (entry.spaCompleted && entry.spaImageUrl) {
          return (
            <ImageThumbnail
              src={entry.spaImageUrl}
              alt={`SPA for ${entry.employeeName}`}
              title={`SPA Photo - ${entry.employeeName}`}
              size="sm"
            />
          );
        }

        if (entry.spaCompleted && entry.spaId) {
          return (
            <Link
              href={`/site-attendance/spa/${entry.spaId}`}
              className="text-sm text-primary hover:underline"
            >
              View
            </Link>
          );
        }

        return <span className="text-sm text-muted-foreground">Not completed</span>;
      },
    },
  ];

  // Format seconds ago for display
  const formatSecondsAgo = (seconds: number): string => {
    if (seconds < 60) return `${seconds}s ago`;
    const minutes = Math.floor(seconds / 60);
    return `${minutes}m ago`;
  };

  // Loading skeleton
  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-3xl font-bold">Site Attendance Report</h1>
            <Skeleton className="h-4 w-48 mt-2" />
          </div>
          <Skeleton className="h-10 w-40" />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <Card key={i}>
              <CardContent className="pt-6">
                <Skeleton className="h-8 w-16 mx-auto" />
                <Skeleton className="h-4 w-24 mx-auto mt-2" />
              </CardContent>
            </Card>
          ))}
        </div>

        <Card>
          <CardContent className="pt-6">
            <div className="space-y-4">
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header Section */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold">Site Attendance Report</h1>
          <div className="flex items-center gap-2 mt-1 text-sm text-muted-foreground">
            <span>Last updated: {formatSecondsAgo(secondsAgo)}</span>
            <Button
              variant="ghost"
              size="icon"
              className="h-6 w-6"
              onClick={() => refetch()}
              disabled={isFetching}
            >
              <RefreshCw className={cn('h-3 w-3', isFetching && 'animate-spin')} />
            </Button>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-muted-foreground" />
          <Input
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            className="w-40"
          />
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="bg-amber-50 border-amber-200">
          <CardContent className="pt-6 text-center">
            <Clock className="h-8 w-8 text-amber-600 mx-auto" />
            <div className="text-3xl font-bold text-amber-700 mt-2">
              {report?.totalPlanned ?? 0}
            </div>
            <p className="text-sm text-amber-600">Planned</p>
          </CardContent>
        </Card>

        <Card className="bg-green-50 border-green-200">
          <CardContent className="pt-6 text-center">
            <UserCheck className="h-8 w-8 text-green-600 mx-auto" />
            <div className="text-3xl font-bold text-green-700 mt-2">
              {report?.totalArrived ?? 0}
            </div>
            <p className="text-sm text-green-600">Arrived</p>
          </CardContent>
        </Card>

        <Card className="bg-blue-50 border-blue-200">
          <CardContent className="pt-6 text-center">
            <UserPlus className="h-8 w-8 text-blue-600 mx-auto" />
            <div className="text-3xl font-bold text-blue-700 mt-2">
              {report?.totalUnplanned ?? 0}
            </div>
            <p className="text-sm text-blue-600">Unplanned</p>
          </CardContent>
        </Card>
      </div>

      {/* Search and Filter Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            {/* Search Input */}
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search employee or site..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>

            {/* Status Filter Buttons */}
            <div className="flex gap-1">
              {(['All', 'Planned', 'Arrived', 'Unplanned'] as StatusFilter[]).map((status) => (
                <Button
                  key={status}
                  variant={statusFilter === status ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setStatusFilter(status)}
                  className={cn(
                    statusFilter === status && status === 'Planned' && 'bg-amber-600 hover:bg-amber-700',
                    statusFilter === status && status === 'Arrived' && 'bg-green-600 hover:bg-green-700',
                    statusFilter === status && status === 'Unplanned' && 'bg-blue-600 hover:bg-blue-700'
                  )}
                >
                  {status}
                </Button>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            Attendance ({filteredEntries.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredEntries}
            isLoading={isFetching && !report}
            keyExtractor={(entry) => `${entry.employeeId}-${entry.siteId}`}
            emptyMessage="No attendance data for this date"
          />
        </CardContent>
      </Card>
    </div>
  );
}
