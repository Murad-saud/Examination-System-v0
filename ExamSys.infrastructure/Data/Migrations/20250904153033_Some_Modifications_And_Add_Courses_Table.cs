using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examination_System.Migrations
{
    /// <inheritdoc />
    public partial class Some_Modifications_And_Add_Courses_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Choices");

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.DropTable(
                name: "StudentExams");

            //migrationBuilder.DropColumn(
            //    name: "DurationMinutes",
            //    table: "Exams");

            migrationBuilder.DropColumn(
                name: "isApproved",
                table: "Exams");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Exams",
                newName: "InstructorId");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Exams",
                newName: "StartDate");

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Convert time values to minutes
            migrationBuilder.Sql(@"
        UPDATE Exams 
        SET Duration = DATEDIFF(MINUTE, '00:00:00', DurationMinutes)
    ");

            // Now drop the old DurationMinutes column
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Exams");

            // Alter the new Duration column to be non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantExam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantExam", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantExamId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedChoiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantAnswers_ParticipantExam_ParticipantExamId",
                        column: x => x.ParticipantExamId,
                        principalTable: "ParticipantExam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantAnswers_ParticipantExamId",
                table: "ParticipantAnswers",
                column: "ParticipantExamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "ParticipantAnswers");

            migrationBuilder.DropTable(
                name: "ParticipantExam");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Exams");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Exams",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "InstructorId",
                table: "Exams",
                newName: "TeacherId");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DurationMinutes",
                table: "Exams",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "isApproved",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Choices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Choices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choices_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedChoiceId = table.Column<int>(type: "int", nullable: false),
                    StudentExamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_StudentExams_StudentExamId",
                        column: x => x.StudentExamId,
                        principalTable: "StudentExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Choices_QuestionId",
                table: "Choices",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_StudentExamId",
                table: "StudentAnswers",
                column: "StudentExamId");
        }
    }
}
