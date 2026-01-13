using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rascor.Modules.SiteAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeofenceSyncLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "geofence_sync_logs",
                schema: "site_attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SyncCompleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordsProcessed = table.Column<int>(type: "integer", nullable: false),
                    RecordsCreated = table.Column<int>(type: "integer", nullable: false),
                    RecordsSkipped = table.Column<int>(type: "integer", nullable: false),
                    LastEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastEventTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geofence_sync_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_geofence_sync_logs_TenantId_SyncCompleted",
                schema: "site_attendance",
                table: "geofence_sync_logs",
                columns: new[] { "TenantId", "SyncCompleted" },
                filter: "\"SyncCompleted\" IS NOT NULL AND \"ErrorMessage\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_geofence_sync_logs_TenantId_SyncStarted",
                schema: "site_attendance",
                table: "geofence_sync_logs",
                columns: new[] { "TenantId", "SyncStarted" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "geofence_sync_logs",
                schema: "site_attendance");
        }
    }
}
