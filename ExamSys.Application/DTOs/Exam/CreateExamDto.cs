namespace ExamSys.Application.DTOs.Exam
{
    public class CreateExamDto
    {
        public string ExamTitle { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public int ExamDuration { get; set; }
        public string InstructorId { get; set; }
    }
}
