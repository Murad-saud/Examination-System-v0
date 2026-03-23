namespace ExamSys.Application.DTOs.Exam
{
    public class ExamResultsDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public decimal TotalPoints { get; set; }
        public List<ParticipantExamResultDto> Results { get; set; }
    }
}
