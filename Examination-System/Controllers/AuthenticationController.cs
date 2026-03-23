using Examination_System.ViewModels;
using ExamSys.Application.DTOs.User;
using ExamSys.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Examination_System.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserService _userService;
        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var userDto = new UserRegisterDto
            {
                FullName = $"{registerVM.FirstName} {registerVM.MiddleName} {registerVM.LastName}",
                Email = registerVM.EmailAdress,
                Password = registerVM.Password,
                DateOfBirth = registerVM.DateOfBirth,
                RoleName = registerVM.RoleName
            };

            var registerUserResult = await _userService.RegisterUserAsync(userDto);
            if (registerUserResult.Any())
            {
                foreach (var error in registerUserResult)
                {
                    ModelState.AddModelError("", error);
                }
                return View(registerVM);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var userLoginDto = new UserLoginDto
            {
                Email = loginVM.EmailAddress,
                Password = loginVM.Password,
                RememberMe = loginVM.RememberMe
            };

            var isSucceeded = await _userService.SignInUserAsync(userLoginDto);
            if (!isSucceeded)
            {
                ModelState.AddModelError("", "Invalid login attempt, please try again");
                return View(loginVM);
            }

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            await _userService.SignOutUserAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
