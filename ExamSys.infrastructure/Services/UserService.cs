using ExamSys.Application.DTOs.User;
using ExamSys.Application.Interfaces;
using ExamSys.Infrastructure.Data;
using ExamSys.Infrastructure.Identity;
using ExamSys.Infrastructure.Services.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly List<string> RoleNames = new List<string> { "Admin", "Teacher", "Student" };
        public UserService(AppDbContext context, UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<List<FullUserDto>> GetAllFilteredUsers(string? searchBy, string? keywords,
             string? roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var allUsersInRole = await _userManager.GetUsersInRoleAsync(roleName);


                if (!string.IsNullOrEmpty(keywords))
                {
                    allUsersInRole = searchBy switch
                    {
                        "emailaddress" => allUsersInRole.Where(u => u.Email.Contains(keywords)).ToList(),
                        "phonenumber" => allUsersInRole.Where(u => u.PhoneNumber == keywords).ToList(),
                        _ => allUsersInRole.Where(u => u.FullName.Contains(keywords)).ToList(),
                    };
                }

                var allUsersDto = new List<FullUserDto>();
                foreach (var user in allUsersInRole)
                {
                    allUsersDto.Add(user.ToFullUserDto(roleName));
                }

                return allUsersDto;
            }
            else
            {
                var allUsersInRoles = await _context.Users.ToListAsync();

                if (!string.IsNullOrEmpty(keywords))
                {
                    allUsersInRoles = searchBy switch
                    {
                        "emailaddress" => allUsersInRoles.Where(u => u.Email.Contains(keywords)).ToList(),
                        "phonenumber" => allUsersInRoles.Where(u => u.PhoneNumber == keywords).ToList(),
                        _ => allUsersInRoles.Where(u => u.FullName.Contains(keywords)).ToList(),
                    };
                }

                var allUsersDto = new List<FullUserDto>();
                foreach (var user in allUsersInRoles)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    string? role = userRoles.FirstOrDefault();

                    allUsersDto.Add(user.ToFullUserDto(role));
                }

                return allUsersDto;
            }
        }

        public async Task<List<FullUserDtoV2>> GetAllFilteredUsersV2(string? searchBy, string? keywords, string? roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var allUsersInRole = await _userManager.GetUsersInRoleAsync(roleName);


                if (!string.IsNullOrEmpty(keywords))
                {
                    allUsersInRole = searchBy switch
                    {
                        "emailaddress" => allUsersInRole.Where(u => u.Email.Contains(keywords)).ToList(),
                        "phonenumber" => allUsersInRole.Where(u => u.PhoneNumber == keywords).ToList(),
                        _ => allUsersInRole.Where(u => u.FullName.Contains(keywords)).ToList(),
                    };
                }

                var allUsersDto = new List<FullUserDtoV2>();
                foreach (var user in allUsersInRole)
                {
                    allUsersDto.Add(user.ToFullUserDtoV2(roleName));
                }

                return allUsersDto;
            }
            else
            {
                var allUsersInRoles = await _context.Users.ToListAsync();

                if (!string.IsNullOrEmpty(keywords))
                {
                    allUsersInRoles = searchBy switch
                    {
                        "emailaddress" => allUsersInRoles.Where(u => u.Email.Contains(keywords)).ToList(),
                        "phonenumber" => allUsersInRoles.Where(u => u.PhoneNumber == keywords).ToList(),
                        _ => allUsersInRoles.Where(u => u.FullName.Contains(keywords)).ToList(),
                    };
                }

                var allUsersDto = new List<FullUserDtoV2>();
                foreach (var user in allUsersInRoles)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    string? role = userRoles.FirstOrDefault();

                    allUsersDto.Add(user.ToFullUserDtoV2(role));
                }

                return allUsersDto;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var userDb = await _userManager.FindByIdAsync(userId);
            if (userDb is null)
                return null;

            // More efficient: get roles and handle potential absence
            var roles = await _userManager.GetRolesAsync(userDb);

            // Split the name just once instead of three times
            var nameParts = userDb.FullName.Split(' ');

            return new UserDto
            {
                Id = userId,
                FirstName = nameParts[0],
                MiddleName = nameParts[1],
                LastName = nameParts[2],
                Email = userDb.Email,
                DateOfBirth = userDb.DateOfBirth, // Explicit format
                RoleName = roles.FirstOrDefault() // This might be null if user has no roles
            };
        }

        public async Task<List<UserNameDto>> GetUsersInRoleAsync(string roleName)
        {
            var allUsersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            var usersDto = allUsersInRole.Select(u => new UserNameDto
            {
                Id = u.Id,
                FullName = u.FullName,
            }).ToList();

            return usersDto;
        }

        public async Task<bool> IsUserAlreadyRegisteredAsync(string userEmail)
        {
            var userDb = await _userManager.FindByEmailAsync(userEmail);
            return userDb != null;
        }

        public async Task<List<string>> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            // Start a transaction to ensure atomicity
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newUser = new ApplicationUser
                {
                    FullName = userRegisterDto.FullName,
                    Email = userRegisterDto.Email,
                    UserName = userRegisterDto.Email,
                    DateOfBirth = userRegisterDto.DateOfBirth,
                    DateCreated = DateTime.UtcNow,
                };

                // 1. Create the user
                var createResult = await _userManager.CreateAsync(newUser, userRegisterDto.Password);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return createResult.Errors.Select(e => e.Description).ToList();
                }

                // 2. Assign the role
                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, userRegisterDto.RoleName);
                if (!addToRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return addToRoleResult.Errors.Select(e => e.Description).ToList();
                }

                // 3. Commit the transaction only if both steps succeeded
                await transaction.CommitAsync();

                // 4. Sign in the user (outside the transaction, as it's not a DB operation)
                await _signInManager.SignInAsync(newUser, isPersistent: false);

                // Return an empty list indicating success
                return new List<string>();
            }
            catch (Exception ex)
            {
                // Critical: Roll back the transaction on any unexpected exception
                await transaction.RollbackAsync();

                // Log the exception here (ex)
                // Return a generic error message. Do not expose exception details.
                return new List<string> { "An error occurred during registration. Please try again." };
            }
        }

        public async Task<bool> RemoveUserAsync(string userId)
        {
            if (userId is null) return false;

            var userDb = await _userManager.FindByIdAsync(userId);
            if (userDb is null) return false;

            var deleteResult = await _userManager.DeleteAsync(userDb);
            if (!deleteResult.Succeeded) return false;

            return true;
        }

        public async Task<bool> SignInUserAsync(UserLoginDto userLoginDto)
        {
            // Use PasswordSignInAsync with email as userName (since UserName is set to Email during registration)
            var signInResult = await _signInManager.PasswordSignInAsync(userLoginDto.Email, userLoginDto.Password, userLoginDto.RememberMe, lockoutOnFailure: false);
            return signInResult.Succeeded;
        }

        public async Task SignOutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> UpdateUserDetailsAsync(EditUserDto editUserDto)
        {
            var userDb = await _userManager.FindByIdAsync(editUserDto.Id);
            if (userDb is null) return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            userDb.FullName = $"{editUserDto.FirstName} {editUserDto.MiddleName} {editUserDto.LastName}";
            userDb.Email = editUserDto.Email;
            userDb.UserName = editUserDto.Email;
            userDb.DateOfBirth = editUserDto.DateOfBirth;

            var updateUserResult = await _userManager.UpdateAsync(userDb);
            if (!updateUserResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Update role operation
            var userRoles = await _userManager.GetRolesAsync(userDb);
            if (!userRoles.Contains(editUserDto.RoleName))
            {
                //removing current user Roles
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(userDb, userRoles);
                if (!removeRolesResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(userDb, editUserDto.RoleName);
                if (!addToRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            // If adding to role and updating user details done successfully
            await transaction.CommitAsync();
            return true;
        }
    }
}
