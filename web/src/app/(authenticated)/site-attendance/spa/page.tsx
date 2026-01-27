'use client';

import * as React from 'react';
import { useSearchParams, useRouter, usePathname } from 'next/navigation';
import { format, startOfWeek, endOfWeek, startOfMonth } from 'date-fns';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { DataTable, type Column } from '@/components/shared/data-table';
import { useSpaRecords, type SitePhotoAttendance } from '@/features/site-attendance/hooks/useSiteAttendance';
import { useAllSites } from '@/lib/api/admin/use-sites';
import { useAllEmployees } from '@/lib/api/admin/use-employees';
import { ImageThumbnail } from '@/features/site-attendance/components/ImagePreviewModal';
import {
  Camera,
  User,
  Building2,
  Filter,
  X,
  Plus,
  MapPin,
  Calendar,
  Cloud,
} from 'lucide-react';

export default function SpaListPage() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Default to current month
  const today = new Date();
  const defaultFromDate = format(startOfMonth(today), 'yyyy-MM-dd');
  const defaultToDate = format(today, 'yyyy-MM-dd');

  // Parse URL params
  const page = Number(searchParams.get('page')) || 1;
  const pageSize = Number(searchParams.get('pageSize')) || 20;
  const employeeId = searchParams.get('employeeId') || undefined;
  const siteId = searchParams.get('siteId') || undefined;
  const fromDate = searchParams.get('fromDate') || defaultFromDate;
  const toDate = searchParams.get('toDate') || defaultToDate;

  // Fetch data
  const { data: spaData, isLoading } = useSpaRecords({
    page,
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

  const handlePageChange = (newPage: number) => {
    updateUrlParams({ page: newPage });
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

  const hasActiveFilters =
    employeeId ||
    siteId ||
    (fromDate && fromDate !== defaultFromDate) ||
    (toDate && toDate !== defaultToDate);

  const columns: Column<SitePhotoAttendance>[] = [
    {
      key: 'eventDate',
      header: 'Date',
      sortable: true,
      render: (spa) => (
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-muted-foreground" />
          <div>
            <div className="font-medium">{format(new Date(spa.eventDate), 'EEE, dd MMM')}</div>
            <div className="text-xs text-muted-foreground">
              {format(new Date(spa.eventDate), 'yyyy')}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: 'employeeName',
      header: 'Employee',
      sortable: true,
      render: (spa) => (
        <div className="flex items-center gap-2">
          <User className="h-4 w-4 text-muted-foreground" />
          {spa.employeeName}
        </div>
      ),
    },
    {
      key: 'siteName',
      header: 'Site',
      sortable: true,
      className: 'hidden md:table-cell',
      headerClassName: 'hidden md:table-cell',
      render: (spa) => (
        <div className="flex items-center gap-2">
          <Building2 className="h-4 w-4 text-muted-foreground" />
          {spa.siteName}
        </div>
      ),
    },
    {
      key: 'image',
      header: 'Photo',
      render: (spa) => (
        <ImageThumbnail
          src={spa.imageUrl}
          alt={`SPA photo for ${spa.employeeName}`}
          title="Site Photo"
          size="md"
        />
      ),
    },
    {
      key: 'signature',
      header: 'Signature',
      className: 'hidden lg:table-cell',
      headerClassName: 'hidden lg:table-cell',
      render: (spa) => (
        <ImageThumbnail
          src={spa.signatureUrl}
          alt={`Signature for ${spa.employeeName}`}
          title="Signature"
          size="sm"
        />
      ),
    },
    {
      key: 'location',
      header: 'Location',
      className: 'hidden xl:table-cell',
      headerClassName: 'hidden xl:table-cell',
      render: (spa) => (
        <div className="text-sm">
          {spa.latitude && spa.longitude ? (
            <div className="flex items-center gap-1 text-muted-foreground">
              <MapPin className="h-3 w-3" />
              <span>
                {spa.latitude.toFixed(4)}, {spa.longitude.toFixed(4)}
              </span>
            </div>
          ) : (
            <span className="text-muted-foreground">-</span>
          )}
          {spa.distanceToSite !== undefined && spa.distanceToSite !== null && (
            <div className="text-xs text-muted-foreground">
              {spa.distanceToSite.toFixed(0)}m from site
            </div>
          )}
        </div>
      ),
    },
    {
      key: 'weatherConditions',
      header: 'Weather',
      className: 'hidden xl:table-cell',
      headerClassName: 'hidden xl:table-cell',
      render: (spa) => (
        <div className="flex items-center gap-1 text-sm text-muted-foreground">
          {spa.weatherConditions ? (
            <>
              <Cloud className="h-3 w-3" />
              {spa.weatherConditions}
            </>
          ) : (
            '-'
          )}
        </div>
      ),
    },
    {
      key: 'createdAt',
      header: 'Submitted',
      className: 'hidden lg:table-cell',
      headerClassName: 'hidden lg:table-cell',
      render: (spa) => (
        <div className="text-sm text-muted-foreground">
          {format(new Date(spa.createdAt), 'HH:mm')}
          <div className="text-xs">{format(new Date(spa.createdAt), 'dd/MM/yyyy')}</div>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Site Photo Attendance</h1>
          <p className="text-muted-foreground">
            View and manage site photo attendance records
          </p>
        </div>
        <Link href="/site-attendance/spa/new">
          <Button className="gap-2">
            <Plus className="h-4 w-4" />
            New SPA
          </Button>
        </Link>
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
                updateUrlParams({ fromDate: format(today, 'yyyy-MM-dd'), toDate: format(today, 'yyyy-MM-dd'), page: 1 });
              }}
            >
              Today
            </Button>
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

      {/* SPA Records Table */}
      <DataTable
        columns={columns}
        data={spaData?.items || []}
        isLoading={isLoading}
        emptyMessage="No site photo attendance records found"
        keyExtractor={(spa) => spa.id}
        pagination={{
          pageNumber: spaData?.pageNumber || 1,
          pageSize: spaData?.pageSize || 20,
          totalCount: spaData?.totalCount || 0,
          totalPages: spaData?.totalPages || 0,
        }}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />
    </div>
  );
}
