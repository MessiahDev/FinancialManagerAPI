namespace FinancialManagerAPI.DTOs.ExpenseDTOs
{
    public class UpdateExpenseDto
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? CategoryName { get; set; }
    }
}
