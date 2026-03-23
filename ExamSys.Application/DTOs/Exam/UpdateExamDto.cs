namespace ExamSys.Application.DTOs.Exam
{
    public class UpdateExamDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
    }
}
