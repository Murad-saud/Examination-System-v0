using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examination_System.Migrations
{
    /// <inheritdoc />
    public partial class Make_CorrectAnswerId_nullabe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_CorrectAnswerId",
                table: "Questions");

            migrationBuilder.AlterColumn<int>(
                name: "CorrectAnswerId",
                table: "Questions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CorrectAnswerId",
                table: "Questions",
                column: "CorrectAnswerId",
                unique: true,
                filter: "[CorrectAnswerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_CorrectAnswerId",
                table: "Questions");

            migrationBuilder.AlterColumn<int>(
                name: "CorrectAnswerId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CorrectAnswerId",
                table: "Questions",
                column: "CorrectAnswerId",
                unique: true);
        }
    }
}
