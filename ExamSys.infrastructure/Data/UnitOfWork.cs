using ExamSys.Core.Interfaces;
using ExamSys.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ExamSys.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        // Repositories
        public IExamRepository Exams { get; }
        public ICourseRepository Courses { get; }
        public IQuestionRepository Questions { get; }
        public IParticipantExamRepository ParticipantExams { get; }
        public IParticipantAnswerRepository ParticipantAnswers { get; }

        public UnitOfWork(
            AppDbContext context,
            IExamRepository examRepository,
            ICourseRepository courseRepository,
            IQuestionRepository questionRepository,
            IParticipantExamRepository participantExamRepository,
            IParticipantAnswerRepository participantAnswers)
        {
            _context = context;
            Exams = examRepository;
            Courses = courseRepository;
            Questions = questionRepository;
            ParticipantExams = participantExamRepository;
            ParticipantAnswers = participantAnswers;
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Helper method for common transaction pattern
        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            await BeginTransactionAsync();
            try
            {
                await operation();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
