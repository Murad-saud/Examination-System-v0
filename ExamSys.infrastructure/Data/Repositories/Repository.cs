using ExamSys.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExamSys.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public virtual void Remove(T entity) => _dbSet.Remove(entity);

        //public virtual async Task SaveAsync() => await _context.SaveChangesAsync();

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);
    }
}
