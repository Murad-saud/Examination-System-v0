using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examination_System.Migrations
{
    /// <inheritdoc />
    public partial class Added_Relation_ParticipantExam__Exam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParticipantExam_ExamId",
                table: "ParticipantExam",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantExam_Exams_ExamId",
                table: "ParticipantExam",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantExam_Exams_ExamId",
                table: "ParticipantExam");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantExam_ExamId",
                table: "ParticipantExam");
        }
    }
}
