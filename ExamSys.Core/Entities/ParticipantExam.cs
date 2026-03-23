using ExamSys.Core.Interfaces;

namespace ExamSys.Core.Entities
{
    public class ParticipantExam
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string ParticipantId { get; set; }
        public int Score { get; set; }
        public int CorrectAnswersCount { get; set; }
        public bool isSubmitted { get; set; }

        //Navigation Properties
        public Exam Exam { get; set; }
        public virtual IUser Participant { get; set; }
        public ICollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new List<ParticipantAnswer>();
    }
}