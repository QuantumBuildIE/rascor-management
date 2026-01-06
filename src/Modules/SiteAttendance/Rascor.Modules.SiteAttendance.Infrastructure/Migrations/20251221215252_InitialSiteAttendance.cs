using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rascor.Modules.SiteAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSiteAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "site_attendance");

            migrationBuilder.CreateTable(
                name: "attendance_settings",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedHoursPerDay = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    WorkStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    LateThresholdMinutes = table.Column<int>(type: "integer", nullable: false),
                    IncludeSaturday = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeSunday = table.Column<bool>(type: "boolean", nullable: false),
                    GeofenceRadiusMeters = table.Column<int>(type: "integer", nullable: false),
                    NoiseThresholdMeters = table.Column<int>(type: "integer", nullable: false),
                    SpaGracePeriodMinutes = table.Column<int>(type: "integer", nullable: false),
                    EnablePushNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    EnableEmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    EnableSmsNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NotificationMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_summaries",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    FirstEntry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastExit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeOnSiteMinutes = table.Column<int>(type: "integer", nullable: false),
                    ExpectedHours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    UtilizationPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntryCount = table.Column<int>(type: "integer", nullable: false),
                    ExitCount = table.Column<int>(type: "integer", nullable: false),
                    HasSpa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_summaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_summaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_summaries_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "public",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bank_holidays",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "device_registrations",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PushToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_device_registrations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "site_photo_attendances",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WeatherConditions = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DistanceToSite = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_photo_attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_site_photo_attendances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_site_photo_attendances_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "public",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attendance_events",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: true),
                    TriggerMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DeviceRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsNoise = table.Column<bool>(type: "boolean", nullable: false),
                    NoiseDistance = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_events_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_events_Sites_SiteId",
                        column: x => x.SiteId,
                        principalSchema: "public",
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_events_device_registrations_DeviceRegistrationId",
                        column: x => x.DeviceRegistrationId,
                        principalSchema: "site_attendance",
                        principalTable: "device_registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "attendance_notifications",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RelatedEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_notifications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_notifications_attendance_events_RelatedEventId",
                        column: x => x.RelatedEventId,
                        principalSchema: "site_attendance",
                        principalTable: "attendance_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_DeviceRegistrationId",
                schema: "site_attendance",
                table: "attendance_events",
                column: "DeviceRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_EmployeeId",
                schema: "site_attendance",
                table: "attendance_events",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_SiteId",
                schema: "site_attendance",
                table: "attendance_events",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_TenantId_EmployeeId",
                schema: "site_attendance",
                table: "attendance_events",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_TenantId_Processed",
                schema: "site_attendance",
                table: "attendance_events",
                columns: new[] { "TenantId", "Processed" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_TenantId_SiteId",
                schema: "site_attendance",
                table: "attendance_events",
                columns: new[] { "TenantId", "SiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_Timestamp",
                schema: "site_attendance",
                table: "attendance_events",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_notifications_EmployeeId",
                schema: "site_attendance",
                table: "attendance_notifications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_notifications_NotificationType",
                schema: "site_attendance",
                table: "attendance_notifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_notifications_RelatedEventId",
                schema: "site_attendance",
                table: "attendance_notifications",
                column: "RelatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_notifications_SentAt",
                schema: "site_attendance",
                table: "attendance_notifications",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_notifications_TenantId_Delivered",
                schema: "site_attendance",
                table: "attendance_notifications",
                columns: new[] { "TenantId", "Delivered" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_notifications_TenantId_EmployeeId",
                schema: "site_attendance",
                table: "attendance_notifications",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_settings_TenantId",
                schema: "site_attendance",
                table: "attendance_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_EmployeeId",
                schema: "site_attendance",
                table: "attendance_summaries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_SiteId",
                schema: "site_attendance",
                table: "attendance_summaries",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_Status",
                schema: "site_attendance",
                table: "attendance_summaries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_TenantId_Date",
                schema: "site_attendance",
                table: "attendance_summaries",
                columns: new[] { "TenantId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_TenantId_EmployeeId",
                schema: "site_attendance",
                table: "attendance_summaries",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_TenantId_EmployeeId_SiteId_Date",
                schema: "site_attendance",
                table: "attendance_summaries",
                columns: new[] { "TenantId", "EmployeeId", "SiteId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_summaries_TenantId_SiteId",
                schema: "site_attendance",
                table: "attendance_summaries",
                columns: new[] { "TenantId", "SiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_bank_holidays_Date",
                schema: "site_attendance",
                table: "bank_holidays",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_bank_holidays_TenantId_Date",
                schema: "site_attendance",
                table: "bank_holidays",
                columns: new[] { "TenantId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_EmployeeId",
                schema: "site_attendance",
                table: "device_registrations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_IsActive",
                schema: "site_attendance",
                table: "device_registrations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_TenantId_DeviceIdentifier",
                schema: "site_attendance",
                table: "device_registrations",
                columns: new[] { "TenantId", "DeviceIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_TenantId_EmployeeId",
                schema: "site_attendance",
                table: "device_registrations",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_site_photo_attendances_EmployeeId",
                schema: "site_attendance",
                table: "site_photo_attendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_site_photo_attendances_SiteId",
                schema: "site_attendance",
                table: "site_photo_attendances",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_site_photo_attendances_TenantId_EmployeeId",
                schema: "site_attendance",
                table: "site_photo_attendances",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_site_photo_attendances_TenantId_EmployeeId_EventDate",
                schema: "site_attendance",
                table: "site_photo_attendances",
                columns: new[] { "TenantId", "EmployeeId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_site_photo_attendances_TenantId_EventDate",
                schema: "site_attendance",
                table: "site_photo_attendances",
                columns: new[] { "TenantId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_site_photo_attendances_TenantId_SiteId",
                schema: "site_attendance",
                table: "site_photo_attendances",
                columns: new[] { "TenantId", "SiteId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_notifications",
                schema: "site_attendance");

            migrationBuilder.DropTable(
                name: "attendance_settings",
                schema: "site_attendance");

            migrationBuilder.DropTable(
                name: "attendance_summaries",
                schema: "site_attendance");

            migrationBuilder.DropTable(
                name: "bank_holidays",
                schema: "site_attendance");

            migrationBuilder.DropTable(
                name: "site_photo_attendances",
                schema: "site_attendance");

            migrationBuilder.DropTable(
                name: "attendance_events",
                schema: "site_attendance");

            migrationBuilder.DropTable(
                name: "device_registrations",
                schema: "site_attendance");
        }
    }
}
