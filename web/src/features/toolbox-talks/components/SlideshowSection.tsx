'use client';

import { useToolboxTalkSlides } from '@/lib/api/toolbox-talks/use-my-toolbox-talks';
import { Slideshow } from './Slideshow';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Presentation } from 'lucide-react';

interface SlideshowSectionProps {
  scheduledTalkId: string;
  languageCode?: string;
}

export function SlideshowSection({
  scheduledTalkId,
  languageCode,
}: SlideshowSectionProps) {
  const { data, isLoading, error } = useToolboxTalkSlides(
    scheduledTalkId,
    languageCode
  );
  const slides = data ?? [];

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <Presentation className="h-5 w-5" />
            Presentation
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Skeleton className="aspect-[16/9] w-full rounded-lg" />
        </CardContent>
      </Card>
    );
  }

  if (error || slides.length === 0) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-lg">
          <Presentation className="h-5 w-5" />
          Presentation
          <span className="text-sm font-normal text-muted-foreground">
            ({slides.length} slides)
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <Slideshow slides={slides} />
      </CardContent>
    </Card>
  );
}
