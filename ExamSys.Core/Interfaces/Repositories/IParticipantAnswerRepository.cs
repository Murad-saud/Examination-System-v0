using ExamSys.Core.Entities;
using ExamSys.Core.ResponseModels.Exam;

namespace ExamSys.Core.Interfaces.Repositories
{
    public interface IParticipantAnswerRepository : IRepository<ParticipantAnswer>
    {
        Task<Dictionary<int, int>> GetPageSavedAnswersAsync(int participanExamId, List<int> questionsIds);
        Task<List<ParticipantAnswer>> GetParticipantSavedAnswersAsync(int participantExamId, List<int> questionIds);
        Task<List<ParticipantAnswer>> GetAllParticipantAnswersAsync(int participantExamId);
        Task<List<CorrectionResult>> AutoCorrectExamAsync(int participantExamId);
    }
}
