using System.ComponentModel.DataAnnotations;

namespace FinancialManagerAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }

        public List<Expense>? Expenses { get; set; }
        public List<Revenue>? Revenues { get; set; }
        public List<Debt>? Debts { get; set; }
    }
}

