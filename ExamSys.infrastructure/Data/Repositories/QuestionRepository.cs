using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces.Repositories;
using ExamSys.Core.ResponseModels.Question;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Data.Repositories
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Question?> GetByExamAndTextAsync(int examId, string questionText)
        {
            return await _context.Questions
                .Where(q => q.ExamId == examId && q.Text == questionText)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetQuestionsCount(int examId)
        {
            return await _context.Questions
                .Where(e => e.ExamId == examId)
                .CountAsync();
        }

        public async Task<QuestionWithAnswersResponse?> GetByIdWithAnswersAsync(int questionId)
        {
            return await _context.Questions
            .Where(q => q.Id == questionId)
            .Select(q => new QuestionWithAnswersResponse()
            {
                ExamId = q.ExamId,
                QuestionId = q.Id,
                QuestionText = q.Text,
                CorrectAnswerId = (int)q.CorrectAnswerId,
                QuestionWeight = q.Weight,
                Answers = q.Answers.ToList()
            }).FirstOrDefaultAsync();

        }

        public async Task<List<SanitizedQuestionResponse>> GetExamSanitizedQuestions(int examId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.Id == examId)
                .OrderBy(q => q.Id)
                .Select(q => new SanitizedQuestionResponse()
                {
                    ExamId = q.ExamId,
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    Weight = q.Weight,
                    Answers = q.Answers.ToList()
                }).ToListAsync();
        }

        public async Task<List<QuestionWithAnswersResponse>> GetAllExamQuestionsAsync(int examId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.ExamId == examId)
                .OrderBy(q => q.Id)
                .Select(q => new QuestionWithAnswersResponse()
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    QuestionWeight = q.Weight,
                    CorrectAnswerId = q.CorrectAnswerId,
                    Answers = q.Answers.ToList()
                }).ToListAsync();
        }

        public async Task<List<QuestionCorrectAnswerResponse>> GetAllExamCorrectAnswers(int examId)
        {
            return await _context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => new QuestionCorrectAnswerResponse()
            {
                QuestionId = q.Id,
                CorrectAnswerId = q.CorrectAnswerId,
                QuestionWeight = q.Weight
            })
            .ToListAsync();
        }

        public async Task<decimal> GetExamTotalPointsAsync(int examId)
        {
            return await _context.Questions
                .Where(q => q.ExamId == examId)
                .SumAsync(q => q.Weight);
        }

        public async Task<PagedQuestionsResult> GetExamQuestionsWithAnswersAsync(int examId, int page, int pageSize)
        {
            var baseQuery = _context.Questions
                .Where(q => q.ExamId == examId)
                .Include(q => q.Answers);

            var totalCount = await baseQuery.CountAsync();
            var questions = await baseQuery
                .OrderBy(q => q.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedQuestionsResult
            {
                Questions = questions,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

    }
}
