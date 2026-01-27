'use client';

import * as React from 'react';
import { X, ZoomIn, ZoomOut, RotateCw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { cn } from '@/lib/utils';

interface ImagePreviewModalProps {
  src: string | null;
  alt?: string;
  title?: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ImagePreviewModal({
  src,
  alt = 'Preview',
  title = 'Image Preview',
  open,
  onOpenChange,
}: ImagePreviewModalProps) {
  const [zoom, setZoom] = React.useState(1);
  const [rotation, setRotation] = React.useState(0);

  // Reset zoom and rotation when modal opens/closes
  React.useEffect(() => {
    if (!open) {
      setZoom(1);
      setRotation(0);
    }
  }, [open]);

  const handleZoomIn = () => {
    setZoom((prev) => Math.min(prev + 0.25, 3));
  };

  const handleZoomOut = () => {
    setZoom((prev) => Math.max(prev - 0.25, 0.5));
  };

  const handleRotate = () => {
    setRotation((prev) => (prev + 90) % 360);
  };

  if (!src) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl p-0 gap-0 overflow-hidden">
        <DialogHeader className="p-4 border-b">
          <div className="flex items-center justify-between">
            <DialogTitle>{title}</DialogTitle>
            <div className="flex items-center gap-1">
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={handleZoomOut}
                disabled={zoom <= 0.5}
                className="h-8 w-8"
              >
                <ZoomOut className="h-4 w-4" />
              </Button>
              <span className="text-sm text-muted-foreground w-12 text-center">
                {Math.round(zoom * 100)}%
              </span>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={handleZoomIn}
                disabled={zoom >= 3}
                className="h-8 w-8"
              >
                <ZoomIn className="h-4 w-4" />
              </Button>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={handleRotate}
                className="h-8 w-8"
              >
                <RotateCw className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </DialogHeader>
        <div className="relative overflow-auto bg-muted/30 max-h-[70vh] flex items-center justify-center p-4">
          <img
            src={src}
            alt={alt}
            className={cn(
              'max-w-full max-h-full object-contain transition-transform duration-200'
            )}
            style={{
              transform: `scale(${zoom}) rotate(${rotation}deg)`,
            }}
          />
        </div>
      </DialogContent>
    </Dialog>
  );
}

// Thumbnail component for triggering the modal
interface ImageThumbnailProps {
  src: string | null | undefined;
  alt?: string;
  title?: string;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
}

export function ImageThumbnail({
  src,
  alt = 'Thumbnail',
  title,
  className,
  size = 'md',
}: ImageThumbnailProps) {
  const [modalOpen, setModalOpen] = React.useState(false);

  const sizeClasses = {
    sm: 'h-10 w-10',
    md: 'h-16 w-16',
    lg: 'h-24 w-24',
  };

  if (!src) {
    return (
      <div
        className={cn(
          'rounded border bg-muted flex items-center justify-center text-muted-foreground text-xs',
          sizeClasses[size],
          className
        )}
      >
        N/A
      </div>
    );
  }

  return (
    <>
      <button
        type="button"
        onClick={() => setModalOpen(true)}
        className={cn(
          'rounded border overflow-hidden hover:opacity-80 transition-opacity focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
          sizeClasses[size],
          className
        )}
      >
        <img
          src={src}
          alt={alt}
          className="w-full h-full object-cover"
        />
      </button>
      <ImagePreviewModal
        src={src}
        alt={alt}
        title={title}
        open={modalOpen}
        onOpenChange={setModalOpen}
      />
    </>
  );
}
