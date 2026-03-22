using Examination_System.Controllers;
using ExamSys.Application.Common;
using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace ExamSys.IntegrationTests.Controllers
{
    public class ParticipantControllerTests
    {
        private readonly Mock<IExamTakingService> _mockTakingService;
        private readonly Mock<IExamService> _mockExamService;
        private readonly Mock<ILogger<ParticipantController>> _mockLogger;
        private readonly ParticipantController _controller;
        private readonly string _userId = "student-123";

        public ParticipantControllerTests()
        {
            _mockTakingService = new Mock<IExamTakingService>();
            _mockExamService = new Mock<IExamService>();
            _mockLogger = new Mock<ILogger<ParticipantController>>();

            // Matches your constructor: (ILogger, IExamService, IExamTakingService)
            _controller = new ParticipantController(
                _mockLogger.Object,
                _mockExamService.Object,
                _mockTakingService.Object);

            // Setup Mock User
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, _userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task JoinExam_ShouldRedirectToAvailableExams_WhenServiceFails()
        {
            // Arrange
            int examId = 1;
            _mockTakingService.Setup(s => s.JoinExamAsync(examId, _userId))
                .ReturnsAsync(Result<JoinExamResult>.Failure("Exam has already ended"));

            // Act
            var result = await _controller.JoinExam(examId);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("AvailableExams");
            _controller.TempData["ErrorMessage"].Should().Be("Exam has already ended");
        }

        [Fact]
        public async Task JoinExam_ShouldReturnExamPageView_OnSuccess()
        {
            // Arrange
            int examId = 1;
            var joinResult = new JoinExamResult { ParticipantExamId = 50 };
            var pageDto = new ExamPageDto { ParticipantExamId = 50, CurrentPage = 1 };

            _mockTakingService.Setup(s => s.JoinExamAsync(examId, _userId))
                .ReturnsAsync(Result<JoinExamResult>.Success(joinResult));

            _mockTakingService.Setup(s => s.GetExamPageAsync(50, 1, _userId))
                .ReturnsAsync(Result<ExamPageDto>.Success(pageDto));

            // Act
            var result = await _controller.JoinExam(examId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("ExamPage");
            viewResult.Model.Should().BeEquivalentTo(pageDto);
        }

        [Fact]
        public async Task SubmitExam_ShouldRedirectToAvailableExams_AfterProcessing()
        {
            // Arrange
            int pExamId = 50;
            _mockTakingService.Setup(s => s.SubmitExamAsync(pExamId, _userId))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.SubmitExam(pExamId, null);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("AvailableExams");
        }

        [Fact]
        public async Task NavigateExamPage_ShouldSaveAnswers_ThenReturnExamPage()
        {
            // Arrange
            int participantExamId = 70;
            int page = 2;

            var answers = new List<SaveAnswersRequest>
            {
                new SaveAnswersRequest { QuestionId = 1, SelectedAnswerId = 2 }
            };

            var pageDto = new ExamPageDto { ParticipantExamId = participantExamId };

            _mockTakingService
                .Setup(s => s.SaveParticipantAnswersAsync(
                    It.IsAny<List<SaveAnswersRequest>>(),
                    _userId,
                    participantExamId))
                .ReturnsAsync(Result.Success());

            _mockTakingService
                .Setup(s => s.GetExamPageAsync(participantExamId, page, _userId))
                .ReturnsAsync(Result<ExamPageDto>.Success(pageDto));

            // Act
            var result = await _controller.NavigateExamPage(
                participantExamId,
                page,
                answers
            );

            // Assert
            result.Should().BeOfType<ViewResult>();
        }


    }
}