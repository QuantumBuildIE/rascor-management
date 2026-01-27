using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rascor.Modules.SiteAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureUrlToSitePhotoAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureUrl",
                schema: "site_attendance",
                table: "site_photo_attendances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureUrl",
                schema: "site_attendance",
                table: "site_photo_attendances");
        }
    }
}
