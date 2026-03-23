namespace ExamSys.Application.DTOs.ExamTaking
{
    public class JoinExamResult
    {
        public int ExamId;
        public int ParticipantExamId { get; set; }
        public int TotalPages { get; set; }
        public DateTime ExamEndTime { get; set; } // Your approach
        // it was Utc, we will handle timing later.
    }
}
