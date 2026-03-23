namespace ExamSys.Core.ResponseModels.Course
{
    public class CourseWithInstructorResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
