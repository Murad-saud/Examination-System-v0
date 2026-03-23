namespace ExamSys.Core.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int? CorrectAnswerId { get; set; }
        public int ExamId { get; set; }
        public int Weight { get; set; }

        //Navigation Properties
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public Answer CorrectAnswer { get; set; }
    }
}
