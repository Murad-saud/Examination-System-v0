namespace ExamSys.Application.DTOs.Exam
{
    public class ParticipantExamResultDto
    {
        public int ParticipantExamId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public int CorrectAnswersCount { get; set; }
        public string Score { get; set; }
        public string FinalScorePercentage { get; set; }
    }
}
