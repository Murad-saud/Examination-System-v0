namespace ExamSys.Application.DTOs.Question
{
    public class CreateQuestionDto
    {
        public int ExamId { get; set; }
        public string QuestionText { get; set; }
        public List<string> AnswersText { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public int QuestionWeight { get; set; }
    }
}
