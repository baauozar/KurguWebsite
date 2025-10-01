using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurguWebsite.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixtablesdddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CaseStudyMetrics_CaseStudyId",
                table: "CaseStudyMetrics");

            migrationBuilder.AlterColumn<string>(
                name: "MetricValue",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MetricName",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "CaseStudyMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CaseStudyMetrics",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CaseStudyMetrics",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "CaseStudyMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CaseStudyMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudyMetrics_CaseStudyId_MetricName",
                table: "CaseStudyMetrics",
                columns: new[] { "CaseStudyId", "MetricName" });

            migrationBuilder.AddForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics",
                column: "CaseStudyId",
                principalTable: "CaseStudies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CaseStudyMetrics_CaseStudyId_MetricName",
                table: "CaseStudyMetrics");

            migrationBuilder.AlterColumn<string>(
                name: "MetricValue",
                table: "CaseStudyMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MetricName",
                table: "CaseStudyMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "CaseStudyMetrics",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CaseStudyMetrics",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "CaseStudyMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "CaseStudyMetrics",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "CaseStudyMetrics",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudyMetrics_CaseStudyId",
                table: "CaseStudyMetrics",
                column: "CaseStudyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseStudyMetrics_CaseStudies_CaseStudyId",
                table: "CaseStudyMetrics",
                column: "CaseStudyId",
                principalTable: "CaseStudies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
