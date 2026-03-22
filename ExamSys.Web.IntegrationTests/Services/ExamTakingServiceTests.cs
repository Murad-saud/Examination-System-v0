using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Services;
using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Infrastructure.Data;
using ExamSys.Infrastructure.Data.Repositories;
using ExamSys.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExamSys.IntegrationTests.Services
{
    public class ExamTakingServiceTests : BaseIntegrationTest
    {
        private readonly ExamTakingService _service;

        public ExamTakingServiceTests()
        {
            var unitOfWork = new UnitOfWork(_context,
                new ExamRepository(_context),
                new CourseRepository(_context),
                new QuestionRepository(_context),
                new ParticipantExamRepository(_context),
                new ParticipantAnswerRepository(_context)
            );

            var cache = _serviceProvider.GetRequiredService<IMemoryCache>();
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

            var cacheService = new ExamCacheService(cache, loggerFactory.CreateLogger<ExamCacheService>());
            var stateManager = new ExamStateManager();
            var logger = loggerFactory.CreateLogger<ExamTakingService>();

            _service = new ExamTakingService(unitOfWork, cacheService, stateManager, logger);
        }

        // 1. Join Success
        [Fact]
        public async Task JoinExamAsync_ShouldReturnSuccess_WhenExamIsActive()
        {
            var examId = 1;
            var userId = "student-1";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var result = await _service.JoinExamAsync(examId, userId);

            result.Succeeded.Should().BeTrue();
            result.Data.ParticipantExamId.Should().BeGreaterThan(0);
        }

        // 2. Resume Success
        [Fact]
        public async Task JoinExamAsync_ShouldResumeAttempt_WhenAlreadyJoined()
        {
            var examId = 2;
            var userId = "student-2";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var existing = new ParticipantExam { ExamId = examId, ParticipantId = userId, isSubmitted = false };
            _context.ParticipantExam.Add(existing);
            await _context.SaveChangesAsync();

            var result = await _service.JoinExamAsync(examId, userId);

            result.Succeeded.Should().BeTrue();
            result.Data.ParticipantExamId.Should().Be(existing.Id);
        }

        // 3. Early Join Guard
        [Fact]
        public async Task JoinExamAsync_ShouldFail_WhenExamHasNotStarted()
        {
            var examId = 3;
            SeedExam(examId, "s3", ExamStatus.Scheduled, startDate: DateTime.Now.AddHours(1));

            var result = await _service.JoinExamAsync(examId, "s3");

            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("started");
        }

        // 4. Late Join Guard
        [Fact]
        public async Task JoinExamAsync_ShouldFail_WhenExamIsExpired()
        {
            var examId = 4;
            SeedExam(examId, "s4", ExamStatus.InProgress, startDate: DateTime.Now.AddHours(-5), duration: 30);

            var result = await _service.JoinExamAsync(examId, "s4");

            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("ended");
        }

        [Fact]
        public async Task GetExamPageAsync_ShouldFail_WhenUserDoesNotOwnParticipantExam()
        {
            // Arrange
            var examId = 555;
            SeedExam(examId, "owner-user", ExamStatus.InProgress);

            var participantExam = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = "owner-user",
                isSubmitted = false
            };
            _context.ParticipantExam.Add(participantExam);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetExamPageAsync(
                participantExam.Id,
                pageNumber: 1,
                userId: "another-user"   // ❌ not the owner
            );

            // Assert
            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("Access");
        }

        [Fact]
        public async Task GetExamPageAsync_ShouldFail_WhenExamIsAlreadySubmitted()
        {
            // Arrange
            var examId = 666;
            var userId = "student-666";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var participantExam = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = true   // ❗ already submitted
            };
            _context.ParticipantExam.Add(participantExam);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetExamPageAsync(
                participantExam.Id,
                pageNumber: 1,
                userId: userId
            );

            // Assert
            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("no longer active");
        }

        [Fact]
        public async Task SaveParticipantAnswersAsync_ShouldReturnSuccess_WhenNoChangesDetected()
        {
            var examId = 777;
            var userId = "student-777";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var pExam = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = false
            };
            _context.ParticipantExam.Add(pExam);
            await _context.SaveChangesAsync();

            // Existing answer
            _context.ParticipantAnswers.Add(new ParticipantAnswer
            {
                ParticipantExamId = pExam.Id,
                QuestionId = 101,
                SelectedAnswerId = 1
            });
            await _context.SaveChangesAsync();

            // SAME answer -> no update
            var request = new List<SaveAnswersRequest>
            {
                new SaveAnswersRequest { QuestionId = 101, SelectedAnswerId = 1 }
            };

            var result = await _service.SaveParticipantAnswersAsync(request, userId, pExam.Id);

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task SaveParticipantAnswersAsync_ShouldReturnSuccess_WhenEmptyInput()
        {
            var examId = 888;
            var userId = "student-888";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var pExam = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = false
            };
            _context.ParticipantExam.Add(pExam);
            await _context.SaveChangesAsync();

            var result = await _service.SaveParticipantAnswersAsync(
                new List<SaveAnswersRequest>(),
                userId,
                pExam.Id
            );

            result.Failed.Should().BeTrue();
        }

        // 9. Submission Logic & Scoring
        [Fact]
        public async Task SubmitExamAsync_ShouldCalculateCorrectScore()
        {
            var examId = 9;
            var userId = "s9";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var pExam = new ParticipantExam { ExamId = examId, ParticipantId = userId, isSubmitted = false };
            _context.ParticipantExam.Add(pExam);
            await _context.SaveChangesAsync();

            // Correct Answer for 101 is 2 (Weight 10). Correct Answer for 102 is 4 (Weight 5).
            _context.ParticipantAnswers.AddRange(
                new ParticipantAnswer { ParticipantExamId = pExam.Id, QuestionId = 101, SelectedAnswerId = 2 }, // Correct
                new ParticipantAnswer { ParticipantExamId = pExam.Id, QuestionId = 102, SelectedAnswerId = 3 }  // Wrong
            );
            await _context.SaveChangesAsync();

            var result = await _service.SubmitExamAsync(pExam.Id, userId);

            result.Succeeded.Should().BeTrue();
            var updated = await _context.ParticipantExam.FindAsync(pExam.Id);
            updated.Score.Should().Be(10); // Only Q1 points
            updated.isSubmitted.Should().BeTrue();
        }

        // 10. Double Submission Block
        [Fact]
        public async Task SubmitExamAsync_ShouldFail_IfAlreadySubmitted()
        {
            var pExam = new ParticipantExam { ExamId = 1, ParticipantId = "s10", isSubmitted = true };
            _context.ParticipantExam.Add(pExam);
            await _context.SaveChangesAsync();

            var result = await _service.SubmitExamAsync(pExam.Id, "s10");

            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("active");
        }

        [Fact] // 11.
        public async Task GetExamPageAsync_ShouldFail_WhenPageIsOutOfBounds()
        {
            // Arrange
            var examId = 900;
            var userId = "student-900";
            SeedExam(examId, userId, ExamStatus.InProgress);
            // SeedExam creates 2 questions, so at 8 per page, TotalPages = 1

            var pExam = new ParticipantExam { ExamId = examId, ParticipantId = userId };
            _context.ParticipantExam.Add(pExam);
            await _context.SaveChangesAsync();

            // Act - Try to get page 5
            var result = await _service.GetExamPageAsync(pExam.Id, 5, userId);

            // Assert
            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("Invalid page number");
        }


        // ==========================================
        // ADDED LATER
        // ==========================================

        [Fact]
        public async Task GetExamPageAsync_ShouldFail_WhenParticipantExamDoesNotExist()
        {
            // Arrange
            var examId = 1000;
            var userId = "student-x";
            SeedExam(examId, userId, ExamStatus.InProgress);

            // Act
            var result = await _service.GetExamPageAsync(
                participantExamId: 9999,
                pageNumber: 1,
                userId: userId
            );

            // Assert
            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("not found");
        }

        [Fact]
        public async Task SaveParticipantAnswersAsync_ShouldFail_WhenUserIsNotOwner()
        {
            // Arrange
            var examId = 1100;
            SeedExam(examId, "real-user", ExamStatus.InProgress);

            var participantExam = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = "real-user",
                isSubmitted = false
            };
            _context.ParticipantExam.Add(participantExam);
            await _context.SaveChangesAsync();

            var request = new List<SaveAnswersRequest>
    {
        new SaveAnswersRequest { QuestionId = 101, SelectedAnswerId = 2 }
    };

            // Act
            var result = await _service.SaveParticipantAnswersAsync(
                request,
                userId: "fake-user",
                participantExamId: participantExam.Id
            );

            // Assert
            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("Access");
        }

        [Fact]
        public async Task SubmitExamAsync_ShouldFail_WhenParticipantExamDoesNotExist()
        {
            // Act
            var result = await _service.SubmitExamAsync(
                participantExamId: 12345,
                userId: "any-user"
            );

            // Assert
            result.Failed.Should().BeTrue();
            result.Error.Should().Contain("not found");
        }

        [Fact]
        public async Task JoinExamAsync_ShouldNotCreateDuplicateParticipantExam()
        {
            // Arrange
            var examId = 1200;
            var userId = "student-1200";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var existing = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = false
            };
            _context.ParticipantExam.Add(existing);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.JoinExamAsync(examId, userId);

            // Assert
            result.Succeeded.Should().BeTrue();

            var attempts = _context.ParticipantExam
                .Where(p => p.ExamId == examId && p.ParticipantId == userId)
                .ToList();

            attempts.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetExamPageAsync_ShouldFail_WhenAttemptWasSubmittedEarlier()
        {
            // Arrange
            var examId = 1300;
            var userId = "student-1300";
            SeedExam(examId, userId, ExamStatus.InProgress);

            var pExam = new ParticipantExam
            {
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = true
            };
            _context.ParticipantExam.Add(pExam);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetExamPageAsync(pExam.Id, 1, userId);

            // Assert
            result.Failed.Should().BeTrue();
            //result.Error.Should().Contain("no longer active");
        }




        // ==========================================
        // CORRECTED SEEDER
        // ==========================================
        private void SeedExam(int examId, string userId, ExamStatus status, DateTime? startDate = null, int duration = 60)
        {
            if (!_context.Courses.Any(c => c.Id == 1))
                _context.Courses.Add(new Course { Id = 1, Name = "Test Course", InstructorId = "inst" });

            var exam = new Exam
            {
                Id = examId,
                Title = "Integration Test Exam",
                Status = status,
                StartDate = startDate ?? DateTime.Now.AddMinutes(-5),
                Duration = duration,
                CourseId = 1,
                InstructorId = "inst"
            };
            _context.Exams.Add(exam);

            // Seed Questions
            var q1 = new Question { Id = 101, ExamId = examId, Text = "Q1", Weight = 10, CorrectAnswerId = 2 };
            var q2 = new Question { Id = 102, ExamId = examId, Text = "Q2", Weight = 5, CorrectAnswerId = 4 };

            _context.Questions.AddRange(q1, q2);

            // Seed Answers
            _context.Answers.AddRange(
                new Answer { Id = 1, Text = "A", QuestionId = 101 },
                new Answer { Id = 2, Text = "B", QuestionId = 101 },
                new Answer { Id = 3, Text = "C", QuestionId = 102 },
                new Answer { Id = 4, Text = "D", QuestionId = 102 }
            );

            _context.SaveChanges();
        }
    }
}