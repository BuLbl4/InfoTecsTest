using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsvProcessor.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MinorUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ValueRecords_FileName_Date",
                table: "ValueRecords");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "Results",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Results",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ValueRecords_FileName_Date",
                table: "ValueRecords",
                columns: new[] { "FileName", "Date" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ValueRecords_FileName_RowNumber",
                table: "ValueRecords",
                columns: new[] { "FileName", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_FileName",
                table: "Results",
                column: "FileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ValueRecords_FileName_Date",
                table: "ValueRecords");

            migrationBuilder.DropIndex(
                name: "IX_ValueRecords_FileName_RowNumber",
                table: "ValueRecords");

            migrationBuilder.DropIndex(
                name: "IX_Results_FileName",
                table: "Results");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "Results",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Results",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_ValueRecords_FileName_Date",
                table: "ValueRecords",
                columns: new[] { "FileName", "Date" },
                descending: new[] { false, true });
        }
    }
}
