using FinancialManagerAPI.Data.Repositories;
using FinancialManagerAPI.Models;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialManagerAPI.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Expense> Expenses { get; }
        IRepository<Debt> Debts { get; }
        IRepository<Revenue> Revenues { get; }
        IRepository<Category> Categories { get; }
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}

