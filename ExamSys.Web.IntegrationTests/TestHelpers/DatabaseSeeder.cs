using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Infrastructure.Data;

namespace ExamSys.IntegrationTests.Helpers
{
    public static class DatabaseSeeder
    {
        public static void SeedDefaultExam(AppDbContext context, int examId = 1)
        {
            var exam = new Exam
            {
                Id = examId,
                Title = "Final Programming Exam",
                Status = ExamStatus.InProgress,
                StartDate = DateTime.UtcNow.AddMinutes(-10),
                Duration = 60,
                CourseId = 1,
                InstructorId = "instructor-1"
            };

            context.Exams.Add(exam);

            // Add a sample question
            context.Questions.Add(new Question
            {
                Id = 1,
                ExamId = examId,
                Text = "What is C#?",
                Weight = 10,
                CorrectAnswerId = 1
            });

            context.SaveChanges();
        }
    }
}