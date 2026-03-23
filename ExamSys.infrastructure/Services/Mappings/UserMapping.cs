using ExamSys.Application.DTOs.User;
using ExamSys.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ExamSys.Infrastructure.Services.Mappings
{
    public static class UserMapping
    {
        public static FullUserDto ToFullUserDto(this ApplicationUser applicationUser, string? role)
        {
            return new FullUserDto
            {
                Id = applicationUser.Id,
                FullName = applicationUser.FullName,
                Email = applicationUser.Email,
                RoleName = role,
                PhoneNumber = applicationUser.PhoneNumber ?? "1234567890",
                Age = DateTime.UtcNow.Year - applicationUser.DateOfBirth.Year,
                DateJoined = new DateTime(2020, 01, 01).ToString("MMMM dd, yyyy"),
            };
        }

        public static FullUserDtoV2 ToFullUserDtoV2(this ApplicationUser applicationUser, string? role)
        {
            return new FullUserDtoV2
            {
                Id = applicationUser.Id,
                FullName = applicationUser.FullName,
                Email = applicationUser.Email,
                RoleName = role,
                DateOfBirth = applicationUser.DateOfBirth.ToString("MMMM dd, yyyy"),
                DateJoined = applicationUser.DateCreated.ToString("MMMM dd, yyyy"),
            };
        }

        public static async Task<FullUserDto> ToFullUserDto(this ApplicationUser applicationUser, UserManager<ApplicationUser> userManager)
        {
            var userRoles = await userManager.GetRolesAsync(applicationUser);
            return new FullUserDto
            {
                Id = applicationUser.Id,
                FullName = applicationUser.FullName,
                Email = applicationUser.Email,
                RoleName = userRoles.FirstOrDefault(),
                PhoneNumber = applicationUser.PhoneNumber ?? "1234567890",
                Age = DateTime.UtcNow.Year - applicationUser.DateOfBirth.Year,
                DateJoined = new DateTime(2020, 01, 01).ToString("MMMM dd, yyyy"),
            };
        }
    }
}
