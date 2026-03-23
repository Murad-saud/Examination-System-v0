using Examination_System.Helpers.Constants;
using Examination_System.Models;
using Examination_System.ViewModels.Admin;
using Examination_System.ViewModels.Course;
using ExamSys.Application.DTOs.Course;
using ExamSys.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Examination_System.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        public CoursesController(ICourseService courseService, IUserService userService)
        {
            _userService = userService;
            _courseService = courseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var createCourseVM = new CreateCourseVM
            {
                Instructors = await _userService.GetUsersInRoleAsync(RoleNames.Instructor),
                FormInputs = new CourseFormModel()
            };

            return View(createCourseVM);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseFormModel FormInputs)
        {
            // 1. Check for input validation errors (e.g., [Required] attributes)
            if (!ModelState.IsValid)
            {
                var createCourseVM = new CreateCourseVM
                {
                    Instructors = await _userService.GetUsersInRoleAsync(RoleNames.Instructor),
                    FormInputs = FormInputs
                };
                return View(createCourseVM);
            }

            // 2. Map to DTO and call service
            var courseDto = new CreateCourseDto()
            {
                CourseName = FormInputs.CourseName,
                InstructorId = FormInputs.InstructorId
            };

            // 3. Handle the Result object explicitly
            var result = await _courseService.CreateCourseAsync(courseDto);

            if (result.Failed)
            {
                // Add the specific error from the Result object to the ModelState
                ModelState.AddModelError("", result.Error);

                var createCourseVM = new CreateCourseVM
                {
                    Instructors = await _userService.GetUsersInRoleAsync(RoleNames.Instructor),
                    FormInputs = FormInputs
                };
                return View(createCourseVM);
            }

            // 4. Success! Redirect.
            return RedirectToAction("CoursesManagement");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCourse(int courseId)
        {
            // Get course with instructor details in a single call
            var courseWithInstructor = await _courseService.GetCourseWithInstructorAsync(courseId);
            if (courseWithInstructor is null) return NotFound();

            // Still need all instructors for the dropdown
            var allInstructors = await _userService.GetUsersInRoleAsync(RoleNames.Instructor);

            var updateCourseVM = new UpdateCourseVM()
            {
                AllInstructors = allInstructors,
                CourseInstructor = allInstructors.FirstOrDefault(i => i.Id == courseWithInstructor.InstructorId),
                FormInputs = new UpdateCourseFormModel()
                {
                    CourseId = courseWithInstructor.CourseId,
                    CourseName = courseWithInstructor.CourseName,
                    InstructorId = courseWithInstructor.InstructorId
                }
            };

            return View(updateCourseVM);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse(UpdateCourseFormModel FormInputs)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate and return the view to show errors
                var allInstructors = await _userService.GetUsersInRoleAsync(RoleNames.Instructor);
                var updateCourseVM = new UpdateCourseVM()
                {
                    AllInstructors = allInstructors,
                    FormInputs = FormInputs
                };
                return View(updateCourseVM);
            }

            var updateCourseDto = new UpdateCourseDto()
            {
                CourseId = FormInputs.CourseId,
                CourseName = FormInputs.CourseName,
                InstructorId = FormInputs.InstructorId
            };

            var result = await _courseService.UpdateCourse(updateCourseDto);
            if (!result)
            {
                ModelState.AddModelError("", "Updating course failed, please try again later");

                // Repopulate and return the view to show the error
                var allInstructors = await _userService.GetUsersInRoleAsync(RoleNames.Instructor);
                var updateCourseVM = new UpdateCourseVM()
                {
                    AllInstructors = allInstructors,
                    FormInputs = FormInputs
                };
                return View(updateCourseVM);
            }

            return RedirectToAction("CoursesManagement");
        }

        [HttpGet]
        public async Task<IActionResult> RemoveCourse(int courseId)
        {
            if (courseId == 0) return NotFound();

            var courseDto = await _courseService.GetCourseWithInstructorAsync(courseId);
            if (courseDto is null) return NotFound();

            var courseVM = new RemoveCourseVM()
            {
                CourseId = courseDto.CourseId,
                CourseName = courseDto.CourseName,
                InstructorName = courseDto.InstructorName
            };

            return View(courseVM);
        }

        [HttpPost]
        [HttpPost]
        [ActionName("RemoveCourse")]
        public async Task<IActionResult> RemoveCourseConfirmed(int courseId)
        {
            if (courseId == 0) return BadRequest();

            var result = await _courseService.RemoveCourse(courseId);
            if (!result)
            {
                ModelState.AddModelError("", "Something wrong happend, couldn't remove the course");
            }

            return RedirectToAction("CoursesManagement");
        }


        [HttpGet]
        public async Task<IActionResult> CoursesManagement()
        {
            var coursesDb = await _courseService.GetAllCoursesAsync();
            return View(coursesDb);
        }
    }
}
