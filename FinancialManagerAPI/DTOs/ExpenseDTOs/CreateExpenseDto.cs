namespace FinancialManagerAPI.DTOs.ExpenseDTOs
{
    public class CreateExpenseDto
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Category { get; set; }
    }
}
