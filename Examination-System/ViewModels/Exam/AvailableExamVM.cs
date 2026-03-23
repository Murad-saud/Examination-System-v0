namespace Examination_System.ViewModels.Exam
{
    public class AvailableExamVM
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public string ExamStatus { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
    }
}
