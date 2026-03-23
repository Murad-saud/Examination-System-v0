using ExamSys.Application.DTOs.ExamTaking;
using ExamSys.Core.ResponseModels.Question;

namespace ExamSys.Application.Interfaces
{
    public interface IExamCacheService
    {
        Task<CachedExamDetails> GetExamQuestionsAsync(int examId);
        Task SetExamQuestions(int examId, CachedExamDetails cachedExamDetails);
        Task<List<QuestionCorrectAnswerResponse>> GetAllCorrectAnswers(int examId);
    }
}
