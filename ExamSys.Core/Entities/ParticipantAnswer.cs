namespace ExamSys.Core.Entities
{
    public class ParticipantAnswer
    {
        public int Id { get; set; }
        public int ParticipantExamId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }
}
