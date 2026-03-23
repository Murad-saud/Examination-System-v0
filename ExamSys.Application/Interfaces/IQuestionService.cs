
using ExamSys.Application.DTOs.Question;

namespace ExamSys.Application.Interfaces
{
    public interface IQuestionService
    {
        Task<QuestionWithAnswersDto> GetQuestionWithAnswersAsync(int questionId);
    }
}
