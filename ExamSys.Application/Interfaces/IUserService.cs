using ExamSys.Application.DTOs.User;

namespace ExamSys.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> IsUserAlreadyRegisteredAsync(string userEmail);
        Task<List<string>> RegisterUserAsync(UserRegisterDto userRegisterDto);
        Task<bool> SignInUserAsync(UserLoginDto userLoginDto);
        Task SignOutUserAsync();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserDetailsAsync(EditUserDto editUserDto);
        Task<bool> RemoveUserAsync(string userId);
        Task<List<FullUserDto>> GetAllFilteredUsers(string? searchBy, string? keywords, string? roleName);
        Task<List<FullUserDtoV2>> GetAllFilteredUsersV2(string? searchBy, string? keywords, string? roleName);
        Task<List<UserNameDto>> GetUsersInRoleAsync(string roleName);
    }
}
