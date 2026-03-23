using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examination_System.Migrations
{
    /// <inheritdoc />
    public partial class Added_Relation_Between_User_And_ParticipantExamNavProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ParticipantId",
                table: "ParticipantExam",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantExam_ParticipantId",
                table: "ParticipantExam",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantExam_AspNetUsers_ParticipantId",
                table: "ParticipantExam",
                column: "ParticipantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantExam_AspNetUsers_ParticipantId",
                table: "ParticipantExam");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantExam_ParticipantId",
                table: "ParticipantExam");

            migrationBuilder.AlterColumn<string>(
                name: "ParticipantId",
                table: "ParticipantExam",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
