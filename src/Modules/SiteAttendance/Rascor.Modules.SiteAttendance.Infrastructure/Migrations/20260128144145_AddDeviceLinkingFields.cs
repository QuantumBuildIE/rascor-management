using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rascor.Modules.SiteAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceLinkingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LinkedAt",
                schema: "site_attendance",
                table: "device_registrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedBy",
                schema: "site_attendance",
                table: "device_registrations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlinkedAt",
                schema: "site_attendance",
                table: "device_registrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnlinkedReason",
                schema: "site_attendance",
                table: "device_registrations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedAt",
                schema: "site_attendance",
                table: "device_registrations");

            migrationBuilder.DropColumn(
                name: "LinkedBy",
                schema: "site_attendance",
                table: "device_registrations");

            migrationBuilder.DropColumn(
                name: "UnlinkedAt",
                schema: "site_attendance",
                table: "device_registrations");

            migrationBuilder.DropColumn(
                name: "UnlinkedReason",
                schema: "site_attendance",
                table: "device_registrations");
        }
    }
}
