namespace ExamSys.Core.ResponseModels.Question
{
    public class QuestionWithCountResponse
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int CorrectAnswerId { get; set; }
        public int ExamId { get; set; }
        public int Weight { get; set; }
        public int AnswersCount { get; set; }
    }
}
