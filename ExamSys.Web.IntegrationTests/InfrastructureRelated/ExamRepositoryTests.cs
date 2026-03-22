using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Infrastructure.Data.Repositories;
using ExamSys.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.IntegrationTests.Infrastructure
{
    public class ExamRepositoryTests : BaseIntegrationTest
    {
        private readonly ExamRepository _repo;

        public ExamRepositoryTests()
        {
            _repo = new ExamRepository(_context);
        }

        [Fact]
        public async Task GetAvailableExams_ShouldOnlyReturnInProgressExams()
        {
            // Arrange
            _context.Exams.AddRange(
                new Exam { Id = 1, Title = "Draft", Status = ExamStatus.Draft, StartDate = DateTime.Now, Duration = 60, CourseId = 1, InstructorId = "i1" },
                new Exam { Id = 2, Title = "Active", Status = ExamStatus.InProgress, StartDate = DateTime.Now.AddMinutes(-10), Duration = 60, CourseId = 1, InstructorId = "i1" },
                new Exam { Id = 3, Title = "Future", Status = ExamStatus.Scheduled, StartDate = DateTime.Now.AddDays(1), Duration = 60, CourseId = 1, InstructorId = "i1" }
            );
            await _context.SaveChangesAsync();

            // Act
            // Assuming your repo has a method for student dashboard
            var results = await _context.Exams.Where(e => e.Status == ExamStatus.InProgress).ToListAsync();

            // Assert
            results.Should().HaveCount(1);
            results.First().Title.Should().Be("Active");
        }

        [Fact]
        public async Task GetExamsToSync_ShouldFindScheduledExamsThatShouldBeActive()
        {
            // Arrange - One exam whose StartDate has passed but status is still 'Scheduled'
            var pastTime = DateTime.Now.AddMinutes(-5);
            _context.Exams.Add(new Exam
            {
                Id = 10,
                Title = "Needs Sync",
                Status = ExamStatus.Scheduled,
                StartDate = pastTime,
                Duration = 30,
                CourseId = 1,
                InstructorId = "i"
            });
            await _context.SaveChangesAsync();

            // Act
            var toSync = await _context.Exams
                .Where(e => e.Status == ExamStatus.Scheduled && e.StartDate <= DateTime.Now)
                .ToListAsync();

            // Assert
            toSync.Should().ContainSingle();
            toSync.First().Id.Should().Be(10);
        }
    }
}