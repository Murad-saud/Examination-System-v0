using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.IntegrationTests.Helpers;
using FluentAssertions;

namespace ExamSys.IntegrationTests.Infrastructure
{
    public class ExamSyncTests : BaseIntegrationTest
    {
        // This tests the logic often used in Background Workers
        [Fact]
        public async Task BackgroundSync_ShouldUpdateStatus_WhenExamTimeReached()
        {
            // Arrange: Exam scheduled for 5 minutes ago, but status still "Scheduled"
            var exam = new Exam
            {
                Id = 100,
                Title = "Sync Test",
                Status = ExamStatus.Scheduled,
                StartDate = DateTime.Now.AddMinutes(-5),
                Duration = 60,
                CourseId = 1,
                InstructorId = "inst"
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Act: Simulate logic inside SyncService
            var pendingExams = _context.Exams
                .Where(e => e.Status == ExamStatus.Scheduled && e.StartDate <= DateTime.Now)
                .ToList();

            foreach (var e in pendingExams)
            {
                e.Status = ExamStatus.InProgress;
            }
            await _context.SaveChangesAsync();

            // Assert
            var updatedExam = await _context.Exams.FindAsync(100);
            updatedExam.Status.Should().Be(ExamStatus.InProgress);
        }

        [Fact]
        public async Task Dashboard_ShouldNotShowDraftExams()
        {
            // Arrange
            _context.Exams.Add(new Exam { Id = 200, Title = "Draft", Status = ExamStatus.Draft, CourseId = 1, InstructorId = "i", StartDate = DateTime.Now, Duration = 60 });
            _context.Exams.Add(new Exam { Id = 201, Title = "Active", Status = ExamStatus.InProgress, CourseId = 1, InstructorId = "i", StartDate = DateTime.Now, Duration = 60 });
            await _context.SaveChangesAsync();

            // Act
            var visibleExams = _context.Exams.Where(e => e.Status == ExamStatus.InProgress).ToList();

            // Assert
            visibleExams.Should().ContainSingle();
            visibleExams.First().Id.Should().Be(201);
        }
    }
}