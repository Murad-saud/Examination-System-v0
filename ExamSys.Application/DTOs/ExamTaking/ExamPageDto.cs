using ExamSys.Application.DTOs.Question;

namespace ExamSys.Application.DTOs.ExamTaking
{
    public class ExamPageDto
    {
        // hidden inputs (with token)
        public int ExamId { get; set; }
        public int ParticipantExamId { get; set; }
        //public string ExamTitle { get; set; }

        public List<SanitizedQuestionDto> PageQuestions { get; set; }
        public Dictionary<int, int> SavedAnswers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public DateTime EndTime { get; set; }
        // No TimeRemaining here - client calculates from ExamEndTimeUtc
    }
}
