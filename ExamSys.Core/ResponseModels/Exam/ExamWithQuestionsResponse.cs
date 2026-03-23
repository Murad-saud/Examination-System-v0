using ExamSys.Core.Enums;
using ExamSys.Core.ResponseModels.Question;

namespace ExamSys.Core.ResponseModels.Exam
{
    public class ExamWithQuestionsResponse
    {
        public int ExamId { get; set; }
        public string InstructorId { get; set; }
        public string ExamTitle { get; set; }
        public ExamStatus Status { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public List<QuestionWithCountResponse> ExamQuestions { get; set; }
    }
}
