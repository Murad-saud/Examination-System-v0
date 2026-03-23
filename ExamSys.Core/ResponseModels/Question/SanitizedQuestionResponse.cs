using ExamSys.Core.Entities;

namespace ExamSys.Core.ResponseModels.Question
{
    public class SanitizedQuestionResponse
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int ExamId { get; set; }
        public int Weight { get; set; }

        public List<Answer> Answers { get; set; }
    }
}
