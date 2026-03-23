using ExamSys.Core.ResponseModels.Question;

namespace ExamSys.Application.DTOs.ExamTaking
{
    public class CachedExamDetails
    {
        public List<QuestionWithAnswersResponse> AllQuestions { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalPages { get; set; }
    }
}
