namespace FinancialManagerAPI.DTOs.ExpenseDTOs
{
    public class CreateExpenseDto
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int UserId { get; set; }
    }
}
