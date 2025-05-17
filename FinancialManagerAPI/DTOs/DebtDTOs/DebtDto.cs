namespace FinancialManagerAPI.DTOs.DebtDTOs
{
    public class DebtDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Creditor { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
    }
}
