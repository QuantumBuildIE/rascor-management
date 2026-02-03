using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rascor.Modules.SiteAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceStatusCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_status_cache",
                schema: "site_attendance",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLatitude = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: true),
                    LastLongitude = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: true),
                    LastAccuracy = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    LastBatteryLevel = table.Column<int>(type: "integer", nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_status_cache", x => x.DeviceId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_device_status_cache_IsActive",
                schema: "site_attendance",
                table: "device_status_cache",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_device_status_cache_LastSeenAt",
                schema: "site_attendance",
                table: "device_status_cache",
                column: "LastSeenAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_status_cache",
                schema: "site_attendance");
        }
    }
}
