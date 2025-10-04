using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurguWebsite.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ddds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "CaseStudyMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "CaseStudyMetrics");
        }
    }
}
