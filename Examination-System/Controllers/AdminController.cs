using Examination_System.Models.Query;
using Examination_System.ViewModels;
using Examination_System.ViewModels.Admin;
using Examination_System.ViewModels.Exam;
using ExamSys.Application.DTOs.User;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IExamService _examService;
        public AdminController(IUserService userService, IExamService examService)
        {
            _userService = userService;
            _examService = examService;
        }
        public IActionResult Dashboard() => View();

        public IActionResult ExamManagement() => View();


        [HttpGet]
        public async Task<IActionResult> UsersManagement([FromQuery] UserSearchFilter userSearchFilter)
        {
            var userManagementVm = new UserManagementVMv2()
            {
                Filters = userSearchFilter
            };
            userManagementVm.Users = await _userService.GetAllFilteredUsersV2(userSearchFilter.SearchBy,
                 userSearchFilter.Keywords, userSearchFilter.Role);

            return View(userManagementVm); //the view will show a text that says no users matched if the list count was 0
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            if (userId is null) return NotFound();

            var userDto = await _userService.GetUserByIdAsync(userId);
            if (userDto is null)
            {
                return NotFound();
            }

            var editUserVM = new EditUserVM
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                MiddleName = userDto.MiddleName,
                LastName = userDto.LastName,
                EmailAddress = userDto.Email,
                DateOfBirth = userDto.DateOfBirth,
                RoleName = userDto.RoleName
            };

            return View(editUserVM);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserVM editUserVm)
        {
            if (!ModelState.IsValid) return View(editUserVm);

            var editUserDto = new EditUserDto
            {
                Id = editUserVm.Id,
                FirstName = editUserVm.FirstName,
                MiddleName = editUserVm.MiddleName,
                LastName = editUserVm.LastName,
                Email = editUserVm.EmailAddress,
                DateOfBirth = editUserVm.DateOfBirth,
                RoleName = editUserVm.RoleName
            };

            var isSuccess = await _userService.UpdateUserDetailsAsync(editUserDto);
            if (!isSuccess)
            {
                ModelState.AddModelError("", "Failed to update user. Please try again.");
                return View(editUserVm);
            }

            return RedirectToAction("UsersManagement");
        }

        [HttpGet]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            if (userId is null) return NotFound();

            var userDto = await _userService.GetUserByIdAsync(userId);
            if (userDto is null) return NotFound();

            var removeUserVM = new RemoveUserVM
            {
                Id = userDto.Id,
                FullName = $"{userDto.FirstName} {userDto.MiddleName} {userDto.LastName}",
                Email = userDto.Email,
                DateOfBirth = userDto.DateOfBirth.ToString("dd/MM/yyyy"),
                DateCreated = DateTime.UtcNow.AddYears(-2).ToString("dd/MM/yyyy"), //just for testing(i'm lazy to modify UserDto)
                RoleName = userDto.RoleName
            };

            return View(removeUserVM);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUser(RemoveUserVM removeUserVM)
        {
            var RemoveUserResult = await _userService.RemoveUserAsync(removeUserVM.Id);
            if (!RemoveUserResult)
            {
                ModelState.AddModelError("", "I'm lazy right now, couldn't delete the user right now and that's it ... try again later");
                return View(removeUserVM);
            }

            return RedirectToAction("UsersManagement");
        }

        public async Task<IActionResult> PendingExams()
        {
            var examSummaryDto = await _examService.GetExamsByState(ExamStatus.Pending);
            var examSummaryVM = new List<ExamSummaryVM>();

            if (examSummaryDto.IsNullOrEmpty())
            {
                return View(examSummaryVM);
            }

            examSummaryVM = examSummaryDto.Select(e => new ExamSummaryVM()
            {
                ExamId = e.ExamId,
                ExamTitle = e.ExamTitle,
                StartDate = e.StartDate,
                Duration = e.Duration,
                QuestionsCount = e.QuestionsCount,
                CourseId = e.CourseId,
                CourseName = e.CourseName,
                InstructorId = e.InstructorId,
                InstructorName = e.InstructorName
            }).ToList();

            return View(examSummaryVM);
        }

        public async Task<IActionResult> ExamApproval(int examId, bool isApproved)
        {

            if (examId == 0) return NotFound();

            var newState = isApproved switch
            {
                true => ExamStatus.Scheduled,
                false => ExamStatus.Rejected
            };

            var result = await _examService.ChangeExamStateAsync(examId, newState);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
            }
            else
            {
                TempData["SuccessMessage"] = isApproved
                    ? "Exam approved and scheduled successfully!"
                    : "Exam rejected successfully!";
            }

            return RedirectToAction("PendingExams");
        }

    }


}
