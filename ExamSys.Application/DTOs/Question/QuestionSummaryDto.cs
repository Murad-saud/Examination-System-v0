using ExamSys.Application.DTOs.Answer;

namespace ExamSys.Application.DTOs.Question
{
    public class QuestionSummaryDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionWeight { get; set; }
        public int CorrectAnswerId { get; set; } // Added for correct answer highlighting
        public List<AnswerDto> Answers { get; set; }
    }
}
