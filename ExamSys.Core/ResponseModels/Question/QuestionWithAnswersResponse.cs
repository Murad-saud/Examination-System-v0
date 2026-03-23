using ExamSys.Core.Entities;

namespace ExamSys.Core.ResponseModels.Question
{
    public class QuestionWithAnswersResponse
    {
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<Answer> Answers { get; set; }
        public int? CorrectAnswerId { get; set; }
        public int QuestionWeight { get; set; }
    }
}
