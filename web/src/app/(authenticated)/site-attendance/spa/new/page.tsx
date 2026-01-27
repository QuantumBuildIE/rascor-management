'use client';

import * as React from 'react';
import { useRouter } from 'next/navigation';
import { format } from 'date-fns';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'sonner';
import { ArrowLeft, Camera, MapPin, Loader2, PenLine, Cloud, FileText, CheckCircle } from 'lucide-react';
import Link from 'next/link';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useAllSites } from '@/lib/api/admin/use-sites';
import { useAllEmployees } from '@/lib/api/admin/use-employees';
import { useAuth } from '@/lib/auth/use-auth';
import { useCreateSpa, useUploadSpaImage, useUploadSpaSignature } from '@/features/site-attendance/hooks/useSiteAttendance';
import { PhotoCapture } from '@/features/site-attendance/components/PhotoCapture';
import { SignaturePad } from '@/features/site-attendance/components/SignaturePad';

// Schema for the form
const spaFormSchema = z.object({
  employeeId: z.string().min(1, 'Employee is required'),
  siteId: z.string().min(1, 'Site is required'),
  eventDate: z.string().min(1, 'Date is required'),
  weatherConditions: z.string().optional(),
  notes: z.string().optional(),
});

type SpaFormValues = z.infer<typeof spaFormSchema>;

// Location state type
interface LocationState {
  latitude: number | null;
  longitude: number | null;
  accuracy: number | null;
  loading: boolean;
  error: string | null;
}

export default function NewSpaPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { data: sites, isLoading: sitesLoading } = useAllSites();
  const { data: employees, isLoading: employeesLoading } = useAllEmployees();

  const createSpa = useCreateSpa();
  const uploadImage = useUploadSpaImage();
  const uploadSignature = useUploadSpaSignature();

  const [photo, setPhoto] = React.useState<File | null>(null);
  const [signature, setSignature] = React.useState<string | null>(null);
  const [location, setLocation] = React.useState<LocationState>({
    latitude: null,
    longitude: null,
    accuracy: null,
    loading: false,
    error: null,
  });
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [uploadProgress, setUploadProgress] = React.useState<{ image: number; signature: number }>({
    image: 0,
    signature: 0,
  });

  // Initialize form
  const form = useForm<SpaFormValues>({
    resolver: zodResolver(spaFormSchema),
    defaultValues: {
      employeeId: '',
      siteId: '',
      eventDate: format(new Date(), 'yyyy-MM-dd'),
      weatherConditions: '',
      notes: '',
    },
  });

  // Auto-select current user's employee record if available
  React.useEffect(() => {
    if (employees && user?.email) {
      const userEmployee = employees.find((e) => e.email?.toLowerCase() === user.email?.toLowerCase());
      if (userEmployee) {
        form.setValue('employeeId', userEmployee.id);
      }
    }
  }, [employees, user, form]);

  // Get current location on mount
  React.useEffect(() => {
    getLocation();
  }, []);

  const getLocation = () => {
    if (!navigator.geolocation) {
      setLocation((prev) => ({ ...prev, error: 'Geolocation is not supported by your browser' }));
      return;
    }

    setLocation((prev) => ({ ...prev, loading: true, error: null }));

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLocation({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracy: position.coords.accuracy,
          loading: false,
          error: null,
        });
      },
      (error) => {
        let errorMessage = 'Failed to get location';
        switch (error.code) {
          case error.PERMISSION_DENIED:
            errorMessage = 'Location permission denied. Please enable location access.';
            break;
          case error.POSITION_UNAVAILABLE:
            errorMessage = 'Location information unavailable.';
            break;
          case error.TIMEOUT:
            errorMessage = 'Location request timed out.';
            break;
        }
        setLocation((prev) => ({ ...prev, loading: false, error: errorMessage }));
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0,
      }
    );
  };

  // Convert base64 signature to File
  const base64ToFile = (base64: string, filename: string): File => {
    const arr = base64.split(',');
    const mime = arr[0].match(/:(.*?);/)?.[1] || 'image/png';
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);
    while (n--) {
      u8arr[n] = bstr.charCodeAt(n);
    }
    return new File([u8arr], filename, { type: mime });
  };

  const onSubmit = async (data: SpaFormValues) => {
    // Validation
    if (!photo) {
      toast.error('Please capture or upload a photo');
      return;
    }
    if (!signature) {
      toast.error('Please provide your signature');
      return;
    }

    setIsSubmitting(true);
    setUploadProgress({ image: 0, signature: 0 });

    try {
      // Step 1: Create SPA record
      const spaRecord = await createSpa.mutateAsync({
        employeeId: data.employeeId,
        siteId: data.siteId,
        eventDate: data.eventDate,
        latitude: location.latitude ?? undefined,
        longitude: location.longitude ?? undefined,
        weatherConditions: data.weatherConditions || undefined,
        notes: data.notes || undefined,
      });

      // Step 2: Upload photo
      await uploadImage.mutateAsync({
        id: spaRecord.id,
        file: photo,
        onProgress: (progress) => setUploadProgress((prev) => ({ ...prev, image: progress })),
      });

      // Step 3: Upload signature
      const signatureFile = base64ToFile(signature, `signature-${spaRecord.id}.png`);
      await uploadSignature.mutateAsync({
        id: spaRecord.id,
        file: signatureFile,
        onProgress: (progress) => setUploadProgress((prev) => ({ ...prev, signature: progress })),
      });

      toast.success('Site Photo Attendance submitted successfully');
      router.push('/site-attendance/spa');
    } catch (error: any) {
      console.error('Error submitting SPA:', error);
      toast.error(error?.response?.data?.error || error?.message || 'Failed to submit Site Photo Attendance');
    } finally {
      setIsSubmitting(false);
    }
  };

  const activeSites = React.useMemo(() => sites?.filter((s) => s.isActive) || [], [sites]);
  const activeEmployees = React.useMemo(() => employees?.filter((e) => e.isActive) || [], [employees]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/site-attendance/spa">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-5 w-5" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">New Site Photo Attendance</h1>
          <p className="text-muted-foreground">
            Record your site attendance with photo verification
          </p>
        </div>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          <div className="grid gap-6 lg:grid-cols-2">
            {/* Left Column - Form Fields */}
            <div className="space-y-6">
              {/* Basic Info Card */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <FileText className="h-5 w-5 text-muted-foreground" />
                    Attendance Details
                  </CardTitle>
                  <CardDescription>
                    Select the site and date for your attendance record
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Employee Field */}
                  <FormField
                    control={form.control}
                    name="employeeId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Employee *</FormLabel>
                        <Select
                          value={field.value}
                          onValueChange={field.onChange}
                          disabled={employeesLoading || isSubmitting}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select employee" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {activeEmployees.map((emp) => (
                              <SelectItem key={emp.id} value={emp.id}>
                                {emp.firstName} {emp.lastName}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Site Field */}
                  <FormField
                    control={form.control}
                    name="siteId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Site *</FormLabel>
                        <Select
                          value={field.value}
                          onValueChange={field.onChange}
                          disabled={sitesLoading || isSubmitting}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select site" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {activeSites.map((site) => (
                              <SelectItem key={site.id} value={site.id}>
                                {site.siteCode} - {site.siteName}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Date Field */}
                  <FormField
                    control={form.control}
                    name="eventDate"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Date *</FormLabel>
                        <FormControl>
                          <Input
                            type="date"
                            {...field}
                            disabled={isSubmitting}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
              </Card>

              {/* Location Card */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <MapPin className="h-5 w-5 text-muted-foreground" />
                    Location
                  </CardTitle>
                  <CardDescription>
                    Your current GPS coordinates
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {location.loading ? (
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Getting location...
                    </div>
                  ) : location.error ? (
                    <div className="space-y-2">
                      <p className="text-sm text-destructive">{location.error}</p>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={getLocation}
                      >
                        Retry
                      </Button>
                    </div>
                  ) : location.latitude && location.longitude ? (
                    <div className="space-y-2">
                      <div className="flex items-center gap-2 text-sm">
                        <CheckCircle className="h-4 w-4 text-green-500" />
                        <span className="text-green-600 font-medium">Location captured</span>
                      </div>
                      <div className="text-sm text-muted-foreground space-y-1">
                        <p>Lat: {location.latitude.toFixed(6)}</p>
                        <p>Lng: {location.longitude.toFixed(6)}</p>
                        {location.accuracy && (
                          <p>Accuracy: {location.accuracy.toFixed(0)}m</p>
                        )}
                      </div>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={getLocation}
                      >
                        Refresh Location
                      </Button>
                    </div>
                  ) : (
                    <div className="space-y-2">
                      <p className="text-sm text-muted-foreground">Location not available</p>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={getLocation}
                      >
                        Get Location
                      </Button>
                    </div>
                  )}
                </CardContent>
              </Card>

              {/* Optional Info Card */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Cloud className="h-5 w-5 text-muted-foreground" />
                    Additional Information
                  </CardTitle>
                  <CardDescription>
                    Optional details about site conditions
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <FormField
                    control={form.control}
                    name="weatherConditions"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Weather Conditions</FormLabel>
                        <FormControl>
                          <Input
                            placeholder="e.g., Sunny, Rainy, Overcast"
                            {...field}
                            disabled={isSubmitting}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="notes"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Notes</FormLabel>
                        <FormControl>
                          <Textarea
                            placeholder="Any additional notes..."
                            className="min-h-[100px]"
                            {...field}
                            disabled={isSubmitting}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </CardContent>
              </Card>
            </div>

            {/* Right Column - Photo & Signature */}
            <div className="space-y-6">
              {/* Photo Card */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Camera className="h-5 w-5 text-muted-foreground" />
                    Photo *
                  </CardTitle>
                  <CardDescription>
                    Take a photo or upload an image as proof of attendance
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <PhotoCapture
                    value={photo}
                    onChange={setPhoto}
                    disabled={isSubmitting}
                  />
                </CardContent>
              </Card>

              {/* Signature Card */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <PenLine className="h-5 w-5 text-muted-foreground" />
                    Signature *
                  </CardTitle>
                  <CardDescription>
                    Sign below to confirm your attendance
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <SignaturePad
                    value={signature}
                    onChange={setSignature}
                    disabled={isSubmitting}
                  />
                </CardContent>
              </Card>

              {/* Upload Progress */}
              {isSubmitting && (
                <Card>
                  <CardHeader>
                    <CardTitle className="text-base">Uploading...</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <div className="space-y-1">
                      <div className="flex justify-between text-sm">
                        <span>Photo</span>
                        <span>{uploadProgress.image}%</span>
                      </div>
                      <div className="h-2 rounded-full bg-muted overflow-hidden">
                        <div
                          className="h-full rounded-full bg-primary transition-all"
                          style={{ width: `${uploadProgress.image}%` }}
                        />
                      </div>
                    </div>
                    <div className="space-y-1">
                      <div className="flex justify-between text-sm">
                        <span>Signature</span>
                        <span>{uploadProgress.signature}%</span>
                      </div>
                      <div className="h-2 rounded-full bg-muted overflow-hidden">
                        <div
                          className="h-full rounded-full bg-primary transition-all"
                          style={{ width: `${uploadProgress.signature}%` }}
                        />
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}
            </div>
          </div>

          {/* Submit Button */}
          <div className="flex justify-end gap-4">
            <Link href="/site-attendance/spa">
              <Button type="button" variant="outline" disabled={isSubmitting}>
                Cancel
              </Button>
            </Link>
            <Button type="submit" disabled={isSubmitting} className="min-w-[150px]">
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Submitting...
                </>
              ) : (
                'Submit Attendance'
              )}
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
}
