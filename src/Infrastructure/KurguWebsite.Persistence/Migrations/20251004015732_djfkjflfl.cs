using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurguWebsite.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class djfkjflfl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics");

            migrationBuilder.AlterColumn<string>(
                name: "MetricValue",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MetricName",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CaseStudyMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudyMetrics_CaseStudyId_DisplayOrder",
                table: "CaseStudyMetrics",
                columns: new[] { "CaseStudyId", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics",
                column: "CaseStudyId",
                principalTable: "CaseStudies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CaseStudyMetrics_CaseStudyId_DisplayOrder",
                table: "CaseStudyMetrics");

            migrationBuilder.AlterColumn<string>(
                name: "MetricValue",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "MetricName",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "CaseStudyMetrics",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics",
                column: "CaseStudyId",
                principalTable: "CaseStudies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
