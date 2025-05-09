using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.DTOs.RevenueDTOs;

namespace FinancialManagerAPI.DTOs.UserDTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public List<CategoryDto>? Categories { get; set; }
        public List<ExpenseDto>? Expenses { get; set; }
        public List<RevenueDto>? Revenues { get; set; }
        public List<DebtDto>? Debts { get; set; }
    }
}
