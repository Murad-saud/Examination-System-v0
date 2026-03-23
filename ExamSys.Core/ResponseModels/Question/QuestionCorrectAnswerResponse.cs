namespace ExamSys.Core.ResponseModels.Question
{
    public class QuestionCorrectAnswerResponse
    {
        public int QuestionId { get; set; }
        public int? CorrectAnswerId { get; set; }
        public int QuestionWeight { get; set; }
    }
}
