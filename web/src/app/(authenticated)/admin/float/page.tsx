"use client";

import * as React from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  useFloatStatus,
  useFloatSync,
  useSpaCheck,
  useUnmatchedSummary,
  useUnmatchedItems,
  useLinkFloatPerson,
  useLinkFloatProject,
  useIgnoreUnmatchedItem,
  useAvailableEmployees,
  useAvailableSites,
  useLinkingSummary,
  useLinkedEmployees,
  useLinkedSites,
  useUnlinkEmployee,
  useUnlinkSite,
  type FloatUnmatchedItem,
} from "@/lib/api/admin/use-float";
import { toast } from "sonner";

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = React.useState(value);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default function FloatIntegrationPage() {
  const [activeTab, setActiveTab] = React.useState<
    "unmatched" | "linkedPeople" | "linkedProjects"
  >("unmatched");
  const [filter, setFilter] = React.useState<"all" | "Person" | "Project">(
    "all"
  );
  const [linkModalOpen, setLinkModalOpen] = React.useState(false);
  const [selectedItem, setSelectedItem] =
    React.useState<FloatUnmatchedItem | null>(null);
  const [searchTerm, setSearchTerm] = React.useState("");
  const [selectedTargetId, setSelectedTargetId] = React.useState<string>("");

  const debouncedSearch = useDebounce(searchTerm, 300);

  // Queries
  const { data: status, isLoading: statusLoading } = useFloatStatus();
  const { data: summary, isLoading: summaryLoading } = useUnmatchedSummary();
  const { data: linkingSummary } = useLinkingSummary();
  const { data: linkedEmployees, isLoading: linkedEmployeesLoading } =
    useLinkedEmployees();
  const { data: linkedSites, isLoading: linkedSitesLoading } = useLinkedSites();
  const { data: unmatchedData, isLoading: unmatchedLoading } = useUnmatchedItems(
    {
      itemType: filter === "all" ? undefined : filter,
      status: "Pending",
      pageSize: 100,
    }
  );
  const { data: availableEmployees } = useAvailableEmployees(
    selectedItem?.itemType === "Person" ? debouncedSearch : undefined
  );
  const { data: availableSites } = useAvailableSites(
    selectedItem?.itemType === "Project" ? debouncedSearch : undefined
  );

  // Mutations
  const syncMutation = useFloatSync();
  const spaCheckMutation = useSpaCheck();
  const linkPersonMutation = useLinkFloatPerson();
  const linkProjectMutation = useLinkFloatProject();
  const ignoreMutation = useIgnoreUnmatchedItem();
  const unlinkEmployeeMutation = useUnlinkEmployee();
  const unlinkSiteMutation = useUnlinkSite();

  const handleSync = async () => {
    try {
      const result = await syncMutation.mutateAsync();
      toast.success(
        `Sync complete: ${result.peopleMatched} people matched, ${result.projectsMatched} projects matched`
      );
      if (result.errors.length > 0) {
        toast.warning(`${result.errors.length} errors occurred during sync`);
      }
    } catch {
      toast.error("Sync failed. Please try again.");
    }
  };

  const handleSpaCheck = async () => {
    try {
      const result = await spaCheckMutation.mutateAsync(undefined);
      toast.success(
        `SPA Check complete: ${result.remindersSent} reminders sent, ${result.spaSubmitted} already submitted`
      );
      if (result.errors.length > 0) {
        toast.warning(
          `${result.errors.length} errors occurred during SPA check`
        );
      }
    } catch {
      toast.error("SPA Check failed. Please try again.");
    }
  };

  const openLinkModal = (item: FloatUnmatchedItem) => {
    setSelectedItem(item);
    setSearchTerm("");
    setSelectedTargetId(item.suggestedMatchId || "");
    setLinkModalOpen(true);
  };

  const handleLink = async () => {
    if (!selectedItem || !selectedTargetId) return;

    try {
      if (selectedItem.itemType === "Person") {
        await linkPersonMutation.mutateAsync({
          id: selectedItem.id,
          employeeId: selectedTargetId,
        });
      } else {
        await linkProjectMutation.mutateAsync({
          id: selectedItem.id,
          siteId: selectedTargetId,
        });
      }

      toast.success("Linked successfully");
      setLinkModalOpen(false);
      setSelectedItem(null);
      setSelectedTargetId("");
    } catch {
      toast.error("Failed to link. Please try again.");
    }
  };

  const handleIgnore = async (item: FloatUnmatchedItem) => {
    if (
      !window.confirm(
        `Ignore "${item.floatName}"? It won't appear in future SPA checks.`
      )
    )
      return;

    try {
      await ignoreMutation.mutateAsync(item.id);
      toast.success("Item ignored");
    } catch {
      toast.error("Failed to ignore. Please try again.");
    }
  };

  const handleUnlinkEmployee = async (
    employeeId: string,
    employeeName: string
  ) => {
    if (
      !window.confirm(
        `Unlink "${employeeName}" from Float? They will need to be re-linked manually.`
      )
    )
      return;

    try {
      await unlinkEmployeeMutation.mutateAsync(employeeId);
      toast.success("Employee unlinked from Float");
    } catch {
      toast.error("Failed to unlink employee. Please try again.");
    }
  };

  const handleUnlinkSite = async (siteId: string, siteName: string) => {
    if (
      !window.confirm(
        `Unlink "${siteName}" from Float? It will need to be re-linked manually.`
      )
    )
      return;

    try {
      await unlinkSiteMutation.mutateAsync(siteId);
      toast.success("Site unlinked from Float");
    } catch {
      toast.error("Failed to unlink site. Please try again.");
    }
  };

  const formatLinkMethod = (method?: string) => {
    if (!method) return "Unknown";
    switch (method) {
      case "Auto-Email":
        return (
          <span className="inline-flex rounded bg-green-100 px-2 py-1 text-xs text-green-800 dark:bg-green-900 dark:text-green-200">
            Auto-Email
          </span>
        );
      case "Auto-Name":
        return (
          <span className="inline-flex rounded bg-blue-100 px-2 py-1 text-xs text-blue-800 dark:bg-blue-900 dark:text-blue-200">
            Auto-Name
          </span>
        );
      case "Manual":
        return (
          <span className="inline-flex rounded bg-purple-100 px-2 py-1 text-xs text-purple-800 dark:bg-purple-900 dark:text-purple-200">
            Manual
          </span>
        );
      default:
        return (
          <span className="inline-flex rounded bg-gray-100 px-2 py-1 text-xs text-gray-800 dark:bg-gray-800 dark:text-gray-200">
            {method}
          </span>
        );
    }
  };

  const formatConfidence = (confidence?: number) => {
    if (!confidence) return null;
    const percent = Math.round(confidence * 100);
    if (percent >= 80) {
      return (
        <span className="text-green-600 dark:text-green-400">
          {percent}% match
        </span>
      );
    } else if (percent >= 60) {
      return (
        <span className="text-yellow-600 dark:text-yellow-400">
          {percent}% match
        </span>
      );
    } else {
      return (
        <span className="text-red-600 dark:text-red-400">{percent}% match</span>
      );
    }
  };

  const isLoading = statusLoading || summaryLoading || unmatchedLoading;
  const unmatchedItems = unmatchedData?.items ?? [];
  const isLinking =
    linkPersonMutation.isPending || linkProjectMutation.isPending;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          Float Integration
        </h1>
        <p className="text-muted-foreground">
          Manage Float scheduling integration and resolve unmatched items
        </p>
      </div>

      {/* Connection Status and Actions Cards - Side by Side */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {/* Connection Status Card */}
        <Card>
          <CardHeader>
            <CardTitle>Connection Status</CardTitle>
          <CardDescription>
            Current Float API connection status and settings
          </CardDescription>
        </CardHeader>
        <CardContent>
          {statusLoading ? (
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
              <span className="text-muted-foreground">Checking...</span>
            </div>
          ) : status ? (
            <div className="flex flex-wrap items-center gap-4">
              <Badge
                variant={
                  status.isConfigured && status.connectionTest?.success
                    ? "default"
                    : "destructive"
                }
              >
                {status.isConfigured && status.connectionTest?.success
                  ? "Connected"
                  : "Not Connected"}
              </Badge>
              {status.connectionTest && status.connectionTest.success && (
                <span className="text-sm text-muted-foreground">
                  {status.connectionTest.peopleCount} people,{" "}
                  {status.connectionTest.projectsCount} projects in Float
                </span>
              )}
              {status.connectionTest?.error && (
                <span className="text-sm text-destructive">
                  {status.connectionTest.error}
                </span>
              )}
              {!status.isEnabled && (
                <span className="text-sm text-muted-foreground">
                  (Integration disabled)
                </span>
              )}
            </div>
          ) : (
            <span className="text-muted-foreground">Unable to check status</span>
          )}
        </CardContent>
      </Card>

      {/* Actions Card */}
      <Card>
        <CardHeader>
          <CardTitle>Actions</CardTitle>
          <CardDescription>
            Manually trigger sync and SPA check operations
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-4">
            <Button
              onClick={handleSync}
              disabled={syncMutation.isPending || !status?.isEnabled}
            >
              {syncMutation.isPending ? (
                <>
                  <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                  Syncing...
                </>
              ) : (
                "Sync Float Data"
              )}
            </Button>
            <Button
              variant="secondary"
              onClick={handleSpaCheck}
              disabled={spaCheckMutation.isPending || !status?.isEnabled}
            >
              {spaCheckMutation.isPending ? (
                <>
                  <span className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                  Checking...
                </>
              ) : (
                "Run SPA Check Now"
              )}
            </Button>
          </div>
          <p className="mt-2 text-sm text-muted-foreground">
            Sync pulls latest people and projects from Float. SPA Check sends
            reminders for missing submissions.
          </p>
        </CardContent>
      </Card>
      </div>

      {/* Linking Statistics */}
      {linkingSummary && (
        <Card>
          <CardHeader>
            <CardTitle>Linking Statistics</CardTitle>
            <CardDescription>
              Overview of Float people and projects linked to local entities
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
              <div className="rounded-lg bg-muted p-3 text-center">
                <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {linkingSummary.floatPeopleTotal}
                </div>
                <div className="text-sm text-muted-foreground">Float People</div>
              </div>
              <div className="rounded-lg bg-muted p-3 text-center">
                <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                  {linkingSummary.linkedPeople}
                </div>
                <div className="text-sm text-muted-foreground">Linked</div>
              </div>
              <div className="rounded-lg bg-muted p-3 text-center">
                <div className="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {linkingSummary.floatProjectsTotal}
                </div>
                <div className="text-sm text-muted-foreground">
                  Float Projects
                </div>
              </div>
              <div className="rounded-lg bg-muted p-3 text-center">
                <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                  {linkingSummary.linkedProjects}
                </div>
                <div className="text-sm text-muted-foreground">Linked</div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Unmatched Summary Alert */}
      {summary && summary.totalPending > 0 && (
        <Card className="border-yellow-200 bg-yellow-50 dark:border-yellow-900 dark:bg-yellow-950">
          <CardHeader className="pb-2">
            <CardTitle className="text-yellow-800 dark:text-yellow-200">
              {summary.totalPending} Unmatched Items Need Attention
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-yellow-700 dark:text-yellow-300">
              {summary.pendingPeople > 0 &&
                `${summary.pendingPeople} Float people not matched to employees. `}
              {summary.pendingProjects > 0 &&
                `${summary.pendingProjects} Float projects not matched to sites.`}
            </p>
            <p className="mt-1 text-sm text-yellow-600 dark:text-yellow-400">
              These items will be skipped during SPA checks until matched or
              ignored.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Tabbed Content */}
      <Card>
        {/* Tabs */}
        <div className="border-b">
          <nav className="-mb-px flex">
            <button
              onClick={() => setActiveTab("unmatched")}
              className={`border-b-2 px-6 py-3 text-sm font-medium ${
                activeTab === "unmatched"
                  ? "border-primary text-primary"
                  : "border-transparent text-muted-foreground hover:text-foreground"
              }`}
            >
              Unmatched (
              {(linkingSummary?.unmatchedPeople || 0) +
                (linkingSummary?.unmatchedProjects || 0)}
              )
            </button>
            <button
              onClick={() => setActiveTab("linkedPeople")}
              className={`border-b-2 px-6 py-3 text-sm font-medium ${
                activeTab === "linkedPeople"
                  ? "border-primary text-primary"
                  : "border-transparent text-muted-foreground hover:text-foreground"
              }`}
            >
              Linked People ({linkingSummary?.linkedPeople || 0})
            </button>
            <button
              onClick={() => setActiveTab("linkedProjects")}
              className={`border-b-2 px-6 py-3 text-sm font-medium ${
                activeTab === "linkedProjects"
                  ? "border-primary text-primary"
                  : "border-transparent text-muted-foreground hover:text-foreground"
              }`}
            >
              Linked Projects ({linkingSummary?.linkedProjects || 0})
            </button>
          </nav>
        </div>

        {/* Tab Content */}
        <CardContent className="pt-4">
          {/* Unmatched Tab */}
          {activeTab === "unmatched" && (
            <>
              <div className="mb-4 flex gap-2">
                <Button
                  variant={filter === "all" ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilter("all")}
                >
                  All
                </Button>
                <Button
                  variant={filter === "Person" ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilter("Person")}
                >
                  People ({summary?.pendingPeople || 0})
                </Button>
                <Button
                  variant={filter === "Project" ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilter("Project")}
                >
                  Projects ({summary?.pendingProjects || 0})
                </Button>
              </div>

              {isLoading ? (
                <div className="flex items-center justify-center py-8">
                  <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
                </div>
              ) : unmatchedItems.length === 0 ? (
                <div className="py-8 text-center text-muted-foreground">
                  No unmatched items. All Float data is linked!
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Type
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Float Name
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Email
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Suggested Match
                        </th>
                        <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {unmatchedItems.map((item) => (
                        <tr key={item.id} className="hover:bg-muted/50">
                          <td className="px-4 py-3">
                            <Badge
                              variant={
                                item.itemType === "Person"
                                  ? "default"
                                  : "secondary"
                              }
                            >
                              {item.itemType}
                            </Badge>
                          </td>
                          <td className="px-4 py-3 font-medium">
                            {item.floatName}
                          </td>
                          <td className="px-4 py-3 text-muted-foreground">
                            {item.floatEmail || "-"}
                          </td>
                          <td className="px-4 py-3">
                            {item.suggestedMatchName ? (
                              <div>
                                <span>{item.suggestedMatchName}</span>
                                <br />
                                {formatConfidence(item.matchConfidence)}
                              </div>
                            ) : (
                              <span className="text-muted-foreground">
                                No suggestion
                              </span>
                            )}
                          </td>
                          <td className="px-4 py-3 text-right">
                            <div className="flex items-center justify-end gap-2">
                              <Button
                                size="sm"
                                onClick={() => openLinkModal(item)}
                              >
                                Link
                              </Button>
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleIgnore(item)}
                                disabled={ignoreMutation.isPending}
                              >
                                Ignore
                              </Button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}

          {/* Linked People Tab */}
          {activeTab === "linkedPeople" && (
            <>
              {linkedEmployeesLoading ? (
                <div className="flex items-center justify-center py-8">
                  <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
                </div>
              ) : !linkedEmployees || linkedEmployees.length === 0 ? (
                <div className="py-8 text-center text-muted-foreground">
                  No employees linked to Float yet.
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Employee
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Float Person
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Method
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Linked
                        </th>
                        <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {linkedEmployees.map((emp) => (
                        <tr key={emp.employeeId} className="hover:bg-muted/50">
                          <td className="px-4 py-3">
                            <div className="font-medium">{emp.employeeName}</div>
                            <div className="text-sm text-muted-foreground">
                              {emp.employeeCode}
                              {emp.employeeEmail && ` • ${emp.employeeEmail}`}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            <div>
                              {emp.floatPersonName ||
                                `Float ID: ${emp.floatPersonId}`}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            {formatLinkMethod(emp.floatLinkMethod)}
                          </td>
                          <td className="px-4 py-3 text-sm text-muted-foreground">
                            {emp.floatLinkedAt
                              ? new Date(emp.floatLinkedAt).toLocaleDateString()
                              : "-"}
                          </td>
                          <td className="px-4 py-3 text-right">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() =>
                                handleUnlinkEmployee(
                                  emp.employeeId,
                                  emp.employeeName
                                )
                              }
                              disabled={unlinkEmployeeMutation.isPending}
                              className="text-destructive hover:bg-destructive/10 hover:text-destructive"
                            >
                              Unlink
                            </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}

          {/* Linked Projects Tab */}
          {activeTab === "linkedProjects" && (
            <>
              {linkedSitesLoading ? (
                <div className="flex items-center justify-center py-8">
                  <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
                </div>
              ) : !linkedSites || linkedSites.length === 0 ? (
                <div className="py-8 text-center text-muted-foreground">
                  No sites linked to Float yet.
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Site
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Float Project
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Method
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                          Linked
                        </th>
                        <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {linkedSites.map((site) => (
                        <tr key={site.siteId} className="hover:bg-muted/50">
                          <td className="px-4 py-3">
                            <div className="font-medium">{site.siteName}</div>
                            {site.siteAddress && (
                              <div className="text-sm text-muted-foreground">
                                {site.siteAddress}
                              </div>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            <div>
                              {site.floatProjectName ||
                                `Float ID: ${site.floatProjectId}`}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            {formatLinkMethod(site.floatLinkMethod)}
                          </td>
                          <td className="px-4 py-3 text-sm text-muted-foreground">
                            {site.floatLinkedAt
                              ? new Date(site.floatLinkedAt).toLocaleDateString()
                              : "-"}
                          </td>
                          <td className="px-4 py-3 text-right">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() =>
                                handleUnlinkSite(site.siteId, site.siteName)
                              }
                              disabled={unlinkSiteMutation.isPending}
                              className="text-destructive hover:bg-destructive/10 hover:text-destructive"
                            >
                              Unlink
                            </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Link Modal */}
      <Dialog open={linkModalOpen} onOpenChange={setLinkModalOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>
              Link &quot;{selectedItem?.floatName}&quot; to{" "}
              {selectedItem?.itemType === "Person" ? "Employee" : "Site"}
            </DialogTitle>
            <DialogDescription>
              Select the {selectedItem?.itemType === "Person" ? "employee" : "site"}{" "}
              to link this Float{" "}
              {selectedItem?.itemType === "Person" ? "person" : "project"} to.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <Input
              placeholder={`Search ${
                selectedItem?.itemType === "Person" ? "employees" : "sites"
              }...`}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />

            <div className="max-h-64 overflow-y-auto rounded-md border">
              {selectedItem?.itemType === "Person"
                ? (availableEmployees ?? []).map((emp) => (
                    <div
                      key={emp.id}
                      onClick={() => setSelectedTargetId(emp.id)}
                      className={`cursor-pointer p-3 hover:bg-muted/50 ${
                        selectedTargetId === emp.id
                          ? "border-l-4 border-primary bg-muted"
                          : ""
                      }`}
                    >
                      <div className="font-medium">{emp.fullName}</div>
                      <div className="text-sm text-muted-foreground">
                        {emp.employeeCode}
                        {emp.email && ` - ${emp.email}`}
                      </div>
                    </div>
                  ))
                : (availableSites ?? []).map((site) => (
                    <div
                      key={site.id}
                      onClick={() => setSelectedTargetId(site.id)}
                      className={`cursor-pointer p-3 hover:bg-muted/50 ${
                        selectedTargetId === site.id
                          ? "border-l-4 border-primary bg-muted"
                          : ""
                      }`}
                    >
                      <div className="font-medium">{site.name}</div>
                      {site.address && (
                        <div className="text-sm text-muted-foreground">
                          {site.address}
                        </div>
                      )}
                    </div>
                  ))}
              {selectedItem?.itemType === "Person" &&
                (!availableEmployees || availableEmployees.length === 0) && (
                  <div className="p-4 text-center text-muted-foreground">
                    No available employees found
                  </div>
                )}
              {selectedItem?.itemType === "Project" &&
                (!availableSites || availableSites.length === 0) && (
                  <div className="p-4 text-center text-muted-foreground">
                    No available sites found
                  </div>
                )}
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setLinkModalOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleLink}
              disabled={!selectedTargetId || isLinking}
            >
              {isLinking ? "Linking..." : "Link"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
