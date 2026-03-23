using ExamSys.Application.DTOs.Answer;

namespace ExamSys.Application.DTOs.Exam
{
    public class ExamTakingQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int ExamId { get; set; }
        public int Weight { get; set; }

        public List<AnswerDetailsDto> Answers { get; set; }
    }
}
