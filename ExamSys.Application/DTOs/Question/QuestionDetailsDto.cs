namespace ExamSys.Application.DTOs.Question
{
    public class QuestionDetailsDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionWeight { get; set; }
        public int AnswersCount { get; set; }
        //public int NoOfAnswers { get; set; }
    }
}
