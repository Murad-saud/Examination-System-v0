using ExamSys.Application.DTOs.Question;
using ExamSys.Core.Enums;

namespace ExamSys.Application.DTOs.Exam
{
    public class ExamBuilderDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int QuestionsCount { get; set; }
        public ExamStatus ExamStatus { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public List<QuestionDetailsDto> ExamQuestions { get; set; }
    }
}
