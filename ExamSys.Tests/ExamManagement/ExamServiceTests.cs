using ExamSys.Application.DTOs.Exam;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using ExamSys.Application.Services;
using ExamSys.Core.Entities;
using ExamSys.Core.Enums;
using ExamSys.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ExamSys.Tests.ExamManagement
{
    public class ExamServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IExamStateManager> _examStateManagerMock;
        private readonly Mock<IExamTakingService> _examTakingServiceMock;
        private readonly ExamService _examService;

        public ExamServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _examStateManagerMock = new Mock<IExamStateManager>();
            _examTakingServiceMock = new Mock<IExamTakingService>();

            var logger = new NullLogger<ExamService>();

            _examService = new ExamService(
                _unitOfWorkMock.Object,
                _examStateManagerMock.Object,
                _examTakingServiceMock.Object,
                logger
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
            var examId = await _examService.CreateExamAsync(dto);

            // Assert
            Assert.Equal(10, examId);
            Assert.NotNull(savedExam);
            Assert.Equal(dto.ExamTitle, savedExam.Title);
            Assert.Equal(ExamStatus.Draft, savedExam.Status);

            _unitOfWorkMock.Verify(u => u.Exams.AddAsync(It.IsAny<Exam>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task ValidateExamOwnership_ShouldReturnSuccess_WhenOwnerMatches()
        {
            // Arrange
            var exam = new Exam
            {
                Id = 5,
                InstructorId = "instructor-1"
            };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(5))
                .ReturnsAsync(exam);

            // Act
            var result = await _examService.ValidateExamOwnership(5, "instructor-1");

            // Assert
            Assert.True(result.Succeeded);
            Assert.False(result.Failed);
        }


        [Fact]
        public async Task ValidateExamOwnership_ShouldReturnFailure_WhenOwnerDoesNotMatch()
        {
            // Arrange
            var exam = new Exam
            {
                Id = 5,
                InstructorId = "instructor-1"
            };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(5))
                .ReturnsAsync(exam);

            // Act
            var result = await _examService.ValidateExamOwnership(5, "instructor-2");

            // Assert
            Assert.True(result.Failed);
            Assert.False(result.Succeeded);
        }


        ////////////////////////////////////////////////////////////////////////


        [Fact]
        public async Task UpdateExamAsync_ShouldUpdateExam_WhenExamExists()
        {
            var exam = new Exam
            {
                Id = 1,
                Title = "Old",
                Duration = 30,
                CourseId = 1
            };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var dto = new UpdateExamDto
            {
                ExamId = 1,
                ExamTitle = "New Title",
                Duration = 60,
                CourseId = 2,
                StartDate = DateTime.UtcNow
            };

            var result = await _examService.UpdateExamAsync(dto);

            result.Succeeded.Should().BeTrue();
            exam.Title.Should().Be("New Title");
            exam.Duration.Should().Be(60);
        }

        [Fact]
        public async Task UpdateExamAsync_ShouldFail_WhenExamNotFound()
        {
            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Exam)null);

            var dto = new UpdateExamDto { ExamId = 99 };

            var result = await _examService.UpdateExamAsync(dto);

            result.Failed.Should().BeTrue();
        }

        [Fact]
        public async Task AddQuestionToExam_ShouldFail_WhenQuestionTextAlreadyExists()
        {
            var exam = new Exam { Id = 1, Status = ExamStatus.Draft };

            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(1))
                .ReturnsAsync(exam);

            _examStateManagerMock
                .Setup(m => m.CanAddQuestions(ExamStatus.Draft))
                .Returns(true);

            _unitOfWorkMock
                .Setup(u => u.Questions.GetByExamAndTextAsync(1, "Q1"))
                .ReturnsAsync(new Question());

            var dto = new CreateQuestionDto
            {
                ExamId = 1,
                QuestionText = "Q1"
            };

            var result = await _examService.AddQuestionToExam(dto);

            result.Failed.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeExamStateAsync_ShouldFail_WhenExamNotFound()
        {
            _unitOfWorkMock
                .Setup(u => u.Exams.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Exam)null);

            var result = await _examService.ChangeExamStateAsync(1, ExamStatus.Scheduled);

            result.Failed.Should().BeTrue();
        }


    }
}
