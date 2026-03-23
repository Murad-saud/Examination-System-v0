namespace ExamSys.Core.Entities
{
    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }

        //Navigation Properties
        public Question Question { get; set; }
    }
}
