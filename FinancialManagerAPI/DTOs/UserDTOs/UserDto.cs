using FinancialManagerAPI.DTOs.CategoryDTOs;
using FinancialManagerAPI.DTOs.DebtDTOs;
using FinancialManagerAPI.DTOs.ExpenseDTOs;
using FinancialManagerAPI.DTOs.RevenueDTOs;
using FinancialManagerAPI.Models.Enums;

namespace FinancialManagerAPI.DTOs.UserDTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;

        public List<CategoryDto> Categories { get; set; } = new();
        public List<ExpenseDto> Expenses { get; set; } = new();
        public List<RevenueDto> Revenues { get; set; } = new();
        public List<DebtDto> Debts { get; set; } = new();
    }
}
