using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Interfaces;
using ExamSys.Application.Services;
using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExamSys.Tests.ExamTaking
{
    public class NavigationAndAutoSaveTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IExamCacheService> _cache = new();
        private readonly Mock<IExamStateManager> _stateManager = new();
        private readonly Mock<ILogger<ExamTakingService>> _logger = new();

        private readonly ExamTakingService _service;

        public NavigationAndAutoSaveTests()
        {
            _service = new ExamTakingService(
                _uow.Object,
                _cache.Object,
                _stateManager.Object,
                _logger.Object
            );
        }

        // =========================
        // NAVIGATION TESTS
        // =========================

        [Fact]
        public async Task GetExamPageAsync_WhenValid_ReturnsPagedQuestions()
        {
            // Arrange
            var participantExam = new ParticipantExam
            {
                Id = 1,
                ExamId = 10,
                ParticipantId = "user1",
                isSubmitted = false
            };

            var exam = new Exam
            {
                Id = 10,
                StartDate = DateTime.Now.AddMinutes(-10),
                Duration = 60
            };

            var questions = new List<QuestionWithAnswersResponse>
            {
                new()
                {
                    QuestionId = 1,
                    QuestionText = "Q1",
                    QuestionWeight = 1,
                    Answers = new List<Answer>
                    {
                        new() { Id = 1, Text = "A1", QuestionId = 1 }
                    }
                }
            };

            _uow.Setup(x => x.ParticipantExams.GetByIdAsync(1))
                .ReturnsAsync(participantExam);

            _uow.Setup(x => x.Exams.GetByIdAsync(10))
                .ReturnsAsync(exam);

            _cache.Setup(x => x.GetExamQuestionsAsync(10))
                .ReturnsAsync(new CachedExamDetails
                {
                    AllQuestions = questions,
                    TotalPages = 1,
                    EndTime = DateTime.Now.AddMinutes(50)
                });

            _uow.Setup(x => x.ParticipantAnswers
                    .GetPageSavedAnswersAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new Dictionary<int, int>());

            // Act
            var result = await _service.GetExamPageAsync(1, 1, "user1");

            // Assert
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.PageQuestions);
            Assert.Equal(1, result.Data.CurrentPage);
        }

        // =========================
        // AUTO-SAVE TESTS
        // =========================

        [Fact]
        public async Task SaveParticipantAnswersAsync_WhenNewAnswers_SavesSuccessfully()
        {
            // Arrange
            var participantExam = new ParticipantExam
            {
                Id = 1,
                ParticipantId = "user1",
                isSubmitted = false
            };

            var answers = new List<SaveAnswersRequest>
            {
                new() { QuestionId = 1, SelectedAnswerId = 2 }
            };

            _uow.Setup(x => x.ParticipantExams.GetByIdAsync(1))
                .ReturnsAsync(participantExam);

            _uow.Setup(x => x.ParticipantAnswers
                    .GetParticipantSavedAnswersAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<ParticipantAnswer>());

            _uow.Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            _uow.Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SaveParticipantAnswersAsync(answers, "user1", 1);

            // Assert
            Assert.True(result.Succeeded);
        }

        // =========================
        // EDGE CASES
        // =========================

        [Fact]
        public async Task GetExamPageAsync_WhenPageNumberInvalid_ReturnsFailure()
        {
            // Arrange
            var participantExam = new ParticipantExam
            {
                Id = 1,
                ExamId = 10,
                ParticipantId = "user1",
                isSubmitted = false
            };

            _uow.Setup(x => x.ParticipantExams.GetByIdAsync(1))
                .ReturnsAsync(participantExam);

            _uow.Setup(x => x.Exams.GetByIdAsync(10))
                .ReturnsAsync(new Exam
                {
                    StartDate = DateTime.Now.AddMinutes(-5),
                    Duration = 30
                });

            _cache.Setup(x => x.GetExamQuestionsAsync(10))
                .ReturnsAsync(new CachedExamDetails
                {
                    AllQuestions = new List<QuestionWithAnswersResponse>(),
                    TotalPages = 1
                });

            // Act
            var result = await _service.GetExamPageAsync(1, 5, "user1");

            // Assert
            Assert.True(result.Failed);
        }

        [Fact]
        public async Task SaveParticipantAnswersAsync_WhenNoAnswersProvided_ReturnsFailure()
        {
            // Arrange
            var participantExam = new ParticipantExam
            {
                Id = 1,
                ParticipantId = "user1",
                isSubmitted = false
            };

            _uow.Setup(x => x.ParticipantExams.GetByIdAsync(1))
                .ReturnsAsync(participantExam);

            // Act
            var result = await _service.SaveParticipantAnswersAsync(
                new List<SaveAnswersRequest>(),
                "user1",
                1);

            // Assert
            Assert.True(result.Failed);
        }
    }
}
