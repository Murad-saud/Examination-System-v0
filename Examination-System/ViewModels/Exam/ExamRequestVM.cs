namespace Examination_System.ViewModels.Exam
{
    public class ExamRequestVM
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int QuestionsCount { get; set; }
        public int ExamDuration { get; set; }
        public string CourseName { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
    }
}
