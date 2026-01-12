'use client';

import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { ChevronLeft } from 'lucide-react';
import { ToolboxTalkForm } from '@/features/toolbox-talks/components/ToolboxTalkForm';

export default function NewToolboxTalkPage() {
  const router = useRouter();

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => router.push('/admin/toolbox-talks/talks')}>
          <ChevronLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Create Toolbox Talk</h1>
          <p className="text-muted-foreground">
            Create a new safety toolbox talk
          </p>
        </div>
      </div>

      <ToolboxTalkForm />
    </div>
  );
}
