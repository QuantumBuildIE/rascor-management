'use client';

import * as React from 'react';
import { Camera, Upload, X, RefreshCw, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

interface PhotoCaptureProps {
  value?: File | null;
  onChange: (file: File | null) => void;
  disabled?: boolean;
  className?: string;
  maxSizeMB?: number;
  acceptedTypes?: string[];
}

export function PhotoCapture({
  value,
  onChange,
  disabled = false,
  className,
  maxSizeMB = 10,
  acceptedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'],
}: PhotoCaptureProps) {
  const fileInputRef = React.useRef<HTMLInputElement>(null);
  const videoRef = React.useRef<HTMLVideoElement>(null);
  const [preview, setPreview] = React.useState<string | null>(null);
  const [isCapturing, setIsCapturing] = React.useState(false);
  const [stream, setStream] = React.useState<MediaStream | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const [isCameraSupported, setIsCameraSupported] = React.useState(true);

  // Check camera support on mount
  React.useEffect(() => {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
      setIsCameraSupported(false);
    }
  }, []);

  // Generate preview when value changes
  React.useEffect(() => {
    if (value) {
      const url = URL.createObjectURL(value);
      setPreview(url);
      return () => URL.revokeObjectURL(url);
    } else {
      setPreview(null);
    }
  }, [value]);

  // Cleanup stream on unmount
  React.useEffect(() => {
    return () => {
      if (stream) {
        stream.getTracks().forEach(track => track.stop());
      }
    };
  }, [stream]);

  const validateFile = (file: File): string | null => {
    if (!acceptedTypes.includes(file.type)) {
      return `Invalid file type. Accepted types: ${acceptedTypes.join(', ')}`;
    }
    if (file.size > maxSizeMB * 1024 * 1024) {
      return `File size exceeds ${maxSizeMB}MB limit.`;
    }
    return null;
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const validationError = validateFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setError(null);
    onChange(file);

    // Reset input for re-selection of same file
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const startCamera = async () => {
    setError(null);
    try {
      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: 'environment', // Prefer back camera on mobile
          width: { ideal: 1920 },
          height: { ideal: 1080 },
        },
      });
      setStream(mediaStream);
      setIsCapturing(true);

      // Wait for next tick to ensure video element is rendered
      setTimeout(() => {
        if (videoRef.current) {
          videoRef.current.srcObject = mediaStream;
        }
      }, 0);
    } catch {
      setError('Could not access camera. Please check permissions or use file upload.');
      setIsCameraSupported(false);
    }
  };

  const capturePhoto = () => {
    if (!videoRef.current) return;

    const video = videoRef.current;
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.drawImage(video, 0, 0);

    canvas.toBlob((blob) => {
      if (blob) {
        const file = new File([blob], `spa-photo-${Date.now()}.jpg`, {
          type: 'image/jpeg',
        });
        onChange(file);
        stopCamera();
      }
    }, 'image/jpeg', 0.9);
  };

  const stopCamera = () => {
    if (stream) {
      stream.getTracks().forEach(track => track.stop());
      setStream(null);
    }
    setIsCapturing(false);
  };

  const clearPhoto = () => {
    onChange(null);
    setPreview(null);
  };

  if (disabled) {
    return (
      <div className={cn('space-y-2', className)}>
        {preview ? (
          <div className="relative rounded-lg overflow-hidden border">
            <img src={preview} alt="Captured photo" className="w-full h-64 object-cover" />
          </div>
        ) : (
          <div className="h-64 rounded-lg border border-dashed flex items-center justify-center text-muted-foreground">
            No photo captured
          </div>
        )}
      </div>
    );
  }

  return (
    <div className={cn('space-y-3', className)}>
      {/* Camera view when capturing */}
      {isCapturing && (
        <div className="relative rounded-lg overflow-hidden border bg-black">
          <video
            ref={videoRef}
            autoPlay
            playsInline
            muted
            className="w-full h-64 object-cover"
          />
          <div className="absolute bottom-4 left-0 right-0 flex justify-center gap-3">
            <Button
              type="button"
              variant="secondary"
              size="lg"
              onClick={stopCamera}
              className="gap-2"
            >
              <X className="h-5 w-5" />
              Cancel
            </Button>
            <Button
              type="button"
              size="lg"
              onClick={capturePhoto}
              className="gap-2"
            >
              <Camera className="h-5 w-5" />
              Capture
            </Button>
          </div>
        </div>
      )}

      {/* Preview when photo is captured */}
      {!isCapturing && preview && (
        <div className="relative rounded-lg overflow-hidden border">
          <img src={preview} alt="Captured photo" className="w-full h-64 object-cover" />
          <div className="absolute top-2 right-2 flex gap-2">
            <Button
              type="button"
              variant="secondary"
              size="sm"
              onClick={clearPhoto}
              className="gap-1"
            >
              <X className="h-4 w-4" />
              Remove
            </Button>
          </div>
        </div>
      )}

      {/* Capture buttons when no photo */}
      {!isCapturing && !preview && (
        <div className="h-64 rounded-lg border border-dashed flex flex-col items-center justify-center gap-4 p-4 bg-muted/30">
          <div className="text-center text-muted-foreground">
            <Camera className="h-12 w-12 mx-auto mb-2 opacity-50" />
            <p className="text-sm">Take a photo or upload an image</p>
            <p className="text-xs mt-1">Max {maxSizeMB}MB - JPEG, PNG, WebP</p>
          </div>

          <div className="flex gap-3">
            {isCameraSupported && (
              <Button
                type="button"
                variant="outline"
                onClick={startCamera}
                className="gap-2"
              >
                <Camera className="h-4 w-4" />
                Camera
              </Button>
            )}
            <Button
              type="button"
              variant="outline"
              onClick={() => fileInputRef.current?.click()}
              className="gap-2"
            >
              <Upload className="h-4 w-4" />
              Upload
            </Button>
          </div>
        </div>
      )}

      {/* Retake buttons when photo exists */}
      {!isCapturing && preview && (
        <div className="flex gap-3 justify-center">
          {isCameraSupported && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={startCamera}
              className="gap-2"
            >
              <RefreshCw className="h-4 w-4" />
              Retake
            </Button>
          )}
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => fileInputRef.current?.click()}
            className="gap-2"
          >
            <Upload className="h-4 w-4" />
            Upload New
          </Button>
        </div>
      )}

      {/* Hidden file input */}
      <input
        ref={fileInputRef}
        type="file"
        accept={acceptedTypes.join(',')}
        onChange={handleFileSelect}
        className="hidden"
      />

      {/* Error message */}
      {error && (
        <div className="p-3 rounded-lg bg-destructive/10 text-destructive text-sm">
          {error}
        </div>
      )}
    </div>
  );
}

// Loading skeleton
export function PhotoCaptureSkeleton() {
  return (
    <div className="space-y-3">
      <div className="h-64 rounded-lg bg-muted animate-pulse flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    </div>
  );
}
