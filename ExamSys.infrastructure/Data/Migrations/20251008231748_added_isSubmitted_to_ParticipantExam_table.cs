using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examination_System.Migrations
{
    /// <inheritdoc />
    public partial class added_isSubmitted_to_ParticipantExam_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelectedChoiceId",
                table: "ParticipantAnswers",
                newName: "SelectedAnswerId");

            migrationBuilder.AddColumn<bool>(
                name: "isSubmitted",
                table: "ParticipantExam",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isSubmitted",
                table: "ParticipantExam");

            migrationBuilder.RenameColumn(
                name: "SelectedAnswerId",
                table: "ParticipantAnswers",
                newName: "SelectedChoiceId");
        }
    }
}
