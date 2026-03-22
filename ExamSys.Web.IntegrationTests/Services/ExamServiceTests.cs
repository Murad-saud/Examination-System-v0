using ExamSys.Application.DTOs.Exam;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using ExamSys.Application.Services;
using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Infrastructure.Data;
using ExamSys.Infrastructure.Data.Repositories;
using ExamSys.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExamSys.IntegrationTests.Services
{
    public class ExamServiceTests : BaseIntegrationTest
    {
        private readonly ExamService _examService;
        private readonly Mock<IExamStateManager> _mockStateManager;
        private readonly Mock<IExamTakingService> _mockTakingService;
        private readonly string _testInstructorId = "inst-999";

        public ExamServiceTests()
        {
            _mockStateManager = new Mock<IExamStateManager>();
            _mockTakingService = new Mock<IExamTakingService>();
            var logger = Mock.Of<ILogger<ExamService>>();

            // Reusing the real UnitOfWork and Repositories for true integration
            var unitOfWork = new UnitOfWork(_context,
                new ExamRepository(_context),
                new CourseRepository(_context),
                new QuestionRepository(_context),
                new ParticipantExamRepository(_context),
                new ParticipantAnswerRepository(_context));

            _examService = new ExamService(unitOfWork, _mockStateManager.Object, _mockTakingService.Object, logger);
        }


        [Fact]
        public async Task CreateExamAsync_ShouldReturnNewId_AndSetInitialStatusToDraft()
        {
            // Arrange
            var dto = new CreateExamDto
            {
                ExamTitle = "Integration Physics",
                StartDate = DateTime.Now.AddDays(1),
                ExamDuration = 90,
                CourseId = 1,
                InstructorId = _testInstructorId
            };

            // Act
            var examId = await _examService.CreateExamAsync(dto);

            // Assert
            examId.Should().BeGreaterThan(0);
            var saved = await _context.Exams.FindAsync(examId);
            saved.Title.Should().Be("Integration Physics");
            saved.Status.Should().Be(ExamStatus.Draft);
        }

        [Fact]
        public async Task UpdateExamAsync_ShouldUpdateAllProperties_OnSuccess()
        {
            // Arrange
            var exam = new Exam { Title = "Old Title", Status = ExamStatus.Draft, InstructorId = _testInstructorId, StartDate = DateTime.Now, Duration = 30, CourseId = 1 };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateExamDto
            {
                ExamId = exam.Id,
                ExamTitle = "Updated Title",
                StartDate = exam.StartDate.AddHours(1),
                Duration = 60,
                CourseId = 2
            };

            // Act
            var result = await _examService.UpdateExamAsync(updateDto);

            // Assert
            result.Succeeded.Should().BeTrue();
            var updated = await _context.Exams.FindAsync(exam.Id);
            updated.Title.Should().Be("Updated Title");
            updated.Duration.Should().Be(60);
        }

        [Fact] // Critical: Security guard
        public async Task ValidateExamOwnership_ShouldFail_WhenInstructorIdDoesNotMatch()
        {
            // Arrange
            var examId = 2001;
            _context.Exams.Add(new Exam { Id = examId, InstructorId = "Owner-ID", Title = "Private Exam", CourseId = 1, StartDate = DateTime.Now, Duration = 60 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.ValidateExamOwnership(examId, "Hacker-ID");

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be("Access Denied.");
        }

        [Fact] // Critical: Tests the math and data shaping logic
        public async Task GetAllParticipantExamsResults_ShouldHandleZeroPointsExam_Gracefully()
        {
            // Arrange
            var examId = 3001;
            _context.Exams.Add(new Exam { Id = examId, Title = "Empty Score Exam", InstructorId = _testInstructorId, CourseId = 1, StartDate = DateTime.Now, Duration = 60 });
            // No questions added = totalPoints is 0
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.GetAllParticipantExamsResults(examId);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Data.TotalPoints.Should().Be(0);
        }

        [Fact]
        public async Task AddQuestionToExam_ShouldFail_WhenQuestionTextAlreadyExists()
        {
            // Arrange
            var examId = 5002;
            var existingText = "Double Question";
            _context.Exams.Add(new Exam { Id = examId, Status = ExamStatus.Preparing, InstructorId = _testInstructorId, Title = "Dup Test", CourseId = 1, StartDate = DateTime.Now, Duration = 60 });

            // We must add the question so GetByExamAndTextAsync finds it
            _context.Questions.Add(new Question { ExamId = examId, Text = existingText, Weight = 5 });
            await _context.SaveChangesAsync();

            _mockStateManager.Setup(m => m.CanAddQuestions(ExamStatus.Preparing)).Returns(true);

            var qDto = new CreateQuestionDto { ExamId = examId, QuestionText = existingText, AnswersText = new List<string> { "a", "b" }, CorrectAnswerIndex = 1 };

            // Act
            var result = await _examService.AddQuestionToExam(qDto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("already exists");
        }

        [Fact]
        public async Task DeleteExamAsync_ShouldRemoveExam_WhenValid()
        {
            // Arrange
            var exam = new Exam
            {
                Title = "Delete Me",
                Status = ExamStatus.Draft,
                InstructorId = _testInstructorId,
                CourseId = 1,
                StartDate = DateTime.Now,
                Duration = 30
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Act
            var result = await _examService.DeleteExamAsync(exam.Id);

            // Assert
            result.Succeeded.Should().BeTrue();
            (await _context.Exams.FindAsync(exam.Id)).Should().BeNull();
        }

        //[Fact]
        //public async Task DeleteExamAsync_ShouldRemoveExamAndQuestions()
        //{
        //    // Arrange
        //    var exam = new Exam
        //    {
        //        Title = "Delete Me",
        //        Status = ExamStatus.Draft,
        //        InstructorId = _testInstructorId,
        //        CourseId = 1,
        //        StartDate = DateTime.Now,
        //        Duration = 60
        //    };
        //    _context.Exams.Add(exam);
        //    await _context.SaveChangesAsync();

        //    _context.Questions.Add(new Question
        //    {
        //        ExamId = exam.Id,
        //        Text = "Temp Question",
        //        Weight = 5
        //    });
        //    await _context.SaveChangesAsync();

        //    // Act
        //    var result = await _examService.DeleteExamAsync(exam.Id);

        //    // Assert
        //    result.Succeeded.Should().BeTrue();
        //    _context.Exams.Any(e => e.Id == exam.Id).Should().BeFalse();
        //    _context.Questions.Any(q => q.ExamId == exam.Id).Should().BeFalse();
        //}

        //[Fact]
        //public async Task ChangeExamStateAsync_ShouldPersistNewStatus()
        //{
        //    // Arrange
        //    var exam = new Exam
        //    {
        //        Title = "State Change",
        //        Status = ExamStatus.Pending,
        //        InstructorId = _testInstructorId,
        //        CourseId = 1,
        //        StartDate = DateTime.Now,
        //        Duration = 60
        //    };
        //    _context.Exams.Add(exam);
        //    await _context.SaveChangesAsync();

        //    _mockStateManager
        //        .Setup(m => m.CanSchedule(ExamStatus.Pending))
        //        .Returns(true);

        //    // Act
        //    var result = await _examService.ChangeExamStateAsync(
        //        exam.Id,
        //        ExamStatus.Scheduled
        //    );

        //    // Assert
        //    result.Succeeded.Should().BeTrue();
        //    var updated = await _context.Exams.FindAsync(exam.Id);
        //    updated.Status.Should().Be(ExamStatus.Scheduled);
        //}







    }
}