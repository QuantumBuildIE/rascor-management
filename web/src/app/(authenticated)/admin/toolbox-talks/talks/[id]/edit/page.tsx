'use client';

import { useParams, useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { ChevronLeft } from 'lucide-react';
import { ToolboxTalkForm } from '@/features/toolbox-talks/components/ToolboxTalkForm';

export default function EditToolboxTalkPage() {
  const params = useParams();
  const router = useRouter();
  const talkId = params.id as string;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => router.push('/admin/toolbox-talks/talks')}>
          <ChevronLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Edit Toolbox Talk</h1>
          <p className="text-muted-foreground">
            Update toolbox talk content and settings
          </p>
        </div>
      </div>

      <ToolboxTalkForm talkId={talkId} />
    </div>
  );
}
