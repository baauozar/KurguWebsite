using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurguWebsite.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class poı : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "_technologies",
                table: "CaseStudies",
                newName: "Technologies");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudies_ClientName",
                table: "CaseStudies",
                column: "ClientName");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudies_Featured_Active_DisplayOrder",
                table: "CaseStudies",
                columns: new[] { "IsFeatured", "IsActive", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CaseStudies_ClientName",
                table: "CaseStudies");

            migrationBuilder.DropIndex(
                name: "IX_CaseStudies_Featured_Active_DisplayOrder",
                table: "CaseStudies");

            migrationBuilder.RenameColumn(
                name: "Technologies",
                table: "CaseStudies",
                newName: "_technologies");
        }
    }
}
