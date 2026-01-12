'use client';

import * as React from 'react';
import { useSearchParams, useRouter, usePathname } from 'next/navigation';
import { format } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { DataTable, type Column, type SortDirection } from '@/components/shared/data-table';
import { useAttendanceEvents, type AttendanceEvent } from '../hooks/useSiteAttendance';
import { useAllSites } from '@/lib/api/admin/use-sites';
import { useAllEmployees } from '@/lib/api/admin/use-employees';
import { MapPin, Clock, User, Building2, Filter, X } from 'lucide-react';

export default function AttendanceEventsPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Parse URL params
  const pageNumber = Number(searchParams.get('page')) || 1;
  const pageSize = Number(searchParams.get('pageSize')) || 20;
  const employeeId = searchParams.get('employeeId') || undefined;
  const siteId = searchParams.get('siteId') || undefined;
  const eventType = searchParams.get('eventType') || undefined;
  const fromDate = searchParams.get('fromDate') || undefined;
  const toDate = searchParams.get('toDate') || undefined;
  const includeNoise = searchParams.get('includeNoise') === 'true';

  // Fetch data
  const { data: eventsData, isLoading } = useAttendanceEvents({
    pageNumber,
    pageSize,
    employeeId,
    siteId,
    eventType,
    fromDate,
    toDate,
    includeNoise,
  });

  const { data: sites } = useAllSites();
  const { data: employees } = useAllEmployees();

  const updateUrlParams = (updates: Record<string, string | number | boolean | null | undefined>) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value === null || value === undefined || value === '' || value === false) {
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

  const handleFilterChange = (key: string, value: string | boolean | null) => {
    updateUrlParams({ [key]: value, page: 1 });
  };

  const clearFilters = () => {
    router.push(pathname);
  };

  const hasActiveFilters = employeeId || siteId || eventType || fromDate || toDate || includeNoise;

  const getEventTypeBadge = (type: 'Enter' | 'Exit') => {
    return (
      <Badge variant={type === 'Enter' ? 'default' : 'secondary'}>
        {type}
      </Badge>
    );
  };

  const getTriggerBadge = (method: 'Automatic' | 'Manual') => {
    return (
      <Badge variant={method === 'Automatic' ? 'outline' : 'secondary'}>
        {method}
      </Badge>
    );
  };

  const columns: Column<AttendanceEvent>[] = [
    {
      key: 'timestamp',
      header: 'Time',
      render: (event) => (
        <div className="flex items-center gap-2">
          <Clock className="h-4 w-4 text-muted-foreground" />
          <div>
            <div className="font-medium">{format(new Date(event.timestamp), 'HH:mm:ss')}</div>
            <div className="text-xs text-muted-foreground">
              {format(new Date(event.timestamp), 'dd MMM yyyy')}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: 'employeeName',
      header: 'Employee',
      render: (event) => (
        <div className="flex items-center gap-2">
          <User className="h-4 w-4 text-muted-foreground" />
          {event.employeeName}
        </div>
      ),
    },
    {
      key: 'siteName',
      header: 'Site',
      render: (event) => (
        <div className="flex items-center gap-2">
          <Building2 className="h-4 w-4 text-muted-foreground" />
          {event.siteName}
        </div>
      ),
    },
    {
      key: 'eventType',
      header: 'Event',
      render: (event) => getEventTypeBadge(event.eventType),
    },
    {
      key: 'triggerMethod',
      header: 'Trigger',
      className: 'hidden md:table-cell',
      headerClassName: 'hidden md:table-cell',
      render: (event) => getTriggerBadge(event.triggerMethod),
    },
    {
      key: 'isNoise',
      header: 'Status',
      className: 'hidden lg:table-cell',
      headerClassName: 'hidden lg:table-cell',
      render: (event) => (
        event.isNoise ? (
          <Badge variant="destructive" className="text-xs">
            Noise ({event.noiseDistance?.toFixed(0)}m)
          </Badge>
        ) : (
          <Badge variant="outline" className="text-xs text-green-600 border-green-600">
            Valid
          </Badge>
        )
      ),
    },
    {
      key: 'location',
      header: 'Location',
      className: 'hidden xl:table-cell',
      headerClassName: 'hidden xl:table-cell',
      render: (event) => (
        event.latitude && event.longitude ? (
          <div className="flex items-center gap-1 text-xs text-muted-foreground">
            <MapPin className="h-3 w-3" />
            {event.latitude.toFixed(4)}, {event.longitude.toFixed(4)}
          </div>
        ) : (
          <span className="text-muted-foreground">-</span>
        )
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Attendance Events</h1>
        <p className="text-muted-foreground">View all site entry and exit events</p>
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

            {/* Event Type Filter */}
            <div className="space-y-2">
              <Label>Event Type</Label>
              <Select
                value={eventType || 'all'}
                onValueChange={(v) => handleFilterChange('eventType', v === 'all' ? null : v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="All Types" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Types</SelectItem>
                  <SelectItem value="Enter">Enter</SelectItem>
                  <SelectItem value="Exit">Exit</SelectItem>
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

            {/* Include Noise */}
            <div className="flex items-end pb-2">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="includeNoise"
                  checked={includeNoise}
                  onCheckedChange={(checked) => handleFilterChange('includeNoise', checked === true)}
                />
                <Label htmlFor="includeNoise" className="font-normal cursor-pointer">
                  Include noise events
                </Label>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Events Table */}
      <DataTable
        columns={columns}
        data={eventsData?.items || []}
        isLoading={isLoading}
        emptyMessage="No attendance events found"
        keyExtractor={(event) => event.id}
        pagination={{
          pageNumber: eventsData?.pageNumber || 1,
          pageSize: eventsData?.pageSize || 20,
          totalCount: eventsData?.totalCount || 0,
          totalPages: eventsData?.totalPages || 0,
        }}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />
    </div>
  );
}
