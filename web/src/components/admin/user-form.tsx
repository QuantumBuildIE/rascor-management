"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { useCreateUser, useUpdateUser, type User } from "@/lib/api/admin/use-users";
import { useRoles, type Role } from "@/lib/api/admin/use-roles";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

// Password strength calculation
function calculatePasswordStrength(password: string): {
  score: number;
  label: string;
  color: string;
} {
  let score = 0;
  if (password.length >= 8) score++;
  if (password.length >= 12) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;

  if (score <= 2) return { score, label: "Weak", color: "bg-red-500" };
  if (score <= 4) return { score, label: "Medium", color: "bg-yellow-500" };
  return { score, label: "Strong", color: "bg-green-500" };
}

const createUserSchema = z
  .object({
    email: z.string().min(1, "Email is required").email("Invalid email address"),
    firstName: z.string().min(1, "First name is required").max(100),
    lastName: z.string().min(1, "Last name is required").max(100),
    password: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string().min(1, "Please confirm your password"),
    roleIds: z.array(z.string()).min(1, "At least one role is required"),
    isActive: z.boolean(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

const updateUserSchema = z.object({
  email: z.string().email("Invalid email address"),
  firstName: z.string().min(1, "First name is required").max(100),
  lastName: z.string().min(1, "Last name is required").max(100),
  roleIds: z.array(z.string()).min(1, "At least one role is required"),
  isActive: z.boolean(),
});

type CreateUserFormValues = z.infer<typeof createUserSchema>;
type UpdateUserFormValues = z.infer<typeof updateUserSchema>;

interface UserFormProps {
  user?: User;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function UserForm({ user, onSuccess, onCancel }: UserFormProps) {
  const isEditing = !!user;

  const createUser = useCreateUser();
  const updateUser = useUpdateUser();
  const { data: roles, isLoading: rolesLoading } = useRoles();

  const form = useForm<CreateUserFormValues | UpdateUserFormValues>({
    resolver: zodResolver(isEditing ? updateUserSchema : createUserSchema),
    defaultValues: {
      email: user?.email ?? "",
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      ...(isEditing ? {} : { password: "", confirmPassword: "" }),
      roleIds: user?.roles.map((r) => r.id) ?? [],
      isActive: user?.isActive ?? true,
    },
  });

  // Only watch password for create mode
  const watchedPassword = !isEditing ? form.watch("password") : undefined;
  const passwordStrength = watchedPassword && typeof watchedPassword === "string"
    ? calculatePasswordStrength(watchedPassword)
    : null;

  const isSubmitting = createUser.isPending || updateUser.isPending;

  async function onSubmit(values: CreateUserFormValues | UpdateUserFormValues) {
    try {
      if (isEditing) {
        const updateValues = values as UpdateUserFormValues;
        await updateUser.mutateAsync({
          id: user.id,
          data: {
            firstName: updateValues.firstName,
            lastName: updateValues.lastName,
            isActive: updateValues.isActive,
            roleIds: updateValues.roleIds,
          },
        });
        toast.success("User updated successfully");
      } else {
        const createValues = values as CreateUserFormValues;
        await createUser.mutateAsync({
          email: createValues.email,
          firstName: createValues.firstName,
          lastName: createValues.lastName,
          password: createValues.password,
          confirmPassword: createValues.confirmPassword,
          isActive: createValues.isActive,
          roleIds: createValues.roleIds,
        });
        toast.success("User created successfully");
      }
      onSuccess?.();
    } catch (error) {
      const message = error instanceof Error ? error.message : "An error occurred";
      toast.error(isEditing ? "Failed to update user" : "Failed to create user", {
        description: message,
      });
    }
  }

  const handleRoleToggle = (roleId: string, checked: boolean) => {
    const currentRoles = form.getValues("roleIds") as string[];
    if (checked) {
      form.setValue("roleIds", [...currentRoles, roleId], { shouldValidate: true });
    } else {
      form.setValue(
        "roleIds",
        currentRoles.filter((id) => id !== roleId),
        { shouldValidate: true }
      );
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email *</FormLabel>
              <FormControl>
                <Input
                  type="email"
                  placeholder="e.g., john.smith@example.com"
                  disabled={isEditing}
                  {...field}
                />
              </FormControl>
              {isEditing && (
                <FormDescription>Email cannot be changed after creation</FormDescription>
              )}
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid gap-6 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>First Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., John" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Last Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., Smith" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {!isEditing && (
          <>
            <FormField
              control={form.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Password *</FormLabel>
                  <FormControl>
                    <Input type="password" placeholder="Enter password" {...field} />
                  </FormControl>
                  {passwordStrength && (
                    <div className="mt-2 space-y-1">
                      <div className="flex items-center gap-2">
                        <div className="h-2 flex-1 rounded-full bg-muted overflow-hidden">
                          <div
                            className={cn(
                              "h-full transition-all",
                              passwordStrength.color
                            )}
                            style={{ width: `${(passwordStrength.score / 6) * 100}%` }}
                          />
                        </div>
                        <span
                          className={cn(
                            "text-xs font-medium",
                            passwordStrength.label === "Weak" && "text-red-500",
                            passwordStrength.label === "Medium" && "text-yellow-500",
                            passwordStrength.label === "Strong" && "text-green-500"
                          )}
                        >
                          {passwordStrength.label}
                        </span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        Use 8+ characters with uppercase, lowercase, numbers, and symbols
                      </p>
                    </div>
                  )}
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="confirmPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Confirm Password *</FormLabel>
                  <FormControl>
                    <Input type="password" placeholder="Confirm password" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </>
        )}

        <FormField
          control={form.control}
          name="roleIds"
          render={() => (
            <FormItem>
              <FormLabel>Roles *</FormLabel>
              <FormDescription>Select one or more roles for this user</FormDescription>
              <div className="mt-2 space-y-3 rounded-md border p-4">
                {rolesLoading ? (
                  <div className="animate-pulse text-sm text-muted-foreground">
                    Loading roles...
                  </div>
                ) : roles && roles.length > 0 ? (
                  roles.map((role: Role) => {
                    const currentRoles = form.watch("roleIds") as string[];
                    const isChecked = currentRoles.includes(role.id);
                    return (
                      <div
                        key={role.id}
                        className="flex items-start space-x-3 space-y-0"
                      >
                        <Checkbox
                          id={`role-${role.id}`}
                          checked={isChecked}
                          onCheckedChange={(checked) =>
                            handleRoleToggle(role.id, checked as boolean)
                          }
                        />
                        <div className="space-y-1 leading-none">
                          <label
                            htmlFor={`role-${role.id}`}
                            className="text-sm font-medium leading-none cursor-pointer"
                          >
                            {role.name}
                          </label>
                          {role.description && (
                            <p className="text-xs text-muted-foreground">
                              {role.description}
                            </p>
                          )}
                        </div>
                      </div>
                    );
                  })
                ) : (
                  <div className="text-sm text-muted-foreground">No roles available</div>
                )}
              </div>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="isActive"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
              <FormControl>
                <Checkbox checked={field.value} onCheckedChange={field.onChange} />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel>Active</FormLabel>
                <FormDescription>
                  Inactive users cannot log in to the system
                </FormDescription>
              </div>
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <LoadingSpinner className="mr-2 h-4 w-4" />
                {isEditing ? "Updating..." : "Creating..."}
              </>
            ) : isEditing ? (
              "Update User"
            ) : (
              "Create User"
            )}
          </Button>
        </div>
      </form>
    </Form>
  );
}

function LoadingSpinner({ className }: { className?: string }) {
  return (
    <svg
      className={`animate-spin ${className}`}
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );
}
