namespace ExamSys.Core.ResponseModels.Course
{
    public class CourseInstructorResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime CourseDateCreated { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
    }
}
