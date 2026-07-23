using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsvProcessor.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalRecord",
                table: "Results",
                newName: "TotalRecords");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ValueRecords",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ValueRecords",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "Results",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "Results",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Results",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Results",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ValueRecords_Date",
                table: "ValueRecords",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ValueRecords_FileName_Date",
                table: "ValueRecords",
                columns: new[] { "FileName", "Date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Results_AverageExecutionTime",
                table: "Results",
                column: "AverageExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_Results_AverageValue",
                table: "Results",
                column: "AverageValue");

            migrationBuilder.CreateIndex(
                name: "IX_Results_MinDate",
                table: "Results",
                column: "MinDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ValueRecords_Date",
                table: "ValueRecords");

            migrationBuilder.DropIndex(
                name: "IX_ValueRecords_FileName_Date",
                table: "ValueRecords");

            migrationBuilder.DropIndex(
                name: "IX_Results_AverageExecutionTime",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_AverageValue",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_MinDate",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ValueRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "TotalRecords",
                table: "Results",
                newName: "TotalRecord");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ValueRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "Results",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "Results",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
