using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Interfaces;
using ExamSys.Application.Services;
using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces;
using ExamSys.Core.ResponseModels.Exam;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExamSys.Tests.ExamTaking
{
    public class ExamTakingServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IExamCacheService> _cacheMock = new();
        private readonly Mock<IExamStateManager> _stateManagerMock = new();
        private readonly Mock<ILogger<ExamTakingService>> _loggerMock = new();

        private readonly ExamTakingService _service;

        public ExamTakingServiceTests()
        {
            _service = new ExamTakingService(
                _unitOfWorkMock.Object,
                _cacheMock.Object,
                _stateManagerMock.Object,
                _loggerMock.Object
            );
        }




        [Fact]
        public async Task GetExamPageAsync_WhenValid_ReturnsPage()
        {
            // Arrange
            var participantExam = new ParticipantExam
            {
                Id = 5,
                ExamId = 1,
                ParticipantId = "user1",
                isSubmitted = false
            };

            var exam = new Exam
            {
                Id = 1,
                StartDate = DateTime.Now.AddMinutes(-10),
                Duration = 60
            };

            _unitOfWorkMock.Setup(u => u.ParticipantExams.GetByIdAsync(5))
                .ReturnsAsync(participantExam);

            _unitOfWorkMock.Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _cacheMock.Setup(c => c.GetExamQuestionsAsync(1))
                .ReturnsAsync(new CachedExamDetails
                {
                    TotalPages = 1,
                    AllQuestions = new List<QuestionWithAnswersResponse>
                    {
                new QuestionWithAnswersResponse
                {
                    QuestionId = 1,
                    QuestionText = "Q1",
                    QuestionWeight = 5,
                    Answers = new List<Answer>
                    {
                        new Answer { Id = 1, Text = "A1", QuestionId = 1 }
                    }
                }
                    }
                });

            _unitOfWorkMock.Setup(u => u.ParticipantAnswers
                .GetPageSavedAnswersAsync(5, It.IsAny<List<int>>()))
                .ReturnsAsync(new Dictionary<int, int>());

            // Act
            var result = await _service.GetExamPageAsync(5, 1, "user1");

            // Assert
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.PageQuestions);
        }

        [Fact]
        public async Task SubmitExamAsync_WhenValid_AutoCorrectsAndSubmits()
        {
            // Arrange
            var participantExam = new ParticipantExam
            {
                Id = 3,
                ExamId = 1,
                ParticipantId = "user1",
                isSubmitted = false
            };

            _unitOfWorkMock.Setup(u => u.ParticipantExams.GetByIdAsync(3))
                .ReturnsAsync(participantExam);

            _unitOfWorkMock.Setup(u => u.ParticipantAnswers
                .AutoCorrectExamAsync(3))
                .ReturnsAsync(new List<CorrectionResult>
                {
            new CorrectionResult { IsCorrect = true, Weight = 5 },
            new CorrectionResult { IsCorrect = false, Weight = 5 }
                });

            _unitOfWorkMock.Setup(u => u.ParticipantExams.Update(participantExam));
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.SubmitExamAsync(3, "user1");

            // Assert
            Assert.True(result.Succeeded);
            Assert.True(participantExam.isSubmitted);
            Assert.Equal(5, participantExam.Score);
            Assert.Equal(1, participantExam.CorrectAnswersCount);
        }

        [Fact]
        public async Task JoinExamAsync_WhenValid_ReturnsSuccess()
        {
            // Arrange
            var examId = 1;
            var userId = "user1";

            var exam = new Exam
            {
                Id = examId,
                StartDate = DateTime.Now.AddMinutes(-10), // already started
                Duration = 60,
                Questions = new List<Question>
        {
            new Question(), new Question(), new Question()
        }
            };

            var participantExam = new ParticipantExam
            {
                Id = 10,
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = false
            };

            _unitOfWorkMock.Setup(u => u.Exams.GetExamByIdAsync(examId))
                .ReturnsAsync(new ExamDetailsResponse
                {
                    ExamId = examId,
                    StartDate = exam.StartDate,
                    Duration = exam.Duration,
                    QuestionsCount = exam.Questions.Count
                });

            _unitOfWorkMock.Setup(u => u.ParticipantExams
                .GetOrCreateParticipantExamAsync(examId, userId))
                .ReturnsAsync(participantExam);

            // Act
            var result = await _service.JoinExamAsync(examId, userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(examId, result.Data.ExamId);
        }

        [Fact]
        public async Task GetExamPageAsync_WhenValid_ReturnsExamPage()
        {
            // Arrange
            var participantExamId = 1;
            var userId = "user1";
            var examId = 5;

            var participantExam = new ParticipantExam
            {
                Id = participantExamId,
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = false
            };

            var exam = new Exam
            {
                Id = examId,
                StartDate = DateTime.Now.AddMinutes(-5),
                Duration = 60
            };

            _unitOfWorkMock.Setup(u => u.ParticipantExams.GetByIdAsync(participantExamId))
                .ReturnsAsync(participantExam);

            _unitOfWorkMock.Setup(u => u.Exams.GetByIdAsync(examId))
                .ReturnsAsync(exam);

            _cacheMock.Setup(c => c.GetExamQuestionsAsync(examId))
                .ReturnsAsync((CachedExamDetails)null);

            _unitOfWorkMock.Setup(u => u.Questions.GetAllExamQuestionsAsync(examId))
                .ReturnsAsync(new List<QuestionWithAnswersResponse>
                {
            new QuestionWithAnswersResponse
            {
                QuestionId = 1,
                QuestionText = "Q1",
                QuestionWeight = 5,
                Answers = new List<Answer>
                {
                    new Answer { Id = 1, Text = "A1", QuestionId = 1 }
                }
            }
                });

            _unitOfWorkMock.Setup(u => u.ParticipantAnswers
                .GetPageSavedAnswersAsync(participantExamId, It.IsAny<List<int>>()))
                .ReturnsAsync(new Dictionary<int, int>());

            // Act
            var result = await _service.GetExamPageAsync(participantExamId, 1, userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.PageQuestions);
        }

        [Fact]
        public async Task SaveParticipantAnswersAsync_WhenValid_SavesSuccessfully()
        {
            // Arrange
            var participantExamId = 1;
            var userId = "user1";

            var participantExam = new ParticipantExam
            {
                Id = participantExamId,
                ParticipantId = userId,
                isSubmitted = false
            };

            _unitOfWorkMock.Setup(u => u.ParticipantExams.GetByIdAsync(participantExamId))
                .ReturnsAsync(participantExam);

            _unitOfWorkMock.Setup(u => u.ParticipantAnswers
                .GetParticipantSavedAnswersAsync(participantExamId, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<ParticipantAnswer>());

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var requests = new List<SaveAnswersRequest>
    {
        new SaveAnswersRequest { QuestionId = 1, SelectedAnswerId = 2 }
    };

            // Act
            var result = await _service.SaveParticipantAnswersAsync(requests, userId, participantExamId);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SubmitExamAsync_WhenValid_SubmitsAndAutoCorrects()
        {
            // Arrange
            var participantExamId = 7;

            var participantExam = new ParticipantExam
            {
                Id = participantExamId,
                isSubmitted = false
            };

            _unitOfWorkMock
                .Setup(u => u.ParticipantExams.GetByIdAsync(participantExamId))
                .ReturnsAsync(participantExam);

            _unitOfWorkMock
                .Setup(u => u.ParticipantAnswers.AutoCorrectExamAsync(participantExamId))
                .ReturnsAsync(new List<CorrectionResult>());

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.SubmitExamAsync(participantExamId, null);

            // Assert
            Assert.True(result.Succeeded);
        }



        // =========== EDGE CASES ===========

        [Fact]
        public async Task JoinExamAsync_WhenExamNotFound_ReturnsFailure()
        {
            _unitOfWorkMock
                .Setup(u => u.ParticipantExams.GetOrCreateParticipantExamAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((ParticipantExam)null);

            var result = await _service.JoinExamAsync(999, "user");

            Assert.True(result.Failed);
        }

        [Fact]
        public async Task SubmitExamAsync_WhenAlreadySubmitted_ReturnsFailure()
        {
            var participantExam = new ParticipantExam { isSubmitted = true };

            _unitOfWorkMock
                .Setup(u => u.ParticipantExams.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(participantExam);

            var result = await _service.SubmitExamAsync(1, "user");

            Assert.True(result.Failed);
        }



    }

}
