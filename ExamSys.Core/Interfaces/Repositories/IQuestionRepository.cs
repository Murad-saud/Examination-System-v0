using ExamSys.Core.Entities;
using ExamSys.Core.ResponseModels.Question;

namespace ExamSys.Core.Interfaces.Repositories
{
    public interface IQuestionRepository : IRepository<Question>
    {
        Task<Question> GetByExamAndTextAsync(int examId, string questionText);
        Task<int> GetQuestionsCount(int examId);
        Task<QuestionWithAnswersResponse> GetByIdWithAnswersAsync(int questionId);
        Task<List<SanitizedQuestionResponse>> GetExamSanitizedQuestions(int examId);
        Task<List<QuestionWithAnswersResponse>> GetAllExamQuestionsAsync(int examId);
        Task<List<QuestionCorrectAnswerResponse>> GetAllExamCorrectAnswers(int examId);
        Task<decimal> GetExamTotalPointsAsync(int examId);
        Task<PagedQuestionsResult> GetExamQuestionsWithAnswersAsync(int examId, int page, int pageSize);
    }
}
