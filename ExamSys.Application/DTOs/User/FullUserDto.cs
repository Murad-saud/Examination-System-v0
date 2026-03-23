namespace ExamSys.Application.DTOs.User
{
    public class FullUserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public string DateJoined { get; set; }
        public string RoleName { get; set; }
    }
}
