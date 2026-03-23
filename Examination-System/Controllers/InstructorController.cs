using Examination_System.Models.Query;
using Examination_System.ViewModels.Exam;
using Examination_System.ViewModels.Instructor;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examination_System.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IExamService _examService;
        private readonly IExamStateManager _examStateManager;
        public InstructorController(ICourseService courseService, IExamService examService,
             IExamStateManager examStateManager)
        {
            _courseService = courseService;
            _examService = examService;
            _examStateManager = examStateManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MyExams([FromQuery] ExamSearchFilter? filter)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var instructorExams = await _examService.GetInstructorExams(InstructorId, filter.ExamStatus,
                 filter.CourseName, filter.Keywords);

            var myExamsVM = new MyExamsVM();
            myExamsVM.AvailableCourses = await _courseService.GetInstructorCourses(InstructorId);
            myExamsVM.FilterInputs = filter;

            if (instructorExams.Count == 0)
            {
                myExamsVM.Exams = new List<ExamTableItemVM>();
                return View(myExamsVM);
            }

            myExamsVM.Exams = instructorExams.Select(e => new ExamTableItemVM()
            {
                ExamId = e.ExamId,
                ExamTitle = e.ExamTitle,
                Status = e.Status,
                CourseName = e.CourseName,
                StartDate = e.StartDate,
                CanEdit = _examStateManager.CanEdit(e.Status),
                CanDelete = _examStateManager.CanDelete(e.Status),
                CanViewResults = _examStateManager.CanViewResults(e.Status),
                IsScheduled = e.Status == ExamStatus.Scheduled,
                IsInProgress = e.Status == ExamStatus.InProgress
            }).ToList();

            return View(myExamsVM);
        }

        public async Task<IActionResult> ExamResults(int examId)
        {
            if (examId <= 0) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ownerShipResult = await _examService.ValidateExamOwnership(examId, userId);

            if (!ownerShipResult.Succeeded)
            {
                TempData["ErrorMessage"] = ownerShipResult.Error;
                return RedirectToAction("MyExams");
            }

            var getResults = await _examService.GetAllParticipantExamsResults(examId);
            if (!getResults.Succeeded)
            {
                TempData["ErrorMessage"] = getResults.Error;
                return RedirectToAction("MyExams");
            }



            return View(getResults.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ParticipantExamPage(int examId, int participantExamId, int targetPage = 1)
        {
            if (examId <= 0 || participantExamId <= 0 || targetPage <= 0)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validate exam ownership
            var ownershipResult = await _examService.ValidateExamOwnership(examId, userId);
            if (!ownershipResult.Succeeded)
            {
                TempData["ErrorMessage"] = ownershipResult.Error;
                return RedirectToAction("ExamResults", new { examId });
            }

            var pageResult = await _examService.GetParticipantExamPageAsync(participantExamId, targetPage);
            if (!pageResult.Succeeded)
            {
                TempData["ErrorMessage"] = pageResult.Error;
                return RedirectToAction("ExamResults", new { examId });
            }

            return View(pageResult.Data);
        }

    }
}
