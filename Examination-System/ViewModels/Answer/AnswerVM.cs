namespace Examination_System.ViewModels.Answer
{
    public class AnswerVM
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public bool isTheCorrectAnswer { get; set; }
    }
}
