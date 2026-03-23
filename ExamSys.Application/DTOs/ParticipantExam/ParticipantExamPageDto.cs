using ExamSys.Application.DTOs.Question;

namespace ExamSys.Application.DTOs.ParticipantExam
{
    public class ParticipantExamPageDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int ParticipantExamId { get; set; }
        public string ParticipantName { get; set; }
        public List<QuestionSummaryDto> PageQuestions { get; set; }
        public Dictionary<int, int> SavedAnswers { get; set; } // QuestionId -> SelectedAnswerId
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
