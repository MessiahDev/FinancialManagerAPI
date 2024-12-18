namespace FinancialManagerAPI.DTOs.DebtDTOs
{
    public class CreateDebtDto
    {
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Creditor { get; set; }
    }
}
