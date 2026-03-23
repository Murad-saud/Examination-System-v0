using ExamSys.Application.DTOs.Answer;
using ExamSys.Application.DTOs.Question;
using ExamSys.Application.Interfaces;
using ExamSys.Core.Interfaces;

namespace ExamSys.Application.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public QuestionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<QuestionWithAnswersDto?> GetQuestionWithAnswersAsync(int questionId)
        {
            var resultDb = await _unitOfWork.Questions.GetByIdWithAnswersAsync(questionId);

            var questionDto = resultDb != null ? new QuestionWithAnswersDto()
            {
                ExamId = resultDb.ExamId,
                QuestionId = resultDb.QuestionId,
                QuestionText = resultDb.QuestionText,
                QuestionWeight = resultDb.QuestionWeight,
                CorrectAnswerId = resultDb.CorrectAnswerId,
                Answers = resultDb.Answers.Select(a => new AnswerDetailsDto()
                {
                    AnswerId = a.Id,
                    AnswerText = a.Text,
                    QuestionId = a.QuestionId
                }).ToList()
            } : null;

            return questionDto;
        }
    }
}
