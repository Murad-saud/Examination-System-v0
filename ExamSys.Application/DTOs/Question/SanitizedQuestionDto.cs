using ExamSys.Application.DTOs.Answer;

namespace ExamSys.Application.DTOs.Question
{
    public class SanitizedQuestionDto
    {
        //public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<AnswerPageDto> Answers { get; set; }
        public int QuestionWeight { get; set; }
    }
}
