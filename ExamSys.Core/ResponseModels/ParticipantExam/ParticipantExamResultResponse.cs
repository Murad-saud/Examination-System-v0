namespace ExamSys.Core.ResponseModels.ParticipantExam
{
    public class ParticipantExamResultResponse
    {
        public int ParticipantExamId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public int CorrectAnswersCount { get; set; }
        public int Score { get; set; }
    }
}
