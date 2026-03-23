using ExamSys.Core.Entities;
using ExamSys.Core.ResponseModels.ParticipantExam;

namespace ExamSys.Core.Interfaces.Repositories
{
    public interface IParticipantExamRepository : IRepository<ParticipantExam>
    {
        Task<ParticipantExam> GetByUserIdAsync(string userId);
        Task<ParticipantExam> GetOrCreateParticipantExamAsync(int examId, string userId);
        Task<ParticipantExam> GetActiveAttemptAsync(string participantId, int examId);
        Task<List<ParticipantExamResultResponse>> GetAllExamResults(int examId);
        Task<ParticipantExam> GetByIdWithExamAndParticipantAsync(int participantExamId);
    }
}
