namespace Examination_System.Dtos
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public string? JoinedDate { get; set; }
    }
}
