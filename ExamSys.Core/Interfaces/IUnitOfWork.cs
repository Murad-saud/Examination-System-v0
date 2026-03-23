using ExamSys.Core.Interfaces.Repositories;

namespace ExamSys.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Your 5 repository properties
        IExamRepository Exams { get; }
        ICourseRepository Courses { get; }
        IQuestionRepository Questions { get; }
        IParticipantExamRepository ParticipantExams { get; }
        IParticipantAnswerRepository ParticipantAnswers { get; }

        // Core transaction methods
        Task<int> SaveChangesAsync();  // Basic save without explicit transaction
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Optional: Helper method for transactional operations
        Task ExecuteInTransactionAsync(Func<Task> operation);
    }
}
