namespace ExamSys.Application.DTOs.User
{
    public class UserRegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string RoleName { get; set; }
    }
}
