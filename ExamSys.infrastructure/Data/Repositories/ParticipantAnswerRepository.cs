using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces.Repositories;
using ExamSys.Core.ResponseModels.Exam;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Data.Repositories
{
    public class ParticipantAnswerRepository : Repository<ParticipantAnswer>, IParticipantAnswerRepository
    {
        public ParticipantAnswerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ParticipantAnswer>> GetAllParticipantAnswersAsync(int participantExamId)
        {
            return await _context.ParticipantAnswers
                .Where(pa => pa.ParticipantExamId == participantExamId)
                .OrderBy(pa => pa.QuestionId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetPageSavedAnswersAsync(int participanExamId, List<int> questionsIds)
        {
            var result = await _context.ParticipantAnswers
                .Where(pa => pa.ParticipantExamId == participanExamId)
                .Where(pa => questionsIds.Contains(pa.QuestionId))
                .GroupBy(pa => pa.QuestionId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.First().SelectedAnswerId
                );

            return result;
        }

        public async Task<List<ParticipantAnswer>> GetParticipantSavedAnswersAsync(int participantExamId, List<int> questionIds)
        {
            var result = await _context.ParticipantAnswers
                .Where(pe => pe.ParticipantExamId == participantExamId)
                .Where(pa => questionIds.Contains(pa.QuestionId))
                .ToListAsync();

            return result;
        }

        public async Task<List<CorrectionResult>> AutoCorrectExamAsync(int participantExamId)
        {
            var results = await _context.ParticipantAnswers
                .Where(pa => pa.ParticipantExamId == participantExamId)
                .Join(_context.Questions,
                    pa => pa.QuestionId,
                    q => q.Id,
                    (pa, q) => new CorrectionResult
                    {
                        QuestionId = pa.QuestionId,
                        SelectedAnswerId = pa.SelectedAnswerId,
                        CorrectAnswerId = q.CorrectAnswerId,
                        IsCorrect = pa.SelectedAnswerId == q.CorrectAnswerId,
                        Weight = q.Weight
                    })
                .ToListAsync();

            return results;
        }

    }
}
