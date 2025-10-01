using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurguWebsite.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixtablesd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceFeatures_DisplayOrder",
                table: "ServiceFeatures");

            migrationBuilder.DropIndex(
                name: "IX_ServiceFeatures_ServiceId",
                table: "ServiceFeatures");

            migrationBuilder.RenameColumn(
                name: "Technologies",
                table: "CaseStudies",
                newName: "_technologies");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Services",
                type: "nvarchar(265)",
                maxLength: 265,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Pages",
                type: "nvarchar(265)",
                maxLength: 265,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "CaseStudies",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFeatures_ServiceId_DisplayOrder",
                table: "ServiceFeatures",
                columns: new[] { "ServiceId", "DisplayOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceFeatures_ServiceId_DisplayOrder",
                table: "ServiceFeatures");

            migrationBuilder.RenameColumn(
                name: "_technologies",
                table: "CaseStudies",
                newName: "Technologies");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Services",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(265)",
                oldMaxLength: 265);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Pages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(265)",
                oldMaxLength: 265);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "CaseStudies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFeatures_DisplayOrder",
                table: "ServiceFeatures",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFeatures_ServiceId",
                table: "ServiceFeatures",
                column: "ServiceId");
        }
    }
}
