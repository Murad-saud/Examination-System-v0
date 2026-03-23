using ExamSys.Core.Entities;
using ExamSys.Core.Interfaces.Repositories;
using ExamSys.Core.ResponseModels.ParticipantExam;
using Microsoft.EntityFrameworkCore;

namespace ExamSys.Infrastructure.Data.Repositories
{
    public class ParticipantExamRepository : Repository<ParticipantExam>, IParticipantExamRepository
    {
        public ParticipantExamRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ParticipantExam> GetByUserIdAsync(string userId)
        {
            return await _context.ParticipantExam
                .Where(pe => pe.ParticipantId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<ParticipantExam> GetOrCreateParticipantExamAsync(int examId, string userId)
        {
            var participantExamDb = await _context.ParticipantExam
                .Where(p => p.ParticipantId == userId && p.ExamId == examId)
                .FirstOrDefaultAsync();

            if (participantExamDb is not null)
                return participantExamDb;

            var newParticipantExam = new ParticipantExam()
            {
                ExamId = examId,
                ParticipantId = userId,
                isSubmitted = false,
                Score = 0
            };

            await _context.ParticipantExam.AddAsync(newParticipantExam);
            await _context.SaveChangesAsync();

            return newParticipantExam;
        }

        public async Task<ParticipantExam> GetActiveAttemptAsync(string participantId, int examId)
        {
            return await _context.ParticipantExam
                .FirstOrDefaultAsync(pe =>
                    pe.ParticipantId == participantId &&
                    pe.ExamId == examId &&
                    !pe.isSubmitted);
        }

        public async Task<List<ParticipantExamResultResponse>> GetAllExamResults(int examId)
        {
            return await _context.ParticipantExam
                .Where(pe => pe.ExamId == examId && pe.isSubmitted)
                .Select(pe => new ParticipantExamResultResponse()
                {
                    ParticipantExamId = pe.Id,
                    ParticipantId = pe.ParticipantId,
                    ParticipantName = pe.Participant.FullName,
                    Score = pe.Score,
                    CorrectAnswersCount = pe.CorrectAnswersCount
                })
                .OrderByDescending(pe => pe.Score)
                .ToListAsync();
        }

        public async Task<ParticipantExam> GetByIdWithExamAndParticipantAsync(int participantExamId)
        {
            return await _context.ParticipantExam
                .Include(pe => pe.Participant)
                .Include(pe => pe.Exam)
                .FirstOrDefaultAsync(pe => pe.Id == participantExamId);
        }

    }
}
