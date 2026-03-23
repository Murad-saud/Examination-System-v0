using Examination_System.ViewModels.Exam;
using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examination_System.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ILogger<ParticipantController> _logger;
        private readonly IExamService _examService;
        private readonly IExamTakingService _examTakingService;

        public ParticipantController(ILogger<ParticipantController> logger, IExamService examService,
            IExamTakingService examTakingService)
        {
            _logger = logger;
            _examService = examService;
            _examTakingService = examTakingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AvailableExams()
        {
            var availableExamsDto = await _examService.GetAvailableExamsAsync();

            var availableExamsVM = availableExamsDto.Select(e => new AvailableExamVM()
            {
                ExamId = e.ExamId,
                ExamTitle = e.ExamTitle,
                ExamStatus = e.ExamStatus.ToString(),
                StartDate = e.StartDate,
                Duration = e.Duration,
                CourseName = e.CourseName,
                InstructorName = e.InstructorName
            }).ToList();

            return View(availableExamsVM);
        }

        public async Task<IActionResult> JoinExam(int examId)
        {
            if (examId <= 0) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var joinExamResult = await _examTakingService.JoinExamAsync(examId, userId);
            if (!joinExamResult.Succeeded)
            {
                TempData["ErrorMessage"] = joinExamResult.Error;
                return RedirectToAction("AvailableExams");
            }

            // Getting first page questions
            var participantExamId = joinExamResult.Data.ParticipantExamId;
            var examPageDto = await _examTakingService.GetExamPageAsync(participantExamId, 1, userId);

            if (examPageDto.Failed)
            {
                TempData["ErrorMessage"] = joinExamResult.Error;
                RedirectToAction("AvailableExams");
            }

            return View("ExamPage", examPageDto.Data);
        }

        [HttpPost]
        public async Task<IActionResult> NavigateExamPage(int participantExamId, int pageNumber
            , List<SaveAnswersRequest> saveAnswers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Filter out unanswered questions (SelectedAnswerId = 0)
            var answersToSave = saveAnswers?
                .Where(a => a.SelectedAnswerId > 0)
                .ToList();

            if (answersToSave?.Any() == true)
            {
                await _examTakingService.SaveParticipantAnswersAsync(answersToSave, userId, participantExamId);
            }

            // Your navigation logic...
            var nextPage = await _examTakingService.GetExamPageAsync(participantExamId, pageNumber, userId);
            return View("ExamPage", nextPage.Data);
        }

        // NEW submit action
        [HttpPost]
        public async Task<IActionResult> SubmitExam(
            int participantExamId,
            List<SaveAnswersRequest> saveAnswers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // 1. Save final page answers
            var answersToSave = saveAnswers?.Where(a => a.SelectedAnswerId > 0).ToList();
            if (answersToSave?.Any() == true)
            {
                await _examTakingService.SaveParticipantAnswersAsync(answersToSave, userId, participantExamId);
            }

            // 2. Submit the exam
            var submitResult = await _examTakingService.SubmitExamAsync(participantExamId, userId);

            return RedirectToAction("AvailableExams");
        }
    }
}
