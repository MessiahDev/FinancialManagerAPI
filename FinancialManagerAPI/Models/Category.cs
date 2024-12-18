using System.ComponentModel.DataAnnotations;

namespace FinancialManagerAPI.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        public List<Expense>? Expenses { get; set; }
    }
}
