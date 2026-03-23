namespace ExamSys.Application.DTOs.Exam
{
    public class ExamSummaryDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public int QuestionsCount { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
    }
}
