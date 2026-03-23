namespace ExamSys.Application.DTOs.Course
{
    public class CourseTableItemDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime CourseDateCreated { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
    }
}
