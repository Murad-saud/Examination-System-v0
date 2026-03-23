using ExamSys.Application.DTOs.Answer;

namespace ExamSys.Application.DTOs.Question
{
    public class QuestionWithAnswersDto
    {
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<AnswerDetailsDto> Answers { get; set; }
        public int? CorrectAnswerId { get; set; }
        public int QuestionWeight { get; set; }
    }
}
