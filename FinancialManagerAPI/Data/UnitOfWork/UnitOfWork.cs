using FinancialManagerAPI.Data.Repositories;
using FinancialManagerAPI.Models;

namespace FinancialManagerAPI.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;

        private IRepository<User>? _users;
        private IRepository<Expense>? _expenses;
        private IRepository<Debt>? _debts;
        private IRepository<Revenue>? _revenues;
        private IRepository<Category>? _categories;

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Expense> Expenses => _expenses ??= new Repository<Expense>(_context);
        public IRepository<Debt> Debts => _debts ??= new Repository<Debt>(_context);
        public IRepository<Revenue> Revenues => _revenues ??= new Repository<Revenue>(_context);
        public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
            await _context.SaveChangesAsync(cancellationToken);

        public void Dispose() => _context.Dispose();
    }
}
