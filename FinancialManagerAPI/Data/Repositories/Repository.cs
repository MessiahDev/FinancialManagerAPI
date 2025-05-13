using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinancialManagerAPI.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int? id) => await _dbSet.FindAsync(id);

        public async Task<T> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<IEnumerable<T>> GetAllByFuncAsync(Expression<Func<T, bool>> predicate) => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public void Add(T entity) => _dbSet.Add(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);

        public async Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }
    }
}
