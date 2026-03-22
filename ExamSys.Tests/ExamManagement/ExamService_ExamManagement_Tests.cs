using ExamSys.Application.DTOs.Exam;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using ExamSys.Application.Services;
using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExamSys.Tests.ExamManagement
{
    public class ExamService_ExamManagement_Tests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IExamStateManager> _stateManagerMock = new();
        private readonly Mock<IExamTakingService> _examTakingServiceMock = new();
        private readonly Mock<ILogger<ExamService>> _loggerMock = new();

        private readonly ExamService _service;

        public ExamService_ExamManagement_Tests()
        {
            _service = new ExamService(
                _unitOfWorkMock.Object,
                _stateManagerMock.Object,
                _examTakingServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateExamAsync_ShouldCreateExam_AndReturnExamId()
        {
            // Arrange
            var dto = new CreateExamDto
            {
                ExamTitle = "Midterm Exam",
                CourseId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                ExamDuration = 90,
                InstructorId = "instructor-1"
            };

            Exam savedExam = null;

            _unitOfWorkMock
                .Setup(u => u.Exams.AddAsync(It.IsAny<Exam>()))
                .Callback<Exam>(e =>
                {
                    e.Id = 10; // simulate DB-generated Id
                    savedExam = e;
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var examId = await _service.CreateExamAsync(dto);

            // Assert
            Assert.Equal(10, examId);
            Assert.NotNull(savedExam);
            Assert.Equal(dto.ExamTitle, savedExam.Title);
            Assert.Equal(ExamStatus.Draft, savedExam.Status);

            _unitOfWorkMock.Verify(u => u.Exams.AddAsync(It.IsAny<Exam>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddQuestionToExam_ShouldSucceed_WhenExamAllowsAddingQuestions()
        {
            // Arrange
            var exam = new Exam { Id = 1, Status = ExamStatus.Draft };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _stateManagerMock
                .Setup(m => m.CanAddQuestions(ExamStatus.Draft))
                .Returns(true);

            _unitOfWorkMock
                .Setup(u => u.Questions.AddAsync(It.IsAny<Question>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var dto = new CreateQuestionDto
            {
                ExamId = 1,
                QuestionText = "Q1",
                AnswersText = new() { "A", "B" },
                CorrectAnswerIndex = 1,
                QuestionWeight = 5
            };

            // Act
            var result = await _service.AddQuestionToExam(dto);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteExamAsync_ShouldSucceed_WhenStateAllowsDelete()
        {
            // Arrange
            var exam = new Exam { Id = 1, Status = ExamStatus.Draft };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _stateManagerMock
                .Setup(m => m.CanDelete(ExamStatus.Draft))
                .Returns(true);

            _unitOfWorkMock
                .Setup(u => u.Exams.Remove(exam));

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteExamAsync(1);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeExamStateAsync_ShouldUpdateStatus_WhenAllowed()
        {
            // Arrange
            var exam = new Exam
            {
                Id = 1,
                Status = ExamStatus.Pending
            };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _stateManagerMock
                .Setup(m => m.CanSchedule(ExamStatus.Pending))
                .Returns(true);

            _unitOfWorkMock
                .Setup(u => u.Exams.Update(exam));

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.ChangeExamStateAsync(1, ExamStatus.Scheduled);

            // Assert
            result.Succeeded.Should().BeTrue();
            exam.Status.Should().Be(ExamStatus.Scheduled);
        }


        // =========== EDGE CASES ===========

        [Fact]
        public async Task AddQuestionToExam_ShouldFail_WhenStateDisallowsAddingQuestions()
        {
            var exam = new Exam { Id = 1, Status = ExamStatus.Completed };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _stateManagerMock
                .Setup(m => m.CanAddQuestions(ExamStatus.Completed))
                .Returns(false);

            var dto = new CreateQuestionDto { ExamId = 1 };

            var result = await _service.AddQuestionToExam(dto);

            result.Failed.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteExamAsync_ShouldFail_WhenExamNotFound()
        {
            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Exam)null);

            var result = await _service.DeleteExamAsync(99);

            result.Failed.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeExamStateAsync_ShouldFail_WhenTransitionNotAllowed()
        {
            var exam = new Exam { Id = 1, Status = ExamStatus.Completed };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _stateManagerMock
                .Setup(m => m.CanSchedule(ExamStatus.Completed))
                .Returns(false);

            var result = await _service.ChangeExamStateAsync(1, ExamStatus.Scheduled);

            result.Failed.Should().BeTrue();
        }

    }
}
