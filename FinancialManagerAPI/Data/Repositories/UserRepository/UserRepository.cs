using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.DTOs.RevenueDTOs;
using FinancialManagerAPI.DTOs.UserDTOs;
using FinancialManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagerAPI.Data.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<User>();
        }

        public async Task<UserDto?> GetUserWithDetailsAsync(int userId)
        {
            var userDto = await _dbSet
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    Expenses = u.Expenses.Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        Description = e.Description,
                        Amount = e.Amount,
                        Date = e.Date,
                        CategoryId = e.Category != null ? e.Category.Id : 0,
                        CategoryName = e.Category != null ? e.Category.Name : "",
                        UserId = e.UserId
                    }).ToList(),
                    Revenues = u.Revenues.Select(r => new RevenueDto
                    {
                        Id = r.Id,
                        Description = r.Description,
                        Amount = r.Amount,
                        Date = r.Date,
                        UserId = r.UserId
                    }).ToList(),
                    Debts = u.Debts.Select(d => new DebtDto
                    {
                        Id = d.Id,
                        Description = d.Description,
                        Amount = d.Amount,
                        DueDate = d.DueDate,
                        Creditor = d.Creditor,
                        IsPaid = d.IsPaid,
                        UserId = d.UserId
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (userDto == null)
                return null;

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    UserId = c.UserId
                })
                .ToListAsync();

            userDto.Categories = categories;

            return userDto;
        }
    }
}
