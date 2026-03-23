using ExamSys.Application.DTOs.Question;

namespace Examination_System.ViewModels.Exam
{
    public class ExamBuilderVM
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string ExamStatus { get; set; }
        public DateTime StartDate { get; set; }
        public int QuestionsCount { get; set; }
        public string CourseName { get; set; }
        public List<QuestionDetailsDto> ExamQuestions { get; set; }
    }
}
