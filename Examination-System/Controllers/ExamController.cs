using Examination_System.Models;
using Examination_System.Models.Query;
using Examination_System.ViewModels.Answer;
using Examination_System.ViewModels.Exam;
using Examination_System.ViewModels.Question;
using ExamSys.Application.DTOs.Exam;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Examination_System.Controllers
{
    [Authorize]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly ICourseService _courseService;
        private readonly IQuestionService _questionService;
        public ExamController(IExamService examService, ICourseService courseService,
            IQuestionService questionService)
        {
            _examService = examService;
            _courseService = courseService;
            _questionService = questionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateExam()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var instructorCourses = await _courseService.GetInstructorCourses(userId);

            // i should make a specific view for that 
            if (instructorCourses.Count == 0) return NotFound("Instructor have no associated courses");

            var createExamVM = new CreateExamVM()
            {
                AvailableCourses = instructorCourses,
                FormInputs = new CreateExamFormModel()
                {
                    InstructorId = userId
                }
            };

            return View(createExamVM);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExam(CreateExamFormModel FormInputs)
        {
            if (!ModelState.IsValid)
            {
                var createExamVm = new CreateExamVM()
                {
                    AvailableCourses = await _courseService
                        .GetInstructorCourses(FormInputs.InstructorId),
                    FormInputs = FormInputs
                };

                return View(createExamVm);
            }

            var createExamDto = new CreateExamDto()
            {
                ExamTitle = FormInputs.ExamTitle,
                CourseId = FormInputs.CourseId,
                StartDate = FormInputs.StartDate,
                ExamDuration = FormInputs.ExamDuration,
                InstructorId = FormInputs.InstructorId
            };

            var examID = await _examService.CreateExamAsync(createExamDto);

            if (examID == 0)
            {
                //ModelState.AddModelError("", result.Error);
                var createExamVm = new CreateExamVM()
                {
                    AvailableCourses = await _courseService.GetInstructorCourses(FormInputs.InstructorId),
                    FormInputs = FormInputs
                };

                return View(createExamVm);
            }


            // Temporarily until creating a convenient view
            return RedirectToAction("ExamBuilder", new { examId = examID });
        }

        public async Task<IActionResult> EditExamDetails(int examId)
        {
            if (examId <= 0) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var examDto = await _examService.GetExamByIdAsync(examId);

            if (examDto is null) return NotFound();
            if (userId != examDto.InstructorId) return Forbid();

            var availableCourses = await _courseService.GetInstructorCourses(userId);
            var editExamVM = new EditExamDetailsVM()
            {
                AvailableCourses = availableCourses,
                FormInputs = new UpdateExamDetailsFormModel()
                {
                    ExamId = examDto.ExamId,
                    ExamTitle = examDto.ExamTitle,
                    StartDate = examDto.StartDate,
                    Duration = examDto.Duration,
                    CourseId = examDto.CourseId
                }
            };

            return View(editExamVM);
        }

        [HttpPost]
        public async Task<IActionResult> EditExamDetails(UpdateExamDetailsFormModel formInputs)
        {
            if (!ModelState.IsValid)
            {
                //DRY --> I know
                return await GetEditExamViewWithCourses(formInputs);
            }

            var updateExamDto = new UpdateExamDto()
            {
                ExamId = formInputs.ExamId,
                ExamTitle = formInputs.ExamTitle,
                StartDate = formInputs.StartDate,
                Duration = formInputs.Duration,
                CourseId = formInputs.CourseId
            };

            var updateResult = await _examService.UpdateExamAsync(updateExamDto);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", updateResult.Error);
                //DRY --> I know
                return await GetEditExamViewWithCourses(formInputs);
            }

            return RedirectToAction("MyExams", "Instructor");
        }

        //[HttpPost]
        public async Task<IActionResult> RemoveExam(int examId)
        {
            if (examId <= 0) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            // First, get the exam to check ownership
            var exam = await _examService.GetExamByIdAsync(examId);
            if (exam == null)
            {
                return NotFound();
            }

            // Check if the user is an admin OR the instructor who owns the exam
            if (!userRoles.Contains("Admin") && exam.InstructorId != userId)
            {
                return Forbid(); // Or NotFound() to avoid revealing existence of exams
            }

            var deleteResult = await _examService.DeleteExamAsync(examId);

            if (!deleteResult.Succeeded)
            {
                TempData["ErrorMessage"] = deleteResult.Error;
            }
            else
            {
                TempData["SuccessMessage"] = "Exam deleted successfully";
            }

            return RedirectToAction("MyExams", "Instructor");
        }

        public async Task<IActionResult> ExamBuilder(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var examDetails = await _examService.GetExamDetailsAsync(examId);
            if (examDetails is null)
            {
                return NotFound();
            }

            var examBuilderVM = new ExamBuilderVM()
            {
                ExamId = examDetails.ExamId,
                ExamTitle = examDetails.ExamTitle,
                ExamStatus = examDetails.ExamStatus.ToString(),
                StartDate = examDetails.StartDate,
                CourseName = examDetails.CourseName,
                QuestionsCount = examDetails.QuestionsCount,
                ExamQuestions = examDetails.ExamQuestions
            };

            return View(examBuilderVM);
        }

        public async Task<IActionResult> SubmitExam(int examId)
        {
            if (examId == 0) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _examService.SubmitExamAsync(examId, userId);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("ExamBuilder", new { examId = examId });
            }

            return RedirectToAction("MyExams", "Instructor", new ExamSearchFilter { ExamStatus = 2 });
        }


        public async Task<IActionResult> AddQuestion(int examId)
        {
            var createQuestionVM = new CreateQuestionFormModel()
            {
                ExamId = examId
            };

            return View(createQuestionVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(CreateQuestionFormModel formModel)
        {
            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            var createQuestionDto = new CreateQuestionDto()
            {
                ExamId = formModel.ExamId,
                QuestionText = formModel.QuestionText.Trim(),
                AnswersText = formModel.AnswersText.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                CorrectAnswerIndex = formModel.IndexOfCorrectAnswer,
                QuestionWeight = formModel.QuestionWeight
            };

            var createQuestionResult = await _examService.AddQuestionToExam(createQuestionDto);
            if (!createQuestionResult.Succeeded)
            {
                ModelState.AddModelError("", createQuestionResult.Error);
                return View(formModel);
            }

            return RedirectToAction("ExamBuilder", new { examId = formModel.ExamId });
        }

        public async Task<IActionResult> RemoveQuestion(int questionId, int examId)
        {
            if (questionId == 0) return NotFound();

            var removingResult = await _examService.DeleteQuestionFromExam(questionId);
            if (!removingResult.Succeeded)
            {
                ModelState.AddModelError("", removingResult.Error);
            }

            var id = examId;
            return RedirectToAction("ExamBuilder", new { examId = id });

        }


        public async Task<IActionResult> UpdateQuestion(int questionId)
        {
            if (questionId == 0) return NotFound();

            var questionDto = await _questionService.GetQuestionWithAnswersAsync(questionId);

            if (questionDto is null) return NotFound();

            var answersVM = questionDto.Answers.Select(a => new AnswerVM()
            {
                QuestionId = a.QuestionId,
                AnswerId = a.AnswerId,
                AnswerText = a.AnswerText,
                isTheCorrectAnswer = a.AnswerId == questionDto.CorrectAnswerId
            }).ToList();

            var updateQuestionVM = new UpdateQuestionVM()
            {
                ExamId = questionDto.ExamId,
                QuestionId = questionDto.QuestionId,
                QuestionText = questionDto.QuestionText,
                QuestionWeight = questionDto.QuestionWeight,
                CorrectAnswerId = (int)questionDto.CorrectAnswerId,
                Answers = answersVM,
                CorrectAnswerIndex = answersVM.Where(e => e.isTheCorrectAnswer == true)
                    .Select(e => answersVM.IndexOf(e))
                    .FirstOrDefault()

            };
            updateQuestionVM.CorrectAnswerIndex++;

            return View(updateQuestionVM);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuestion(UpdateQuestionVM updateQuestionVM)
        {
            throw new NotImplementedException();
        }


        //Helper Methods <======================================================================>
        private async Task<IActionResult> GetEditExamViewWithCourses(UpdateExamDetailsFormModel formInputs)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var availableCourses = await _courseService.GetInstructorCourses(userId);
            var editExamVM = new EditExamDetailsVM()
            {
                AvailableCourses = availableCourses,
                FormInputs = formInputs
            };
            return View(editExamVM);
        }

    }
}
