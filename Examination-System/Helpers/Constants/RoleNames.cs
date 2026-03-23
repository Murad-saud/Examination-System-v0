namespace Examination_System.Helpers.Constants
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Instructor = "Instructor";
        public const string Participant = "Participant";

        public static readonly IReadOnlyList<string> All = new[] { Admin, Instructor, Participant };
    }
}
